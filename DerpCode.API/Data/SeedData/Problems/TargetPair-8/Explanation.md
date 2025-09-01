# Target Pair - Solution Explanation

## The Problem

We need to find two numbers in an array that add up to a target value and return their indices. The key insight is that we can use a hash table to solve this efficiently.

## Approach 1: Brute Force (Not Recommended)

The naive approach would be to check every pair of numbers:

- Time Complexity: O(n²)
- Space Complexity: O(1)

This works but is inefficient for large arrays.

## Approach 2: Hash Table (Recommended)

The optimal solution uses a hash table to store numbers we've seen along with their indices.

### Algorithm:

1. Create an empty hash table/map
2. For each number in the array:
   - Calculate the complement: `complement = target - current_number`
   - Check if the complement exists in our hash table
   - If it exists, we found our answer! Return the indices
   - If it doesn't exist, store the current number and its index in the hash table
3. Continue until we find the pair

### Why This Works:

- **One Pass**: We only need to iterate through the array once
- **Fast Lookup**: Hash tables provide O(1) average lookup time
- **Complement Logic**: If we're looking for two numbers that sum to `target`, and we know one number, we can calculate what the other number must be

### Example Walkthrough:

```
nums = [2, 7, 11, 15], target = 9

Iteration 1: num = 2, index = 0
- complement = 9 - 2 = 7
- 7 not in hash table yet
- Store: {2: 0}

Iteration 2: num = 7, index = 1
- complement = 9 - 7 = 2
- 2 exists in hash table at index 0
- Found it! Return [0, 1]
```

## Time & Space Complexity

- **Time Complexity**: O(n) - single pass through array
- **Space Complexity**: O(n) - hash table can store up to n elements

## Key Insights

1. **Think in terms of complements**: Instead of looking for two numbers that sum to target, look for the complement of each number
2. **Trade space for time**: Using extra memory (hash table) dramatically improves time complexity
3. **Hash tables are your friend**: Many array problems can be optimized using hash tables for fast lookups

This problem is a classic introduction to using hash tables for optimization and demonstrates how the right data structure can transform an O(n²) problem into an O(n) solution!
