# Longest Substring Without Repeating Characters

This problem asks you to find the length of the longest substring in a given string that contains no repeating characters. This is a classic sliding window problem that teaches important techniques for optimizing string traversal.

## Key Concepts

The optimal solution uses the **sliding window** technique with a hash table to achieve O(n) time complexity:

1. **Sliding Window**: Maintain a window of characters that has no repeats
2. **Hash Table/Set**: Track characters currently in the window
3. **Two Pointers**: Use left and right pointers to define the window boundaries

## Why This Approach Works

- **Right Pointer**: Expands the window by adding new characters
- **Left Pointer**: Contracts the window when duplicates are found
- **Hash Table**: Provides O(1) lookup to check if a character is already in the window
- **Maximum Tracking**: Keep track of the maximum window size seen so far

## Algorithm Walkthrough

### Sliding Window Approach

1. Initialize `left = 0`, `maxLength = 0`, and an empty set/map for seen characters
2. Iterate through the string with a `right` pointer:
   - If `s[right]` is not in the set:
     - Add `s[right]` to the set
     - Update `maxLength = max(maxLength, right - left + 1)`
   - If `s[right]` is already in the set:
     - Remove `s[left]` from the set
     - Move `left` pointer forward
     - Repeat until `s[right]` is no longer in the set
3. Return `maxLength`

## Complete TypeScript Implementation

```typescript
function lengthOfLongestSubstring(s: string): number {
    const charSet = new Set<string>();
    let left = 0;
    let maxLength = 0;

    for (let right = 0; right < s.length; right++) {
        // Shrink window from left while we have a duplicate
        while (charSet.has(s[right])) {
            charSet.delete(s[left]);
            left++;
        }
        
        // Add current character to window
        charSet.add(s[right]);
        
        // Update maximum length
        maxLength = Math.max(maxLength, right - left + 1);
    }

    return maxLength;
}
```

## Optimized Version Using Hash Map

You can optimize further by storing character indices instead of just presence:

```typescript
function lengthOfLongestSubstring(s: string): number {
    const charIndex = new Map<string, number>();
    let left = 0;
    let maxLength = 0;

    for (let right = 0; right < s.length; right++) {
        const char = s[right];
        
        // If we've seen this character and it's in our current window
        if (charIndex.has(char) && charIndex.get(char)! >= left) {
            // Jump left pointer to just after the duplicate
            left = charIndex.get(char)! + 1;
        }
        
        // Update character's latest position
        charIndex.set(char, right);
        
        // Update maximum length
        maxLength = Math.max(maxLength, right - left + 1);
    }

    return maxLength;
}
```

## Step-by-Step Example

Let's trace through `"pwwkew"`:

1. **right=0, char='p'**: Window: "p", length=1, max=1
2. **right=1, char='w'**: Window: "pw", length=2, max=2
3. **right=2, char='w'**: Duplicate! Remove 'p', then 'w'. Window: "w", length=1, max=2
4. **right=3, char='k'**: Window: "wk", length=2, max=2
5. **right=4, char='e'**: Window: "wke", length=3, max=3
6. **right=5, char='w'**: Duplicate! Remove until no 'w'. Window: "kew", length=3, max=3

Final answer: **3** (the substring "wke" or "kew")

## Time Complexity

- **Sliding Window with Set**: $O(n)$ where $n$ is the length of the string
  - Each character is visited at most twice (once by right pointer, once by left)
  
- **Optimized with Map**: $O(n)$ with better constants
  - Left pointer can jump directly instead of incrementing

## Space Complexity

- **O(min(n, m))** where $n$ is string length and $m$ is character set size
  - In the worst case, we store all unique characters
  - For ASCII, this is bounded by 128 characters

## Common Mistakes to Avoid

1. **Forgetting empty string**: Handle the edge case where input is empty
2. **Not removing from left**: When a duplicate is found, must remove characters from the left
3. **Off-by-one errors**: Window length is `right - left + 1`, not `right - left`
4. **Case sensitivity**: 'A' and 'a' are different characters

## Alternative Approaches

### Brute Force (Not Recommended)

Check every possible substring and verify uniqueness: O(n³) or O(n²) with set

### Why Sliding Window is Better

- **Efficiency**: O(n) vs O(n²) or O(n³)
- **Single Pass**: Only need to traverse the string once
- **Space Efficient**: Only store characters in current window

The key insight is that we don't need to check every substring. By maintaining a valid window and expanding/contracting it intelligently, we can find the answer in a single pass through the string.
