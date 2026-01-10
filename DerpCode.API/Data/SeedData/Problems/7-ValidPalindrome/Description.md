# Valid Palindrome

A phrase is a **palindrome** if it reads the same forward and backward. Non-alphanumeric characters should be ignored. Alphanumeric characters include letters and numbers. Palindromes are not case sensitive.

Given a string `s`, return `true` if it is a palindrome, or `false` otherwise.

## Examples

### Example 1:

**Input:** `s = "A man a plan a canal Panama"`  
**Output:** `true`  
**Explanation:** "amanaplanacanalpanama" is a palindrome.

### Example 2:

**Input:** `s = "race a car"`  
**Output:** `false`  
**Explanation:** "raceacar" is not a palindrome.

### Example 3:

**Input:** `s = ""`  
**Output:** `true`  
**Explanation:** An empty string reads the same forward and backward, so it is a palindrome.

### Example 4:

**Input:** `s = "a"`  
**Output:** `true`  
**Explanation:** A single character is always a palindrome.

### Example 5:

**Input:** `s = "Madam"`  
**Output:** `true`  
**Explanation:** "madam" is a palindrome.

### Example 6:

**Input:** `s = "No 'x' in Nixon"`  
**Output:** `false`  
**Explanation:** "noxinnixon" is not a palindrome.

## Constraints

- `0 <= s.length <= 2 * 10^5`
- `s` consists only of printable ASCII characters.

## Notes

- You should ignore spaces, punctuation, and capitalization when determining if a string is a palindrome.
- Only alphanumeric characters should be considered.
- An empty string is considered a valid palindrome.
