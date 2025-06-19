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
        },
        new DriverTemplate
        {
            Id = 4,
            Language = LanguageType.Rust,
            Template = """
                use serde::{Deserialize, Serialize};
                use std::{env, fs, time::Instant};

                mod solution;
                use solution::Solution;

                #[derive(Serialize, Deserialize)]
                #[serde(rename_all = "camelCase")]
                struct SubmissionResult {
                    pass: bool,
                    test_case_count: usize,
                    passed_test_cases: usize,
                    failed_test_cases: usize,
                    error_message: String,
                    execution_time_in_ms: u128,
                    test_case_results: Vec<TestCaseResult>,
                }

                #[derive(Serialize, Deserialize)]
                #[serde(rename_all = "camelCase")]
                struct TestCaseResult {
                    test_case_index: usize,
                    pass: bool,
                    error_message: Option<String>,
                    execution_time_in_ms: u128,
                    input: serde_json::Value,
                    expected_output: serde_json::Value,
                    actual_output: serde_json::Value,
                    is_hidden: bool,
                }

                fn main() {
                    let args: Vec<String> = env::args().collect();

                    if args.len() < 4 {
                        eprintln!("Usage: cargo run <inputFilePath> <expectedOutputFilePath> <resultFilePath>");
                        std::process::exit(1);
                    }

                    let input_path = &args[1];
                    let expected_path = &args[2];
                    let result_path = &args[3];

                    let start = Instant::now();

                    let result = match run_test_harness(input_path, expected_path) {
                        Ok(mut r) => {
                            r.execution_time_in_ms = start.elapsed().as_millis();
                            r
                        }
                        Err(e) => SubmissionResult {
                            pass: false,
                            test_case_count: 0,
                            passed_test_cases: 0,
                            failed_test_cases: 0,
                            error_message: e.to_string(),
                            execution_time_in_ms: start.elapsed().as_millis(),
                            test_case_results: vec![],
                        },
                    };

                    let json = serde_json::to_string_pretty(&result).unwrap();
                    fs::write(result_path, json).unwrap();
                }

                fn run_test_harness(
                    input_path: &str,
                    expected_path: &str,
                ) -> Result<SubmissionResult, Box<dyn std::error::Error>> {
                    // EXAMPLE: Replace with your actual test logic
                    let input_json = fs::read_to_string(input_path)?;
                    let expected_json = fs::read_to_string(expected_path)?;

                    let input: Vec<i32> = serde_json::from_str(&input_json)?;
                    let expected: Vec<i32> = serde_json::from_str(&expected_json)?;

                    let test_case_count = input.len() / 2;
                    let mut passed = 0;
                    let mut failed = 0;
                    let mut test_results = vec![];

                    for (i, pair) in input.chunks(2).enumerate() {
                        if pair.len() < 2 {
                            continue;
                        }

                        let a = pair[0];
                        let b = pair[1];

                        let t_start = Instant::now();
                        let result = Solution::add(a, b);
                        let duration = t_start.elapsed().as_millis();

                        let expected_result = expected[i];
                        let did_pass = result == expected_result;

                        if did_pass {
                            passed += 1;
                        } else {
                            failed += 1;
                        }

                        test_results.push(TestCaseResult {
                            test_case_index: i,
                            pass: did_pass,
                            error_message: if did_pass {
                                None
                            } else {
                                Some(format!("Expected {} but got {}", expected_result, result))
                            },
                            execution_time_in_ms: duration,
                            input: serde_json::json!({ "a": a, "b": b }),
                            expected_output: serde_json::json!(expected_result),
                            actual_output: serde_json::json!(result),
                            is_hidden: false,
                        });
                    }

                    Ok(SubmissionResult {
                        pass: passed == test_case_count,
                        test_case_count,
                        passed_test_cases: passed,
                        failed_test_cases: failed,
                        error_message: String::new(),
                        execution_time_in_ms: 0, // filled later
                        test_case_results: test_results,
                    })
                }
                """,
            UITemplate = """
                pub struct Solution;

                impl Solution {
                    pub fn add(a: i32, b: i32) -> i32 { // update the function signature to match your requirements
                        // Your code here
                    }
                }
                """
        }
    ];
}