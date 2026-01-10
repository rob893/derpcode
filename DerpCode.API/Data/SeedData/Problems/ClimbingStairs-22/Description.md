You are climbing a staircase. It takes `n` steps to reach the top.

Each time you can either climb **1 or 2 steps**. In how many distinct ways can you climb to the top?

## Requirements

Implement a function with the following signature:

- `int climbStairs(int n)` - Return the number of distinct ways to climb to the top

### Constraints:

- `1 <= n <= 45`
- The answer is guaranteed to fit in a 32-bit integer

## Examples

```java
climbStairs(2);  // returns 2
// Explanation: There are two ways to climb to the top.
// 1. 1 step + 1 step
// 2. 2 steps

climbStairs(3);  // returns 3
// Explanation: There are three ways to climb to the top.
// 1. 1 step + 1 step + 1 step
// 2. 1 step + 2 steps
// 3. 2 steps + 1 step

climbStairs(4);  // returns 5
// Explanation: There are five ways:
// 1. 1+1+1+1
// 2. 1+1+2
// 3. 1+2+1
// 4. 2+1+1
// 5. 2+2

climbStairs(1);  // returns 1
// Explanation: Only one way: 1 step
```

## Key Concepts

- **Dynamic Programming**: Build solution from smaller subproblems
- **Recurrence Relation**: Ways to reach step n = ways to reach (n-1) + ways to reach (n-2)
- **Fibonacci Pattern**: The solution follows the Fibonacci sequence
