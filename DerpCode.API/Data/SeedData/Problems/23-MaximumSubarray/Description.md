Given an integer array `nums`, find the subarray with the largest sum, and return its sum.

A **subarray** is a contiguous non-empty sequence of elements within an array.

## Requirements

Implement a function with the following signature:

- `int maxSubArray(int[] nums)` - Return the maximum sum of any contiguous subarray

### Constraints:

- `1 <= nums.length <= 10^5`
- `-10^4 <= nums[i] <= 10^4`

## Examples

```java
maxSubArray([-2,1,-3,4,-1,2,1,-5,4]);  // returns 6
// Explanation: The subarray [4,-1,2,1] has the largest sum of 6

maxSubArray([1]);  // returns 1
// Explanation: The subarray [1] has the largest sum of 1

maxSubArray([5,4,-1,7,8]);  // returns 23
// Explanation: The subarray [5,4,-1,7,8] has the largest sum of 23

maxSubArray([-1]);  // returns -1
// Explanation: The subarray [-1] has the largest sum of -1
```
