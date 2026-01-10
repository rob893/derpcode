Design a function that processes **range queries** over an array of integers, except the queries are mean and the array is allowed to mutate.

## Task

Implement `RangeMaxSubsetXor(nums, queries)`.

You are given:

- `nums`: an array of non-negative integers
- `queries`: a list of queries of two types

Each query is one of:

- `[1, i, x]` — **update**: set `nums[i] = x` (1-indexed)
- `[2, l, r]` — **ask**: return the **maximum XOR value** obtainable by XOR-ing **any subset** of the numbers in the subarray `nums[l..r]` (1-indexed, inclusive)

For every query of type `2`, append its answer to the output list.

## Notes

- “Subset” includes the empty subset (which has XOR value `0`).
- The result of a type `2` query is a single integer.

## Example

```
nums = [3, 10, 5]
queries = [
  [2, 1, 3],
  [1, 2, 7],
  [2, 1, 2],
  [2, 2, 3]
]

output = [15, 7, 7]
```

Explanation (of the example only):

- On `[2, 1, 3]`, the maximum subset XOR of `{3,10,5}` is `10 XOR 5 = 15`.
