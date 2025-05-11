import Docker from 'dockerode';
import fs from 'fs';
import os from 'os';
import path from 'path';
import { Language, Problem, SubmissionResult } from './models';

const docker = new Docker(); // uses /var/run/docker.sock by default

export async function runWorker(userCode: string, language: Language, problem: Problem): Promise<SubmissionResult> {
  const driver = problem.drivers.find(d => d.language === language);

  if (!driver) {
    throw new Error(`No driver found for language: ${language}`);
  }

  const { image, driverCode } = driver;
  const { input, expectedOutput } = problem;

  const tempDir = fs.mkdtempSync(path.join(os.tmpdir(), 'submission_'));
  const userCodePath = path.join(tempDir, 'UserCode.txt');
  const driverCodePath = path.join(tempDir, 'DriverCode.txt');
  const inputPath = path.join(tempDir, 'input.json');
  const expectedOutputPath = path.join(tempDir, 'expectedOutput.json');
  const resultsPath = path.join(tempDir, 'results.json');

  fs.writeFileSync(userCodePath, userCode);
  fs.writeFileSync(driverCodePath, driverCode);
  fs.writeFileSync(inputPath, JSON.stringify(input));
  fs.writeFileSync(expectedOutputPath, JSON.stringify(expectedOutput));

  try {
    const container = await docker.createContainer({
      Image: image,
      Tty: false,
      HostConfig: {
        Binds: [`${tempDir}:/home/runner/submission`],
        NetworkMode: 'none',
        Memory: 512 * 1024 * 1024,
        NanoCpus: 500_000_000,
        AutoRemove: true
      }
    });

    await container.start();
    // Stream logs to the console
    container.logs(
      {
        follow: true,
        stdout: true,
        stderr: true
      },
      (err, stream) => {
        if (err || !stream) {
          console.error('Error streaming logs:', err);
          return;
        }

        stream.on('data', (data: Buffer) => {
          console.log(data.toString());
        });
      }
    );

    // Wait for the container to finish
    await container.wait();

    const outputPath = path.join(tempDir, 'output.txt');
    const errorPath = path.join(tempDir, 'error.txt');

    const output = fs.existsSync(outputPath) ? fs.readFileSync(outputPath, 'utf8') : '';
    const error = fs.existsSync(errorPath) ? fs.readFileSync(errorPath, 'utf8') : '';
    const results = fs.existsSync(resultsPath) ? fs.readFileSync(resultsPath, 'utf8') : '';

    console.log(`=== Output ===\n${output}`);
    console.log(`=== Errors ===\n${error}`);

    if (error) {
      return {
        pass: false,
        errorMessage: `${error}\n${output}`,
        executionTimeInMs: -1,
        failedTestCases: -1,
        passedTestCases: -1,
        testCaseCount: -1
      };
    }

    const result: SubmissionResult = JSON.parse(results);

    return result;
  } catch (err: any) {
    console.error('Execution error:', err);
    return {
      pass: false,
      errorMessage: err.message,
      executionTimeInMs: -1,
      failedTestCases: -1,
      passedTestCases: -1,
      testCaseCount: -1
    };
  } finally {
    fs.rmSync(tempDir, { recursive: true, force: true });
  }
}
