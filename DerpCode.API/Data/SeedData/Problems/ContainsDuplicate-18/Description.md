Given an integer array `nums`, return `true` if any value appears **at least twice** in the array, and return `false` if every element is distinct.

## Requirements

Implement a function with the following signature:

- `boolean containsDuplicate(int[] nums)` - Returns `true` if the array contains duplicates, `false` otherwise

## Examples

**Example 1:**

```java
Input: nums = [1,2,3,1]
Output: true
Explanation: The element 1 appears at indices 0 and 3.
```

**Example 2:**

```java
Input: nums = [1,2,3,4]
Output: false
Explanation: All elements are distinct.
```

**Example 3:**

```java
Input: nums = [1,1,1,3,3,4,3,2,4,2]
Output: true
Explanation: Multiple elements appear more than once.
```

## Constraints

- `1 <= nums.length <= 10^5`
- `-10^9 <= nums[i] <= 10^9`
