# Fibonacci Number Implementation

The Fibonacci sequence is one of the most famous sequences in mathematics and computer science. Computing the nth Fibonacci number is a classic problem that can be solved in several ways, each with different time and space complexities.

## Understanding the Problem

The Fibonacci sequence starts with 0 and 1, and each subsequent number is the sum of the two preceding numbers:

- F(0) = 0
- F(1) = 1
- F(n) = F(n-1) + F(n-2) for n > 1

This creates the sequence: 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, ...

## Approach 1: Recursive Solution (Naive)

The most straightforward approach follows the mathematical definition directly:

```csharp
public int Fibonacci(int n)
{
    if (n <= 1) return n;
    return Fibonacci(n - 1) + Fibonacci(n - 2);
}
```

**Time Complexity**: O(2^n) - exponential time due to redundant calculations
**Space Complexity**: O(n) - due to recursion stack depth

**Problem**: This approach recalculates the same values multiple times, making it extremely inefficient for larger values of n.

## Approach 2: Dynamic Programming (Memoization)

We can optimize the recursive approach by storing previously calculated results:

```csharp
private Dictionary<int, int> memo = new Dictionary<int, int>();

public int Fibonacci(int n)
{
    if (n <= 1) return n;

    if (memo.ContainsKey(n))
        return memo[n];

    memo[n] = Fibonacci(n - 1) + Fibonacci(n - 2);
    return memo[n];
}
```

**Time Complexity**: O(n) - each value is calculated once
**Space Complexity**: O(n) - for memoization storage and recursion stack

## Approach 3: Iterative Solution (Bottom-Up)

The most efficient approach builds the sequence from the bottom up:

```csharp
public int Fibonacci(int n)
{
    if (n <= 1) return n;

    int prev2 = 0; // F(0)
    int prev1 = 1; // F(1)

    for (int i = 2; i <= n; i++)
    {
        int current = prev1 + prev2;
        prev2 = prev1;
        prev1 = current;
    }

    return prev1;
}
```

**Time Complexity**: O(n) - single pass through the sequence
**Space Complexity**: O(1) - only uses a constant amount of extra space

## Approach 4: Matrix Exponentiation (Advanced)

For very large values of n, matrix exponentiation can compute Fibonacci numbers in O(log n) time:

The transformation can be represented as:

```
[F(n+1)]   [1 1]^n   [1]
[F(n)  ] = [1 0]   * [0]
```

This approach is overkill for the given constraints but demonstrates an advanced technique.

## Recommended Solution

For the given constraints (n ≤ 30), the **iterative approach** is optimal because:

1. **Linear Time**: O(n) is efficient for small to medium values
2. **Constant Space**: O(1) space usage is minimal
3. **Simple Implementation**: Easy to understand and implement correctly
4. **No Overflow Issues**: Within the range of standard integers for n ≤ 30

## Edge Cases to Consider

- **n = 0**: Should return 0
- **n = 1**: Should return 1
- **Negative n**: Problem constraints specify n ≥ 0, so this shouldn't occur

## Common Pitfalls

1. **Off-by-one errors**: Make sure F(0) = 0 and F(1) = 1
2. **Integer overflow**: For very large n, consider using long or BigInteger
3. **Recursive stack overflow**: Naive recursion fails for large n due to deep call stacks

The iterative solution elegantly avoids all these issues while maintaining optimal performance for the given constraints.
