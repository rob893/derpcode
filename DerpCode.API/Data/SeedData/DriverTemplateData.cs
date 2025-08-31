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
                use serde_json::Value;

                mod base_driver;
                use base_driver::{BaseDriver, ProblemDriver, TestCase};

                mod solution;
                use solution::Solution;

                /// Problem-specific driver for NewProblem.
                pub struct NewProblemDriver;

                impl ProblemDriver for NewProblemDriver {
                    /// Parse input and expected output into test cases.
                    /// TODO: Implement parsing logic for your specific problem.
                    fn parse_test_cases(&self, input: &Value, expected_output: &Value) -> Vec<TestCase> {
                        let _input_array = input.as_array().expect("Input should be an array");
                        let _expected_array = expected_output.as_array().expect("Expected output should be an array");
                        
                        // TODO: Implement parsing logic
                        // Example for pairs of integers:
                        // let mut test_cases = Vec::new();
                        // for i in (0..input_array.len()).step_by(2) {
                        //     if i + 1 < input_array.len() {
                        //         let a = input_array[i].as_i64().expect("Input should be integer") as i32;
                        //         let b = input_array[i + 1].as_i64().expect("Input should be integer") as i32;
                        //         let expected = expected_array[i / 2].clone();
                        //         test_cases.push(TestCase {
                        //             input: serde_json::json!({ "a": a, "b": b }),
                        //             expected_output: expected,
                        //         });
                        //     }
                        // }
                        // test_cases
                        
                        panic!("Implement parse_test_cases method");
                    }

                    /// Execute the solution function with the test case input.
                    /// TODO: Update to call your specific solution function.
                    fn execute_test_case(&self, _test_case: &TestCase, _index: usize) -> Result<Value, Box<dyn std::error::Error>> {
                        // TODO: Execute your solution function
                        // Example: 
                        // let a = test_case.input["a"].as_i64().ok_or("Missing 'a' input")? as i32;
                        // let b = test_case.input["b"].as_i64().ok_or("Missing 'b' input")? as i32;
                        // let result = Solution::add(a, b);
                        // Ok(serde_json::json!(result))
                        
                        Err("Implement execute_test_case method".into())
                    }

                    /// Compare results using simple equality.
                    /// TODO: Customize comparison logic if needed.
                    fn compare_results(&self, actual: &Value, expected: &Value) -> bool {
                        actual == expected
                    }
                }

                fn main() {
                    // Create and run the driver
                    let driver = BaseDriver::new(NewProblemDriver);
                    driver.run();
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