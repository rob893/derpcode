You are given an array of integers `nums` and an integer `target`, return **indices** of the two numbers such that they add up to `target`.

## Requirements

- You may assume that each input would have **exactly one solution**
- You may **not** use the same element twice
- You can return the answer in any order

> ⚡ **Performance Goal**: Try to solve this in **O(n)** time complexity.

## Examples

### Example 1

```
Input: nums = [1,8,15,25], target = 9
Output: [0,1]
Explanation: Because nums[0] + nums[1] == 9, we return [0, 1].
```

### Example 2

```
Input: nums = [4,1,5], target = 6
Output: [1,2]
Explanation: Because nums[1] + nums[2] == 6, we return [1, 2].
```

### Example 3

```
Input: nums = [2,4], target = 6
Output: [0,1]
Explanation: Because nums[0] + nums[1] == 6, we return [0, 1].
```

## Constraints

- `2 <= nums.length <= 10^4`
- `-10^9 <= nums[i] <= 10^9`
- `-10^9 <= target <= 10^9`
- Only one valid answer exists
