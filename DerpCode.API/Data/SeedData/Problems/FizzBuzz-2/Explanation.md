# FizzBuzz Implementation

FizzBuzz is a classic programming problem that teaches fundamental concepts of conditional logic, modular arithmetic, and control flow. Despite its apparent simplicity, it effectively demonstrates several important programming principles.

## Key Concepts

This problem covers essential programming concepts:

1. **Modular Arithmetic**: Using the modulo operator (`%`) to check divisibility
2. **Conditional Logic**: Implementing if-else statements with multiple conditions
3. **Boolean Logic**: Understanding how to combine conditions with logical operators
4. **String Handling**: Working with string return values and type conversion
5. **Control Flow**: Managing the order of condition checks

## Why This Matters

FizzBuzz teaches critical programming skills:

- **Logical Thinking**: Breaking down complex conditions into simple checks
- **Edge Case Handling**: Considering all possible scenarios and their priorities
- **Clean Code**: Writing readable and maintainable conditional logic
- **Testing Logic**: Understanding how different inputs produce different outputs

## Algorithm Walkthrough

The key insight is the **order of conditions**. We must check for the combined condition first:

1. **Check Divisibility by Both 3 and 5**: If `n % 15 == 0`, return "fizzbuzz"
2. **Check Divisibility by 3**: If `n % 3 == 0`, return "fizz"
3. **Check Divisibility by 5**: If `n % 5 == 0`, return "buzz"
4. **Default Case**: Return empty string

## Complete C# Implementation

```csharp
using System;

public class Solution
{
    /// <summary>
    /// Implements FizzBuzz logic for a single number.
    /// </summary>
    /// <param name="n">The number to evaluate</param>
    /// <returns>
    /// "FizzBuzz" if divisible by both 3 and 5,
    /// "Fizz" if divisible by 3,
    /// "Buzz" if divisible by 5,
    /// otherwise empty string
    /// </returns>
    public string FizzBuzz(int n)
    {
        // Check for divisibility by both 3 and 5 first
        if (n % 15 == 0)
        {
            return "fizzbuzz";
        }

        // Check for divisibility by 3
        if (n % 3 == 0)
        {
            return "fizz";
        }

        // Check for divisibility by 5
        if (n % 5 == 0)
        {
            return "buzz";
        }

        // Default case: return empty string.
        return string.Empty;
    }
}
```

## Alternative Implementation

You can also use logical AND to check for both conditions:

```csharp
public string FizzBuzz(int n)
{
    // Check for divisibility by both 3 and 5
    if (n % 3 == 0 && n % 5 == 0)
    {
        return "fizzbuzz";
    }

    if (n % 3 == 0)
    {
        return "fizz";
    }

    if (n % 5 == 0)
    {
        return "buzz";
    }

    return string.Empty;
}
```

## Step-by-Step Examples

Let's trace through several examples:

1. `FizzBuzz(15)`

   - `15 % 15 == 0` → **True** → Return "fizzbuzz"

2. `FizzBuzz(9)`

   - `9 % 15 == 0` → False
   - `9 % 3 == 0` → **True** → Return "fizz"

3. `FizzBuzz(10)`

   - `10 % 15 == 0` → False
   - `10 % 3 == 0` → False
   - `10 % 5 == 0` → **True** → Return "buzz"

4. `FizzBuzz(7)`
   - `7 % 15 == 0` → False
   - `7 % 3 == 0` → False
   - `7 % 5 == 0` → False
   - Return `""`

## Time Complexity

- **O(1)** - Constant time for each evaluation, regardless of input size

## Space Complexity

- **O(1)** - No additional space required beyond the output string

## Understanding the Modulo Operator

The modulo operator (`%`) returns the remainder of division:

```csharp
15 % 3 = 0  // 15 ÷ 3 = 5 remainder 0
15 % 5 = 0  // 15 ÷ 5 = 3 remainder 0
16 % 3 = 1  // 16 ÷ 3 = 5 remainder 1
17 % 5 = 2  // 17 ÷ 5 = 3 remainder 2
```

A number is **divisible** by another when the remainder is 0.

## Common Mistakes to Avoid

1. **Wrong Order of Conditions**: Checking individual conditions before the combined condition

   ```csharp
   // WRONG - this will never return "FizzBuzz"
   if (n % 3 == 0) return "fizz";
   if (n % 5 == 0) return "buzz";
   if (n % 15 == 0) return "fizzbuzz"; // Never reached!
   ```

2. **Incorrect Logical Operators**: Using OR instead of AND

   ```csharp
   // WRONG - this is always true for multiples of 3 or 5
   if (n % 3 == 0 || n % 5 == 0) return "fizzbuzz";
   ```

3. **Forgetting String Conversion**: Returning integer instead of string for the default case

4. **Not Handling Edge Cases**: Consider behavior with 0, negative numbers, etc.

## Extended FizzBuzz Variations

### Multiple Conditions

```csharp
public string ExtendedFizzBuzz(int n)
{
    string result = "";

    if (n % 3 == 0) result += "fizz";
    if (n % 5 == 0) result += "buzz";
    if (n % 7 == 0) result += "bang";

    return string.IsNullOrEmpty(result) ? "" : result;
}
```

This approach is more extensible and handles arbitrary combinations naturally.

## Building Foundation

FizzBuzz establishes important programming concepts:

- **Conditional Logic**: How to structure if-else statements effectively
- **Order of Operations**: Why the sequence of checks matters
- **Modular Arithmetic**: Understanding division remainders and divisibility
- **String Handling**: Converting between data types
- **Edge Case Analysis**: Considering all possible input scenarios

These skills are fundamental for more complex algorithmic problems involving conditions and logical reasoning.
