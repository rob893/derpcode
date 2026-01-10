# Valid Palindrome - Complete Solution Guide

The "Valid Palindrome" problem is a classic string manipulation challenge that tests your ability to process text and apply the two-pointer technique. This problem is frequently asked in technical interviews and demonstrates fundamental concepts of string processing and algorithmic thinking.

## Problem Understanding

We need to determine if a given string is a palindrome after:

1. **Converting to lowercase**: All uppercase letters become lowercase
2. **Removing non-alphanumeric characters**: Only letters (a-z) and numbers (0-9) remain
3. **Checking if it reads the same forwards and backwards**

## Visual Example

**Original String:** `"A man a plan a canal Panama"`

**Step 1 - Convert to lowercase:** `"a man a plan a canal panama"`

**Step 2 - Remove non-alphanumeric:** `"amanaplanacanalpanama"`

**Step 3 - Check palindrome:**

```
a m a n a p l a n a c a n a l p a n a m a
^                                     ^
 ^                                   ^
  ^                                 ^
   ^                               ^
    ^                             ^
     ^                           ^
      ^                         ^
       ^                       ^
        ^                     ^
         ^                   ^
          ^                 ^
           ^               ^
            ^             ^
             ^           ^
              ^         ^
               ^       ^
                ^     ^
                 ^   ^
                  ^ ^
                   ^
```

All character pairs match, so it's a palindrome!

## Solution Approaches

### Approach 1: Clean String First (Simple)

The straightforward approach processes the string in two phases:

**Algorithm:**

1. Create a cleaned string with only lowercase alphanumeric characters
2. Use two pointers to check if the cleaned string is a palindrome

**Time Complexity:** O(n)  
**Space Complexity:** O(n) for the cleaned string

```python
def isPalindrome(s):
    # Phase 1: Clean the string
    cleaned = ""
    for char in s:
        if char.isalnum():
            cleaned += char.lower()

    # Phase 2: Check palindrome with two pointers
    left, right = 0, len(cleaned) - 1
    while left < right:
        if cleaned[left] != cleaned[right]:
            return False
        left += 1
        right -= 1

    return True
```

### Approach 2: Two Pointers with Skip (Optimal)

The optimal approach processes the string in one pass without creating an additional string:

**Algorithm:**

1. Use two pointers starting from both ends
2. Skip non-alphanumeric characters
3. Compare valid characters after converting to lowercase
4. Move pointers inward until they meet

**Time Complexity:** O(n)  
**Space Complexity:** O(1)

```python
def isPalindrome(s):
    left, right = 0, len(s) - 1

    while left < right:
        # Skip non-alphanumeric characters from left
        while left < right and not s[left].isalnum():
            left += 1

        # Skip non-alphanumeric characters from right
        while left < right and not s[right].isalnum():
            right -= 1

        # Compare characters (case-insensitive)
        if s[left].lower() != s[right].lower():
            return False

        left += 1
        right -= 1

    return True
```

## Key Insights

### Character Processing

- **isalnum()**: Built-in method to check if a character is alphanumeric
- **lower()**: Converts characters to lowercase for case-insensitive comparison
- **Skip invalid characters**: Move pointers without comparison when encountering non-alphanumeric characters

### Two Pointers Technique

- **Initialization**: Start with `left = 0` and `right = len(s) - 1`
- **Movement**: Only move pointers after processing current characters
- **Termination**: Stop when `left >= right` (pointers meet or cross)

### Edge Cases to Consider

- **Empty string**: Returns `true` (considered a valid palindrome)
- **Single character**: Always returns `true`
- **Only non-alphanumeric**: Like `"!@#"` becomes empty, so returns `true`
- **Mixed case**: Like `"Aa"` should return `true`

## Common Mistakes to Avoid

1. **Forgetting case conversion**: `'A' != 'a'` but they should be treated as equal
2. **Not handling non-alphanumeric**: Spaces and punctuation should be ignored
3. **Off-by-one errors**: Make sure `left < right` condition is correct
4. **Creating unnecessary strings**: The optimal solution uses O(1) space

## Step-by-Step Example

Let's trace through `"A man a plan a canal Panama"`:

```
Initial: left=0, right=26
s[0]='A', s[26]='a' → 'a'=='a' ✓, left=1, right=25
s[1]=' ' → skip, left=2
s[2]='m', s[25]='m' → 'm'=='m' ✓, left=3, right=24
s[3]='a', s[24]='a' → 'a'=='a' ✓, left=4, right=23
s[4]='n', s[23]='n' → 'n'=='n' ✓, left=5, right=22
s[5]=' ' → skip, left=6
s[6]='a', s[22]='a' → 'a'=='a' ✓, left=7, right=21
...continue until left >= right
```

All comparisons match, so the result is `true`.

## Alternative Solutions

### Using Recursion

```python
def isPalindrome(s):
    def helper(left, right):
        if left >= right:
            return True

        if not s[left].isalnum():
            return helper(left + 1, right)
        if not s[right].isalnum():
            return helper(left, right - 1)

        if s[left].lower() != s[right].lower():
            return False

        return helper(left + 1, right - 1)

    return helper(0, len(s) - 1)
```

### Using Regular Expressions

```python
import re

def isPalindrome(s):
    cleaned = re.sub(r'[^a-zA-Z0-9]', '', s).lower()
    return cleaned == cleaned[::-1]
```

## Practice Variations

1. **Case-sensitive palindrome**: Don't convert to lowercase
2. **Alphanumeric palindrome**: Include specific characters only
3. **Word-level palindrome**: Check if words form a palindrome
4. **Longest palindromic substring**: Find the longest palindrome within a string

## Summary

The Valid Palindrome problem teaches important concepts:

- **String processing**: Cleaning and normalizing text data
- **Two pointers technique**: Efficient O(1) space solution
- **Character validation**: Using built-in methods like `isalnum()` and `lower()`
- **Edge case handling**: Empty strings, single characters, and special characters

The optimal two-pointer solution with character skipping is the preferred approach due to its O(1) space complexity while maintaining O(n) time complexity.
