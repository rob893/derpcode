# Contains Duplicate Solution

The problem asks us to determine if an array contains any duplicate values. This is a fundamental problem that tests understanding of data structures and their time-space tradeoffs.

## Approach: Hash Set

The optimal solution uses a **hash set** (or hash table) to track elements we've seen while iterating through the array. This allows us to detect duplicates in a single pass.

### Why Hash Set Works

A hash set provides O(1) average-case lookup time, making it perfect for checking if we've seen an element before. As we iterate through the array:

1. If the element is already in the set → we found a duplicate, return `true`
2. If the element is not in the set → add it and continue
3. If we finish the loop without finding duplicates → return `false`

## Algorithm

1. Create an empty hash set
2. Iterate through each element in the array:
   - Check if the element exists in the set
   - If yes, return `true` (duplicate found)
   - If no, add the element to the set
3. If the loop completes, return `false` (no duplicates)

## Complete Python Implementation

```python
class Solution:
    @staticmethod
    def contains_duplicate(nums):
        seen = set()
        
        for num in nums:
            if num in seen:
                return True
            seen.add(num)
        
        return False
```

## Step-by-Step Example

Let's trace through `nums = [1, 2, 3, 1]`:

1. **Iteration 1**: Check `1`
   - `1` not in set
   - Add `1` to set: `{1}`

2. **Iteration 2**: Check `2`
   - `2` not in set
   - Add `2` to set: `{1, 2}`

3. **Iteration 3**: Check `3`
   - `3` not in set
   - Add `3` to set: `{1, 2, 3}`

4. **Iteration 4**: Check `1`
   - `1` is in set ✓
   - Return `true` (duplicate found!)

## Time Complexity

- **O(n)** where n is the length of the array
- We iterate through the array once, and hash set operations (lookup and insert) are O(1) on average

## Space Complexity

- **O(n)** in the worst case
- When all elements are unique, we store all n elements in the hash set
- If duplicates are found early, space usage can be much less

## Alternative Approaches

### Approach 1: Brute Force (Not Recommended)

Compare every element with every other element using nested loops.

- **Time**: O(n²)
- **Space**: O(1)
- **Verdict**: Too slow for large inputs

### Approach 2: Sorting

Sort the array first, then check if adjacent elements are equal.

- **Time**: O(n log n) due to sorting
- **Space**: O(1) if sorting in-place, O(n) otherwise
- **Verdict**: Good if space is limited but slower than hash set approach

### Approach 3: Hash Set (Optimal)

The solution we implemented above.

- **Time**: O(n)
- **Space**: O(n)
- **Verdict**: Best overall balance for this problem

## Language-Specific Tips

- **Python**: Use the built-in `set()` type, which is highly optimized
- **JavaScript/TypeScript**: Use `Set` or `Map` for similar functionality
- **Java**: Use `HashSet<Integer>` from `java.util`
- **C#**: Use `HashSet<int>` from `System.Collections.Generic`
- **Rust**: Use `HashSet<i32>` from `std::collections`

## Common Mistakes to Avoid

1. **Not considering negative numbers**: The solution works for any integers, including negatives
2. **Modifying the input array**: Unless explicitly allowed, don't sort the original array
3. **Off-by-one errors**: Make sure to iterate through all elements
4. **Not handling empty arrays**: Though constraints guarantee at least 1 element, good practice to handle edge cases

The key insight is recognizing that we need fast lookup to check for duplicates efficiently, making a hash set the perfect data structure for this problem.
