# Climbing Stairs

The climbing stairs problem is a classic introduction to dynamic programming. It demonstrates how breaking down a problem into smaller subproblems can lead to an elegant and efficient solution.

## Key Concepts

This problem can be solved using dynamic programming with O(n) time complexity:

1. **Recurrence Relation**: `dp[n] = dp[n-1] + dp[n-2]`
2. **Base Cases**: `dp[1] = 1` and `dp[2] = 2`
3. **Fibonacci Pattern**: The solution is actually the (n+1)th Fibonacci number

## Why This Approach Works

- **Step n-1**: If you're at step n-1, you can take 1 step to reach n
- **Step n-2**: If you're at step n-2, you can take 2 steps to reach n
- **Total Ways**: Sum of ways to reach n-1 and n-2

The insight is that to reach step n, you must have come from either step n-1 (taking a 1-step) or step n-2 (taking a 2-step).

## Algorithm Walkthrough

### Dynamic Programming Approach

1. Handle base cases: n=1 returns 1, n=2 returns 2
2. Create an array to store results for each step
3. Fill the array using: `dp[i] = dp[i-1] + dp[i-2]`
4. Return `dp[n]`

### Space-Optimized Approach

Since we only need the previous two values, we can optimize to O(1) space:

1. Use two variables instead of an array
2. Update them as we iterate

## Complete TypeScript Implementation

```typescript
// Dynamic Programming with O(n) space
function climbStairs(n: number): number {
    if (n === 1) return 1;
    if (n === 2) return 2;
    
    const dp: number[] = new Array(n + 1);
    dp[1] = 1;
    dp[2] = 2;
    
    for (let i = 3; i <= n; i++) {
        dp[i] = dp[i - 1] + dp[i - 2];
    }
    
    return dp[n];
}

// Space-optimized with O(1) space
function climbStairsOptimized(n: number): number {
    if (n === 1) return 1;
    if (n === 2) return 2;
    
    let prev2 = 1;  // dp[1]
    let prev1 = 2;  // dp[2]
    
    for (let i = 3; i <= n; i++) {
        const current = prev1 + prev2;
        prev2 = prev1;
        prev1 = current;
    }
    
    return prev1;
}
```

## Step-by-Step Example

Let's trace through n=5:

1. **Base**: dp[1] = 1, dp[2] = 2
2. **dp[3]**: dp[2] + dp[1] = 2 + 1 = 3
3. **dp[4]**: dp[3] + dp[2] = 3 + 2 = 5
4. **dp[5]**: dp[4] + dp[3] = 5 + 3 = 8

Ways to reach step 5:
- From step 4 (5 ways) + 1 step
- From step 3 (3 ways) + 2 steps
- Total: 8 ways

## Time Complexity

- **Dynamic Programming**: O(n) - Visit each step once
- **Space-Optimized**: O(n) time, O(1) space

## Space Complexity

- **With Array**: O(n) - Store all intermediate results
- **Space-Optimized**: O(1) - Only track previous two values

## Connection to Fibonacci

The climbing stairs sequence is:
- n=1: 1
- n=2: 2
- n=3: 3
- n=4: 5
- n=5: 8

This is the Fibonacci sequence starting from 1, 2 (instead of 0, 1):
- F(1) = 1
- F(2) = 2
- F(n) = F(n-1) + F(n-2)

## Common Mistakes to Avoid

1. **Off-by-one errors**: Make sure base cases are correct
2. **Not handling n=1**: Don't forget the simplest case
3. **Integer overflow**: For large n, consider using BigInt (though n ≤ 45 is safe for int32)
4. **Inefficient recursion**: Naive recursion is O(2^n) - always use DP or memoization

## Alternative Approaches

### Matrix Exponentiation

For very large n, you can use matrix exponentiation to achieve O(log n) time:

```
[[1, 1], [1, 0]]^n
```

However, for the constraint n ≤ 45, the iterative DP approach is simpler and efficient enough.

The key insight is recognizing that this problem has **optimal substructure** (solution depends on solutions to smaller problems) and **overlapping subproblems** (same subproblems are solved multiple times), making it perfect for dynamic programming.
