Given a string `s`, find the length of the **longest substring** without repeating characters.

## Requirements

Implement a function with the following signature:

- `int lengthOfLongestSubstring(string s)` - Return the length of the longest substring without repeating characters

### Constraints:

- A **substring** is a contiguous sequence of characters within the string
- Characters are case-sensitive: `'A'` and `'a'` are considered different characters
- The empty string has length 0

## Examples

```java
lengthOfLongestSubstring("abcabcbb");  // returns 3
// Explanation: The longest substring is "abc", with length 3

lengthOfLongestSubstring("bbbbb");  // returns 1
// Explanation: The longest substring is "b", with length 1

lengthOfLongestSubstring("pwwkew");  // returns 3
// Explanation: The longest substring is "wke", with length 3
// Note: "pwke" is not valid as it's not a substring (not contiguous)

lengthOfLongestSubstring("abcde");  // returns 5
// Explanation: The entire string has no repeating characters

lengthOfLongestSubstring("");  // returns 0
// Explanation: Empty string has length 0

lengthOfLongestSubstring("au");  // returns 2
// Explanation: The entire string has no repeating characters
```

## Key Concepts

- **Substring**: Must be contiguous (adjacent characters in sequence)
- **No Repeating Characters**: Each character in the substring must be unique
- **Maximum Length**: Find the longest such substring possible
