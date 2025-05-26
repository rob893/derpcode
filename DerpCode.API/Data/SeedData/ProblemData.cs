using System.Collections.Generic;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Data.SeedData;

public static class ProblemData
{
    public static readonly List<Problem> Problems =
    [
        new Problem
        {
            Id = 1,
            Name = "Add Two Numbers",
            Input = [5, 1, 6, 4],
            Tags = [TagData.Tags[0]],
            Description = "Given two numbers, return their sum.",
            Difficulty = ProblemDifficulty.VeryEasy,
            ExpectedOutput = [6, 10],
            Drivers =
            [
                new()
                {
                    Id = 1,
                    Language = LanguageType.CSharp,
                    Image = "code-executor-csharp",
                    UITemplate = """
                        using System;

                        public class Solution
                        {
                            public static int Add(int a, int b)
                            {
                                // Your code here
                            }
                        }
                        """,
                    DriverCode = """
                        using System;
                        using System.Diagnostics;
                        using System.IO;
                        using System.Text.Json;

                        public class Program
                        {
                            private class SubmissionResult
                            {
                                public bool Pass { get; set; }
                                public int TestCaseCount { get; set; }
                                public int PassedTestCases { get; set; }
                                public int FailedTestCases { get; set; }
                                public string ErrorMessage { get; set; } = string.Empty;
                                public long ExecutionTimeInMs { get; set; }
                            }

                            public static void Main(string[] args)
                            {
                                if (args.Length < 3)
                                {
                                    Console.Error.WriteLine("Usage: dotnet run <inputFilePath> <expectedOutputFilePath> <resultFilePath>");
                                    Environment.Exit(1);
                                }

                                string inputPath = args[0];
                                string expectedPath = args[1];
                                string resultPath = args[2];

                                try
                                {
                                    string input = File.ReadAllText(inputPath);
                                    string expectedOutput = File.ReadAllText(expectedPath);
                                    var sw = Stopwatch.StartNew();

                                    var results = RunTests(input, expectedOutput);
                                    results.ExecutionTimeInMs = sw.ElapsedMilliseconds;

                                    var asJson = JsonSerializer.Serialize(results, new JsonSerializerOptions
                                    {
                                        WriteIndented = true,
                                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                    });

                                    File.WriteAllText(resultPath, asJson);
                                }
                                catch (Exception ex)
                                {
                                    Console.Error.WriteLine($"Error reading files: {ex.Message}");

                                    var results = new SubmissionResult
                                    {
                                        ErrorMessage = ex.Message,
                                    };
                                    var asJson = JsonSerializer.Serialize(results, new JsonSerializerOptions
                                    {
                                        WriteIndented = true,
                                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                    });

                                    File.WriteAllText(resultPath, asJson);
                                    Environment.Exit(1);
                                }
                            }

                            private static SubmissionResult RunTests(string inputJsonStr, string expectedOutputJsonStr)
                            {
                                var input = JsonSerializer.Deserialize<int[]>(inputJsonStr);
                                var expectedOutput = JsonSerializer.Deserialize<int[]>(expectedOutputJsonStr);
                                int testCaseCount = input.Length / 2;
                                int passedTestCases = 0;
                                int failedTestCases = 0;

                                for (int i = 0, j = 0; i < input.Length; i += 2, j++)
                                {
                                    var a = input[i];
                                    var b = input[i + 1];
                                    var result = Solution.Add(a, b);
                                    int expected = expectedOutput[j];

                                    if (result == expected)
                                    {
                                        passedTestCases++;
                                    }
                                    else
                                    {
                                        failedTestCases++;
                                    }
                                }

                                return new SubmissionResult
                                {
                                    TestCaseCount = testCaseCount,
                                    PassedTestCases = passedTestCases,
                                    FailedTestCases = failedTestCases,
                                    Pass = passedTestCases == testCaseCount
                                };
                            }
                        }
                        """
                },
                new()
                {
                    Id = 2,
                    Language = LanguageType.JavaScript,
                    Image = "code-executor-javascript",
                    UITemplate = """
                        export function add(a, b) {
                            // Your code here
                        }
                        """,
                    DriverCode = """
                        import fs from 'fs';
                        import path from 'path';
                        import { add } from './solution.js'

                        function main() {
                            const args = process.argv.slice(2);

                            if (args.length < 3) {
                                console.error('Usage: node index.js <inputFilePath> <expectedOutputFilePath> <resultFilePath>');
                                process.exit(1);
                            }

                            const [inputPath, expectedPath, resultPath] = args;

                            const result = {
                                pass: false,
                                testCaseCount: 0,
                                passedTestCases: 0,
                                failedTestCases: 0,
                                errorMessage: '',
                                executionTimeInMs: 0
                            };

                            try {
                                const input = fs.readFileSync(inputPath, 'utf8');
                                const expectedOutput = fs.readFileSync(expectedPath, 'utf8');

                                const start = Date.now();

                                const testResults = runTests(input, expectedOutput);

                                result.pass = testResults.pass;
                                result.testCaseCount = testResults.testCaseCount;
                                result.passedTestCases = testResults.passedTestCases;
                                result.failedTestCases = testResults.failedTestCases;
                                result.executionTimeInMs = Date.now() - start;

                            } catch (err) {
                                console.error('Error reading files:' + err.message);
                                result.errorMessage = err.message;
                            }

                            fs.writeFileSync(resultPath, JSON.stringify(result, null, 2));
                        }

                        function runTests(inputJsonStr, expectedOutputJsonStr) {
                            const input = JSON.parse(inputJsonStr);         // array of integers
                            const expectedOutput = JSON.parse(expectedOutputJsonStr); // array of integers
                            const testCaseCount = Math.floor(input.length / 2);
                            let passedTestCases = 0;
                            let failedTestCases = 0;

                            for (let i = 0, j = 0; i < input.length; i += 2, j++) {
                                const a = input[i];
                                const b = input[i + 1];
                                const result = add(a, b);           // your test function
                                const expected = expectedOutput[j];

                                if (result === expected) {
                                    passedTestCases++;
                                } else {
                                    failedTestCases++;
                                }
                            }

                            return {
                                testCaseCount,
                                passedTestCases,
                                failedTestCases,
                                pass: passedTestCases === testCaseCount
                            };
                        }

                        main();
                        """
                },
                new()
                {
                    Id = 3,
                    Language = LanguageType.TypeScript,
                    Image = "code-executor-typescript",
                    UITemplate = """
                        export function add(a: number, b: number): number {
                            // Your code here
                        }
                        """,
                    DriverCode = """
                        import fs from 'fs';
                        import { add } from './solution';

                        interface SubmissionResult {
                          pass: boolean;
                          testCaseCount: number;
                          passedTestCases: number;
                          failedTestCases: number;
                          errorMessage: string;
                          executionTimeInMs: number;
                        }

                        function main(): void {
                          const args = process.argv.slice(2);

                          if (args.length < 3) {
                            console.error('Usage: node index.js <inputFilePath> <expectedOutputFilePath> <resultFilePath>');
                            process.exit(1);
                          }

                          const [inputPath, expectedPath, resultPath] = args;

                          const result: SubmissionResult = {
                            pass: false,
                            testCaseCount: 0,
                            passedTestCases: 0,
                            failedTestCases: 0,
                            errorMessage: '',
                            executionTimeInMs: 0
                          };

                          try {
                            const input = fs.readFileSync(inputPath, 'utf8');
                            const expectedOutput = fs.readFileSync(expectedPath, 'utf8');

                            const start = Date.now();

                            const testResults = runTests(input, expectedOutput);

                            result.pass = testResults.pass;
                            result.testCaseCount = testResults.testCaseCount;
                            result.passedTestCases = testResults.passedTestCases;
                            result.failedTestCases = testResults.failedTestCases;
                            result.executionTimeInMs = Date.now() - start;
                          } catch (err: any) {
                            console.error('Error reading files:' + err.message);
                            result.errorMessage = err.message;
                          }

                          fs.writeFileSync(resultPath, JSON.stringify(result, null, 2));
                        }

                        function runTests(inputJsonStr: string, expectedOutputJsonStr: string): SubmissionResult {
                          const input = JSON.parse(inputJsonStr);
                          const expectedOutput = JSON.parse(expectedOutputJsonStr);
                          const testCaseCount = Math.floor(input.length / 2);
                          let passedTestCases = 0;
                          let failedTestCases = 0;

                          for (let i = 0, j = 0; i < input.length; i += 2, j++) {
                            const a = input[i];
                            const b = input[i + 1];
                            const result = add(a, b);
                            const expected = expectedOutput[j];

                            if (result === expected) {
                              passedTestCases++;
                            } else {
                              failedTestCases++;
                            }
                          }

                          return {
                            testCaseCount,
                            passedTestCases,
                            failedTestCases,
                            pass: passedTestCases === testCaseCount,
                            errorMessage: '',
                            executionTimeInMs: 0
                          };
                        }

                        main();
                        """
                }
            ]
        },
        new Problem
        {
            Id = 2,
            Name = "FizzBuzz",
            Tags = [TagData.Tags[0]],
            Description = "Given a number, return \"fizz\" if it is divisible by 3, \"buzz\" if it is divisible by 5, and \"fizzbuzz\" if it is divisible by both.",
            Difficulty = ProblemDifficulty.Easy,
            Input = [5, 1, 3, 4, 15],
            ExpectedOutput = ["buzz", "", "fizz", "", "fizzbuzz"],
            Drivers =
            [
                new()
                {
                    Id = 4,
                    Language = LanguageType.TypeScript,
                    Image = "code-executor-typescript",
                    UITemplate = """
                        export function fizzBuzz(a: number): string {
                            // Your code here
                        }
                        """,
                    DriverCode = """
                        import fs from 'fs';
                        import { fizzBuzz } from './solution';

                        interface SubmissionResult {
                          pass: boolean;
                          testCaseCount: number;
                          passedTestCases: number;
                          failedTestCases: number;
                          errorMessage: string;
                          executionTimeInMs: number;
                        }

                        function main(): void {
                          const args = process.argv.slice(2);

                          if (args.length < 3) {
                            console.error('Usage: node index.js <inputFilePath> <expectedOutputFilePath> <resultFilePath>');
                            process.exit(1);
                          }

                          const [inputPath, expectedPath, resultPath] = args;

                          const result: SubmissionResult = {
                            pass: false,
                            testCaseCount: 0,
                            passedTestCases: 0,
                            failedTestCases: 0,
                            errorMessage: '',
                            executionTimeInMs: 0
                          };

                          try {
                            const input = fs.readFileSync(inputPath, 'utf8');
                            const expectedOutput = fs.readFileSync(expectedPath, 'utf8');

                            const start = Date.now();

                            const testResults = runTests(input, expectedOutput);

                            result.pass = testResults.pass;
                            result.testCaseCount = testResults.testCaseCount;
                            result.passedTestCases = testResults.passedTestCases;
                            result.failedTestCases = testResults.failedTestCases;
                            result.executionTimeInMs = Date.now() - start;
                          } catch (err: any) {
                            console.error('Error reading files:' + err.message);
                            result.errorMessage = err.message;
                          }

                          fs.writeFileSync(resultPath, JSON.stringify(result, null, 2));
                        }

                        function runTests(inputJsonStr: string, expectedOutputJsonStr: string): SubmissionResult {
                          const input: number[] = JSON.parse(inputJsonStr);
                          const expectedOutput: string[] = JSON.parse(expectedOutputJsonStr);
                          const testCaseCount = input.length;
                          let passedTestCases = 0;
                          let failedTestCases = 0;

                          for (let i = 0; i < input.length; i++) {
                            const a = input[i];
                            const result = fizzBuzz(a);
                            const expected = expectedOutput[i];

                            if (result === expected) {
                              passedTestCases++;
                            } else {
                              failedTestCases++;
                            }
                          }

                          return {
                            testCaseCount,
                            passedTestCases,
                            failedTestCases,
                            pass: passedTestCases === testCaseCount,
                            errorMessage: '',
                            executionTimeInMs: 0
                          };
                        }

                        main();
                        """
                }
            ]
        }
    ];
}