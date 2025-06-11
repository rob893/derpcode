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
                        public List<TestCaseResult> TestCaseResults { get; set; } = new List<TestCaseResult>();
                    }

                    private class TestCaseResult
                    {
                        public int TestCaseIndex { get; set; }
                        public bool Pass { get; set; }
                        public string? ErrorMessage { get; set; }
                        public int ExecutionTimeInMs { get; set; }
                        public object Input { get; set; } = default!;
                        public object ExpectedOutput { get; set; } = default!;
                        public object ActualOutput { get; set; } = default!;
                        public bool IsHidden { get; set; }
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
                        // Example implementation:
                        var testCaseResults = new List<TestCaseResult>();
                        
                        // TODO: Implement your test logic and populate testCaseResults
                        // Each test case should create a TestCaseResult with detailed information
                        // Example for measuring execution time:
                        /*
                        for (int i = 0; i < testCaseCount; i++)
                        {
                            var testCaseSw = System.Diagnostics.Stopwatch.StartNew();
                            var result = YourTestFunction(input[i]);
                            testCaseSw.Stop();
                            
                            testCaseResults.Add(new TestCaseResult
                            {
                                TestCaseIndex = i,
                                Pass = result == expected[i],
                                ErrorMessage = passed ? null : $"Expected {expected[i]} but got {result}",
                                ExecutionTimeInMs = (int)testCaseSw.ElapsedMilliseconds,
                                Input = input[i],
                                ExpectedOutput = expected[i],
                                ActualOutput = result,
                                IsHidden = false
                            });
                        }
                        */
                        
                        throw new NotImplementedException("Implement your test logic here and populate TestCaseResults.");
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
                        executionTimeInMs: 0,
                        testCaseResults: []
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
                        result.testCaseResults = testResults.testCaseResults;
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

                    // TODO: Implement your test logic here and populate testCaseResults
                    // Each test case should create an object with detailed information
                    const testCaseResults = [];

                    // Example for measuring execution time:
                    /*
                    for (let i = 0; i < input.length; i++) {
                        const testCaseStart = Date.now();
                        const result = yourTestFunction(input[i]);
                        const testCaseEnd = Date.now();
                        
                        testCaseResults.push({
                            testCaseIndex: i,
                            pass: result === expected[i],
                            errorMessage: passed ? null : `Expected ${expected[i]} but got ${result}`,
                            executionTimeInMs: testCaseEnd - testCaseStart,
                            input: input[i],
                            expectedOutput: expected[i],
                            actualOutput: result,
                            isHidden: false
                        });
                    }
                    */

                    // Implement your test logic here
                    throw new Error('Implement your test logic here and populate testCaseResults.');

                    // return example:
                    return {
                        pass: true,
                        testCaseCount: 1,
                        passedTestCases: 1,
                        failedTestCases: 0,
                        testCaseResults
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
                    testCaseResults: TestCaseResult[];
                }

                interface TestCaseResult {
                    testCaseIndex: number;
                    pass: boolean;
                    errorMessage: string | null;
                    executionTimeInMs: number;
                    input: any;
                    expectedOutput: any;
                    actualOutput: any;
                    isHidden: boolean;
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
                        executionTimeInMs: 0,
                        testCaseResults: []
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
                        result.testCaseResults = testResults.testCaseResults;
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

                    // TODO: Implement your test logic here and populate testCaseResults
                    // Each test case should create a TestCaseResult with detailed information
                    const testCaseResults: TestCaseResult[] = [];

                    // Example for measuring execution time:
                    /*
                    for (let i = 0; i < input.length; i++) {
                        const testCaseStart = Date.now();
                        const result = yourTestFunction(input[i]);
                        const testCaseEnd = Date.now();
                        
                        testCaseResults.push({
                            testCaseIndex: i,
                            pass: result === expected[i],
                            errorMessage: passed ? null : `Expected ${expected[i]} but got ${result}`,
                            executionTimeInMs: testCaseEnd - testCaseStart,
                            input: input[i],
                            expectedOutput: expected[i],
                            actualOutput: result,
                            isHidden: false
                        });
                    }
                    */

                    // Implement your test logic here
                    throw new Error('Implement your test logic here and populate testCaseResults.');

                    // return example:
                    return {
                        pass: true,
                        testCaseCount: 1,
                        passedTestCases: 1,
                        failedTestCases: 0,
                        errorMessage: '',
                        executionTimeInMs: 0,
                        testCaseResults
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