Given a string `s` containing just the characters `'('`, `')'`, `'{'`, `'}'`, `'['` and `']'`, determine if the input string is valid.

An input string is valid if:

1. Open brackets must be closed by the same type of brackets
2. Open brackets must be closed in the correct order
3. Every close bracket has a corresponding open bracket of the same type

## Requirements

Implement a function with the following signature:

- `boolean isValid(string s)` - Return `true` if the string is valid, `false` otherwise

### Constraints:

- `1 <= s.length <= 10^4`
- `s` consists of parentheses only: `'()[]{}'`

## Examples

```java
isValid("()");       // returns true
isValid("()[]{}");   // returns true
isValid("(]");       // returns false
isValid("([)]");     // returns false (wrong order)
isValid("{[]}");     // returns true
isValid("(((");      // returns false (unclosed brackets)
```

## Key Concepts

- **Matching Brackets**: Each opening bracket must have a corresponding closing bracket of the same type
- **Complete Pairing**: All brackets must be properly paired with no leftovers
