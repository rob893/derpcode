Given a 2D integer array `matrix`, return a new 2D array that is the **mirror image** of `matrix` across its **vertical axis**.

That means:

- Every row keeps its position.
- Within each row, the order of elements is reversed.

## Requirements

Implement a function with this behavior:

- Input: a 2D array of integers
- Output: a 2D array of integers with each row reversed

You may either:

- return a new mirrored matrix, or
- modify the input matrix in-place and return it

## Examples

```text
Input:
[
  [1, 2, 3],
  [4, 5, 6]
]

Output:
[
  [3, 2, 1],
  [6, 5, 4]
]
```

```text
Input:
[
  [7, 8],
  [9, 10],
  [11, 12]
]

Output:
[
  [8, 7],
  [10, 9],
  [12, 11]
]
```
