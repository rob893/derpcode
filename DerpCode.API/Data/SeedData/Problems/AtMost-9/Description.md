Given an array of integers `arr`, a target integer `target`, and an integer `atMostNTimes`, determine if the target integer appears in the array **at most** `atMostNTimes` times.

## Requirements

Implement the `AtMost` function:

- Return `true` if `target` appears in `arr` **≤ `atMostNTimes`** times
- Return `false` if `target` appears in `arr` **> `atMostNTimes`** times

## Parameters

- `arr`: An array of integers (can be empty)
- `target`: The integer to search for
- `atMostNTimes`: The maximum allowed occurrences (inclusive)

## Examples

### Example 1

```
Input: arr = [1, 2, 3, 1, 1], target = 1, atMostNTimes = 2
Output: false
Explanation: The number 1 appears 3 times, which is > 2
```

### Example 2

```
Input: arr = [5, 5, 5, 5, 5], target = 5, atMostNTimes = 3
Output: false
Explanation: The number 5 appears 5 times, which is > 3
```

### Example 3

```
Input: arr = [1, 2, 3, 4, 5], target = 6, atMostNTimes = 0
Output: true
Explanation: The number 6 appears 0 times, which is ≤ 0
```

### Example 4

```
Input: arr = [7, 7, 7, 7], target = 7, atMostNTimes = 5
Output: true
Explanation: The number 7 appears 4 times, which is ≤ 5
```

### Example 5

```
Input: arr = [], target = 1, atMostNTimes = 0
Output: true
Explanation: Empty array means target appears 0 times, which is ≤ 0
```

## Constraints

- `0 ≤ arr.length ≤ 1000`
- `-1000 ≤ arr[i] ≤ 1000`
- `-1000 ≤ target ≤ 1000`
- `0 ≤ atMostNTimes ≤ 1000`
