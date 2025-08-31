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
                #pragma warning disable CS8602
                using System;
                using System.Collections.Generic;
                using System.Text.Json;
                using DerpCode.Driver.Base;

                namespace DerpCode.Driver.NewProblem
                {
                    public class NewProblemDriver : BaseProblemDriver
                    {
                        public override List<TestCase> ParseTestCases(object input, object expectedOutput)
                        {
                            throw new NotImplementedException();
                        }

                        public override object? ExecuteTestCase(TestCase testCase, int index)
                        {
                            throw new NotImplementedException();
                        }

                        
                        public override bool CompareResults(object? actual, object expected)
                        {
                            return object.Equals(actual, expected);
                        }

                        public override string FormatErrorMessage(object? actual, object expected)
                        {
                            return $"Expected {expected} but got {actual}";
                        }
                    }

                    public class Program
                    {
                        public static void Main(string[] args)
                        {
                            // Create and run the driver
                            var driver = new BaseDriver(new NewProblemDriver());
                            driver.Run(args);
                        }
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
                import { BaseDriver, IProblemDriver } from './base-driver.js';
                import { add } from './solution.js'; // update import to match your solution function

                /**
                 * Problem-specific driver for NewProblem.
                 */
                class NewProblemDriver extends IProblemDriver {
                    /**
                     * Parse input and expected output into test cases.
                     * TODO: Implement parsing logic for your specific problem.
                     */
                    parseTestCases(input, expectedOutput) {
                        const testCases = [];
                        
                        for (let i = 0; i < input.length; i++) {
                            testCases.push({
                                input: input[i],
                                expectedOutput: expectedOutput[i]
                            });
                        }
                        
                        return testCases;
                    }

                    /**
                     * Execute the solution function with the test case input.
                     * TODO: Update to call your specific solution function.
                     */
                    executeTestCase(testCase) {
                        // Example: return add(testCase.input.a, testCase.input.b);
                        throw new Error('Implement executeTestCase method');
                    }

                    /**
                     * Compare actual and expected results.
                     * TODO: Implement comparison logic for your specific problem.
                     */
                    compareResults(actual, expected) {
                        return actual === expected; // Update comparison logic as needed
                    }

                    /**
                     * Format error message for failed tests.
                     */
                    formatErrorMessage(actual, expected) {
                        return `Expected ${expected} but got ${actual}`;
                    }
                }

                // Create and run the driver
                const driver = new BaseDriver(new NewProblemDriver());
                driver.run();
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
                import { ProblemDriverBase, BaseDriver, TestCase } from './base-driver';
                import { add } from './solution'; // update import to match your solution function

                /**
                 * Problem-specific driver for NewProblem.
                 */
                class NewProblemDriver extends ProblemDriverBase {
                    /**
                     * Parse input and expected output into test cases.
                     * TODO: Implement parsing logic for your specific problem.
                     */
                    parseTestCases(input: any, expectedOutput: any): TestCase[] {
                        const testCases: TestCase[] = [];
                        
                        // Example: simple array parsing (adjust for your problem)
                        for (let i = 0; i < input.length; i++) {
                            testCases.push({
                                input: input[i],
                                expectedOutput: expectedOutput[i]
                            });
                        }
                        
                        return testCases;
                    }

                    /**
                     * Execute the solution function with the test case input.
                     * TODO: Implement execution logic for your specific problem.
                     */
                    executeTestCase(testCase: TestCase, index: number): any {
                        // Example: call your solution function
                        return add(testCase.input); // update to match your solution function call
                    }

                    /**
                     * Compare results using simple equality.
                     * TODO: Customize comparison logic if needed.
                     */
                    compareResults(actual: any, expected: any): boolean {
                        return actual === expected;
                    }
                }

                // Create and run the driver
                const driver = new BaseDriver(new NewProblemDriver());
                driver.run();
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

                        println!("|derpcode-start-test-{}|", i);
                        let t_start = Instant::now();
                        let result = Solution::add(a, b);
                        let duration = t_start.elapsed().as_millis();
                        println!("|derpcode-end-test-{}|", i);

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
        },
        new DriverTemplate
        {
            Id = 5,
            Language = LanguageType.Python,
            Template = """
            from base_driver import BaseDriver, IProblemDriver
            from solution import Solution

            class NewProblemDriver(IProblemDriver):
                \"\"\"Problem-specific driver for NewProblem.\"\"\"
                
                def parse_test_cases(self, input_data, expected_output):
                    test_cases = []
                    
                    # TODO: Implement parsing logic for your specific problem
                    for i in range(len(input_data)):
                        # Example: test_cases.append({'input': input_data[i], 'expectedOutput': expected_output[i]})
                        raise NotImplementedError("Implement parse_test_cases method")
                    
                    return test_cases
                
                def execute_test_case(self, test_case, index):
                    # TODO: Execute your solution function with the test case input
                    # Example: return Solution.add(test_case['input']['a'], test_case['input']['b'])
                    raise NotImplementedError("Implement execute_test_case method")
                
                def compare_results(self, actual, expected):
                    return actual == expected  # Update comparison logic as needed
                
                def format_error_message(self, actual, expected):
                    return f"Expected {expected} but got {actual}"

            if __name__ == "__main__":
                # Create and run the driver
                driver = BaseDriver(NewProblemDriver())
                driver.run()
            """,
            UITemplate = """
            def add(a, b):  # update the function signature to match your requirements
                # Your code here
                pass
            """
        },
        new DriverTemplate
        {
            Id = 6,
            Language = LanguageType.Java,
            Template = """
            import java.util.*;
            import com.google.gson.JsonArray;
            import com.google.gson.JsonElement;
            import com.google.gson.JsonParser;

            /**
             * Problem-specific driver for NewProblem.
             */
            class NewProblemDriver extends ProblemDriverBase {
                @Override
                public List<TestCase> parseTestCases(JsonArray input, JsonArray expectedOutput) {
                    List<TestCase> testCases = new ArrayList<>();
                    
                    // TODO: Implement parsing logic for your specific problem
                    for (int i = 0; i < input.size(); i++) {
                        // Example: testCases.add(new TestCase(input.get(i), expectedOutput.get(i)));
                        throw new UnsupportedOperationException("Implement parseTestCases method");
                    }
                    
                    return testCases;
                }

                @Override
                public Object executeTestCase(TestCase testCase, int index) throws Exception {
                    // TODO: Execute your solution function with the test case input
                    // Example: return Solution.add((Integer) testCase.getInput(), ...);
                    throw new UnsupportedOperationException("Implement executeTestCase method");
                }

                @Override
                public boolean compareResults(Object actual, Object expected) {
                    return Objects.equals(actual, expected); // Update comparison logic as needed
                }
            }

            class Program {
                public static void main(String[] args) {
                    // Create and run the driver
                    BaseDriver driver = new BaseDriver(new NewProblemDriver());
                    driver.run(args);
                }
            }
            """,
            UITemplate = """
            public class Solution {
                public static int add(int a, int b) { // update the function signature to match your requirements
                    // Your code here
                    return 0;
                }
            }
            """
        }
    ];
}