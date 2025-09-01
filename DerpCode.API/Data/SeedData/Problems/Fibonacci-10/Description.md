The Fibonacci sequence is a classic mathematical sequence where each number is the sum of the two preceding ones. Your task is to compute the **nth Fibonacci number**.

## Requirements

Implement a function that:

- Takes an integer `n` as input (where n ≥ 0)
- Returns the nth Fibonacci number

## Fibonacci Sequence Definition

The Fibonacci sequence is defined as:

- F(0) = 0
- F(1) = 1
- F(n) = F(n-1) + F(n-2) for n > 1

So the sequence goes: 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, ...

> ⚡ **Performance Goal**: Consider the time complexity of your approach - naive recursion can be very slow for large n.

## Examples

```
Input: n = 0
Output: 0
Explanation: F(0) = 0

Input: n = 1
Output: 1
Explanation: F(1) = 1

Input: n = 2
Output: 1
Explanation: F(2) = F(1) + F(0) = 1 + 0 = 1

Input: n = 10
Output: 55
Explanation: F(10) = 55
```

## Constraints

- 0 ≤ n ≤ 30
- Your solution should handle edge cases for n = 0 and n = 1
