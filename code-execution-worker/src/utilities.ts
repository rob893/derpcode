import Docker from 'dockerode';
import fs from 'fs';
import os from 'os';
import path from 'path';
import { SubmissionResult } from './models';

const docker = new Docker(); // uses /var/run/docker.sock by default

export async function runWorker(
  userCode: string,
  driverCode: string,
  input: string,
  imageName: string,
  expectedOutput: string
): Promise<SubmissionResult> {
  const tempDir = fs.mkdtempSync(path.join(os.tmpdir(), 'submission_'));
  const userCodePath = path.join(tempDir, 'UserCode.txt');
  const driverCodePath = path.join(tempDir, 'DriverCode.txt');
  const inputPath = path.join(tempDir, 'input.txt');

  fs.writeFileSync(userCodePath, userCode);
  fs.writeFileSync(driverCodePath, driverCode);
  fs.writeFileSync(inputPath, input);

  try {
    const container = await docker.createContainer({
      Image: imageName,
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

    console.log(`=== Output ===\n${output}`);
    console.log(`=== Errors ===\n${error}`);

    if (output.trim() === expectedOutput) {
      console.log('Test passed!');
      console.log('Expected:', expectedOutput);
      console.log('Got:', output.trim());
      return { pass: true };
    } else {
      console.log('Test failed!');
      console.log('Expected:', expectedOutput);
      console.log('Got:', output.trim());
      return { pass: false };
    }
  } catch (err: any) {
    console.error('Execution error:', err);
    return { pass: false };
  } finally {
    fs.rmSync(tempDir, { recursive: true, force: true });
    console.log(`Temporary files are located at: ${tempDir}`);
  }
}
