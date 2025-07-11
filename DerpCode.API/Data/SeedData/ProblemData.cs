using System;
using System.Collections.Generic;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Data.SeedData;

public static class ProblemData
{
    // Remove this once I test the folder sync functionality
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
            ExplanationArticle = new()
            {
                Id = 1,
                Title = "Add Two Numbers Explanation",
                Content = "To add two numbers, you can use the '+' operator in most programming languages. etc",
                Type = ArticleType.ProblemSolution,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Tags = [TagData.Tags[0]],
                UserId = 1,
                LastEditedById = 1
            },
            ExpectedOutput = [6, 10],
            Hints = ["Use the '+' operator to add two numbers."],
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
                    Answer = """
                        using System;

                        public class Solution
                        {
                            public static int Add(int a, int b)
                            {
                                return a + b;
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
                                var testCaseResults = new List<TestCaseResult>();

                                for (int i = 0, j = 0; i < input.Length; i += 2, j++)
                                {
                                    var a = input[i];
                                    var b = input[i + 1];
                                    
                                    var testCaseSw = Stopwatch.StartNew();
                                    var result = Solution.Add(a, b);
                                    testCaseSw.Stop();
                                    
                                    int expected = expectedOutput[j];
                                    bool passed = result == expected;

                                    testCaseResults.Add(new TestCaseResult
                                    {
                                        TestCaseIndex = j,
                                        Pass = passed,
                                        ErrorMessage = passed ? null : $"Expected {expected} but got {result}",
                                        ExecutionTimeInMs = (int)testCaseSw.ElapsedMilliseconds,
                                        Input = new { a, b },
                                        ExpectedOutput = expected,
                                        ActualOutput = result,
                                        IsHidden = false
                                    });

                                    if (passed)
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
                                    Pass = passedTestCases == testCaseCount,
                                    TestCaseResults = testCaseResults
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
                    Answer = """
                        export function add(a, b) {
                            return a + b;
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
                            const input = JSON.parse(inputJsonStr);         // array of integers
                            const expectedOutput = JSON.parse(expectedOutputJsonStr); // array of integers
                            const testCaseCount = Math.floor(input.length / 2);
                            let passedTestCases = 0;
                            let failedTestCases = 0;
                            const testCaseResults = [];

                            for (let i = 0, j = 0; i < input.length; i += 2, j++) {
                                const a = input[i];
                                const b = input[i + 1];
                                
                                const testCaseStart = Date.now();
                                const result = add(a, b);           // your test function
                                const testCaseEnd = Date.now();
                                
                                const expected = expectedOutput[j];
                                const passed = result === expected;

                                testCaseResults.push({
                                    testCaseIndex: j,
                                    pass: passed,
                                    errorMessage: passed ? null : `Expected ${expected} but got ${result}`,
                                    executionTimeInMs: testCaseEnd - testCaseStart,
                                    input: { a, b },
                                    expectedOutput: expected,
                                    actualOutput: result,
                                    isHidden: false
                                });

                                if (passed) {
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
                                testCaseResults
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
                    Answer = """
                        export function add(a: number, b: number): number {
                            return a + b;
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
                          const input = JSON.parse(inputJsonStr);
                          const expectedOutput = JSON.parse(expectedOutputJsonStr);
                          const testCaseCount = Math.floor(input.length / 2);
                          let passedTestCases = 0;
                          let failedTestCases = 0;
                          const testCaseResults: TestCaseResult[] = [];

                          for (let i = 0, j = 0; i < input.length; i += 2, j++) {
                            const a = input[i];
                            const b = input[i + 1];
                            
                            const testCaseStart = Date.now();
                            const result = add(a, b);
                            const testCaseEnd = Date.now();
                            
                            const expected = expectedOutput[j];
                            const passed = result === expected;

                            testCaseResults.push({
                              testCaseIndex: j,
                              pass: passed,
                              errorMessage: passed ? null : `Expected ${expected} but got ${result}`,
                              executionTimeInMs: testCaseEnd - testCaseStart,
                              input: { a, b },
                              expectedOutput: expected,
                              actualOutput: result,
                              isHidden: false
                            });

                            if (passed) {
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
                            executionTimeInMs: 0,
                            testCaseResults
                          };
                        }

                        main();
                        """
                },
                new()
                {
                    Id = 4,
                    Language = LanguageType.Rust,
                    Image = "code-executor-rust",
                    UITemplate = """
                        pub struct Solution;

                        impl Solution {
                            pub fn add(a: i32, b: i32) -> i32 {
                                // Your code here
                            }
                        }
                        """,
                    Answer = """
                        pub struct Solution;

                        impl Solution {
                            pub fn add(a: i32, b: i32) -> i32 {
                                a + b
                            }
                        }
                        """,
                    DriverCode = """
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
            ExplanationArticle = new()
            {
                Id = 2,
                Title = "FizzBuzz Explanation",
                Content = "Use the modulo operator to check divisibility. etc",
                Type = ArticleType.ProblemSolution,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Tags = [TagData.Tags[0]],
                UserId = 1,
                LastEditedById = 1
            },
            Hints = ["Use the modulo operator '%' to check divisibility."],
            ExpectedOutput = ["buzz", "", "fizz", "", "fizzbuzz"],
            Drivers =
            [
                new()
                {
                    Id = 5,
                    Language = LanguageType.TypeScript,
                    Image = "code-executor-typescript",
                    UITemplate = """
                        export function fizzBuzz(a: number): string {
                            // Your code here
                        }
                        """,
                    Answer = """
                        export function fizzBuzz(a: number): string {
                            if (a % 3 === 0 && a % 5 === 0) {
                                return "fizzbuzz";
                            } else if (a % 3 === 0) {
                                return "fizz";
                            } else if (a % 5 === 0) {
                                return "buzz";
                            } else {
                                return "";
                            }
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
                          const input: number[] = JSON.parse(inputJsonStr);
                          const expectedOutput: string[] = JSON.parse(expectedOutputJsonStr);
                          const testCaseCount = input.length;
                          let passedTestCases = 0;
                          let failedTestCases = 0;
                          const testCaseResults: TestCaseResult[] = [];

                          for (let i = 0; i < input.length; i++) {
                            const a = input[i];
                            
                            const testCaseStart = Date.now();
                            const result = fizzBuzz(a);
                            const testCaseEnd = Date.now();
                            
                            const expected = expectedOutput[i];
                            const passed = result === expected;

                            testCaseResults.push({
                              testCaseIndex: i,
                              pass: passed,
                              errorMessage: passed ? null : `Expected "${expected}" but got "${result}"`,
                              executionTimeInMs: testCaseEnd - testCaseStart,
                              input: a,
                              expectedOutput: expected,
                              actualOutput: result,
                              isHidden: false
                            });

                            if (passed) {
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
                            executionTimeInMs: 0,
                            testCaseResults
                          };
                        }

                        main();
                        """
                }
            ]
        }
    ];
}