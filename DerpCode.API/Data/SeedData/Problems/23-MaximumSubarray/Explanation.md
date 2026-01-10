# Maximum Subarray

The maximum subarray problem is a classic dynamic programming problem that demonstrates the power of Kadane's algorithm. It's widely used in finance, data analysis, and algorithm design.

## Key Concepts

The optimal solution uses **Kadane's Algorithm** to achieve O(n) time complexity:

1. **Local Maximum**: Track the best sum ending at the current position
2. **Global Maximum**: Track the overall best sum seen so far
3. **Decision**: At each position, decide whether to extend the current subarray or start fresh

## Why Kadane's Algorithm Works

At each position, we face a choice:
- **Extend**: Add current element to existing subarray
- **Start Fresh**: Begin a new subarray from current element

We choose to extend only if it increases the sum, otherwise we start fresh.

## Algorithm Walkthrough

### Kadane's Algorithm

1. Initialize `maxSoFar` and `maxEndingHere` to first element
2. For each remaining element:
   - `maxEndingHere = max(nums[i], maxEndingHere + nums[i])`
   - `maxSoFar = max(maxSoFar, maxEndingHere)`
3. Return `maxSoFar`

The key insight: `maxEndingHere` represents the maximum sum of subarrays ending at position i.

## Complete TypeScript Implementation

```typescript
function maxSubArray(nums: number[]): number {
    let maxSoFar = nums[0];
    let maxEndingHere = nums[0];
    
    for (let i = 1; i < nums.length; i++) {
        // Either extend current subarray or start new one
        maxEndingHere = Math.max(nums[i], maxEndingHere + nums[i]);
        
        // Update overall maximum
        maxSoFar = Math.max(maxSoFar, maxEndingHere);
    }
    
    return maxSoFar;
}
```

## Step-by-Step Example

Let's trace through `[-2, 1, -3, 4, -1, 2, 1, -5, 4]`:

1. **i=0**: maxEndingHere = -2, maxSoFar = -2
2. **i=1**: maxEndingHere = max(1, -2+1) = 1, maxSoFar = 1
3. **i=2**: maxEndingHere = max(-3, 1-3) = -2, maxSoFar = 1
4. **i=3**: maxEndingHere = max(4, -2+4) = 4, maxSoFar = 4
5. **i=4**: maxEndingHere = max(-1, 4-1) = 3, maxSoFar = 4
6. **i=5**: maxEndingHere = max(2, 3+2) = 5, maxSoFar = 5
7. **i=6**: maxEndingHere = max(1, 5+1) = 6, maxSoFar = 6
8. **i=7**: maxEndingHere = max(-5, 6-5) = 1, maxSoFar = 6
9. **i=8**: maxEndingHere = max(4, 1+4) = 5, maxSoFar = 6

Result: **6** (subarray [4, -1, 2, 1])

## Time Complexity

- **O(n)** where n is the length of the array
  - Single pass through the array

## Space Complexity

- **O(1)** - Only using two variables

## Alternative Approaches

### Divide and Conquer

You can solve this using divide and conquer in O(n log n):

1. Divide array into left and right halves
2. Recursively find max subarray in each half
3. Find max subarray crossing the midpoint
4. Return the maximum of the three

However, Kadane's algorithm is simpler and more efficient.

### Brute Force

Check all possible subarrays: O(n²) or O(n³) - not recommended for large inputs.

## Why This Is Dynamic Programming

- **Optimal Substructure**: Solution for position i depends on solution for i-1
- **Overlapping Subproblems**: We reuse `maxEndingHere` from previous iteration
- **Bottom-Up**: Build solution iteratively from left to right

## Common Mistakes to Avoid

1. **Not handling negative numbers**: Algorithm works correctly even with all negative numbers
2. **Forgetting to update maxSoFar**: Must track both local and global maximums
3. **Starting from wrong index**: Make sure to handle the first element correctly
4. **Trying to track the actual subarray**: The problem only asks for the sum, not the indices

## Variations

- **Maximum Subarray with at most one deletion**: Can skip one element
- **Maximum Circular Subarray**: Array wraps around
- **Maximum Product Subarray**: Multiply instead of sum

The key insight of Kadane's algorithm is that at each step, we only need to remember one piece of information: the best sum we can achieve ending at the current position. This greedy local decision leads to the globally optimal solution.
