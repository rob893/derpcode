using System.Collections.Generic;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Data.SeedData;

public static class DriverTemplateData
{
    public static readonly List<DriverTemplate> Templates =
    [
        new DriverTemplate
        {
            Id = 1,
            Language = LanguageType.CSharp,
            Template = """
                using System;
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
                            var sw = System.Diagnostics.Stopwatch.StartNew();

                            var results = RunTests(input, expectedOutput);
                            results.ExecutionTimeInMs = sw.ElapsedMilliseconds;

                            string asJson = JsonSerializer.Serialize(results, new JsonSerializerOptions
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
                            string asJson = JsonSerializer.Serialize(results, new JsonSerializerOptions
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
                        // parse input and expected output json objects and implement your test logic here
                        throw new NotImplementedException("Implement your test logic here.");
                    }
                }
                """,
            UITemplate = """
                using System;

                public class Solution
                {
                    public static int Add(int a, int b) // update the function signature to match your requirements
                    {
                        // Your code here
                    }
                }
                """
        },
        new DriverTemplate
        {
            Id = 2,
            Language = LanguageType.JavaScript,
            Template = """
                import fs from 'fs';
                import path from 'path';

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
                    // Example logic: parse and compare
                    const input = JSON.parse(inputJsonStr);
                    const expected = JSON.parse(expectedOutputJsonStr);

                    // Implement your test logic here
                    throw new Error('Implement your test logic here.');

                    // return example:
                    return {
                        pass: true,
                        testCaseCount: 1,
                        passedTestCases: 1,
                        failedTestCases: 0
                    };
                }

                main();
                """,
            UITemplate = """
                export function add(a, b) { // update the function signature to match your requirements
                    // Your code here
                }
                """
        },
        new DriverTemplate
        {
            Id = 3,
            Language = LanguageType.TypeScript,
            Template = """
                import fs from 'fs';
    
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
                        console.error('Usage: ts-node index.ts <inputFilePath> <expectedOutputFilePath> <resultFilePath>');
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
                    // Example logic: parse and compare
                    const input = JSON.parse(inputJsonStr);
                    const expected = JSON.parse(expectedOutputJsonStr);

                    // Implement your test logic here
                    throw new Error('Implement your test logic here.');

                    // return example:
                    return {
                        pass: true,
                        testCaseCount: 1,
                        passedTestCases: 1,
                        failedTestCases: 0,
                        errorMessage: '',
                        executionTimeInMs: 0
                    };
                }

                main();
                """,
            UITemplate = """
                export function add(a: number, b: number): number { // update the function signature to match your requirements
                    // Your code here
                }
                """
        }
    ];
}
