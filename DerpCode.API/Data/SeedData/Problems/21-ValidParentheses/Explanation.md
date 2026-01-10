# Valid Parentheses

Validating parentheses is a classic stack problem that demonstrates the Last-In-First-Out (LIFO) data structure. This problem is fundamental to understanding expression parsing and compiler design.

## Key Concepts

The optimal solution uses a **stack** data structure to achieve O(n) time complexity:

1. **Stack**: Stores opening brackets as we encounter them
2. **Matching**: When we see a closing bracket, check if it matches the top of the stack
3. **LIFO Property**: The most recently opened bracket should be closed first

## Why a Stack Works

- **Opening Bracket**: Push onto the stack to "remember" it
- **Closing Bracket**: Pop from stack and verify it matches
- **Order Enforcement**: Stack naturally enforces correct nesting order
- **Validation**: If stack is empty at the end, all brackets were properly matched

## Algorithm Walkthrough

### Stack-Based Approach

1. Create an empty stack
2. Iterate through each character in the string:
   - If it's an opening bracket `(`, `{`, or `[`: push onto stack
   - If it's a closing bracket `)`, `}`, or `]`:
     - If stack is empty, return false (no matching opener)
     - Pop from stack and check if it matches the closing bracket
     - If it doesn't match, return false
3. After processing all characters, return true if stack is empty, false otherwise

## Complete TypeScript Implementation

```typescript
function isValid(s: string): boolean {
    const stack: string[] = [];
    const pairs: { [key: string]: string } = {
        ')': '(',
        '}': '{',
        ']': '['
    };
    
    for (const char of s) {
        if (char === '(' || char === '{' || char === '[') {
            // Opening bracket - push to stack
            stack.push(char);
        } else {
            // Closing bracket - check if it matches
            if (stack.length === 0) {
                return false; // No matching opening bracket
            }
            
            const top = stack.pop()!;
            if (top !== pairs[char]) {
                return false; // Mismatched brackets
            }
        }
    }
    
    // Valid only if all brackets were matched (stack is empty)
    return stack.length === 0;
}
```

## Step-by-Step Example

Let's trace through `"([])"`

:

1. **char='('**: Opening bracket → Push to stack: `['(']`
2. **char='['**: Opening bracket → Push to stack: `['(', '[']`
3. **char=']'**: Closing bracket → Pop `'['` → Match! Stack: `['(']`
4. **char=')'**: Closing bracket → Pop `'('` → Match! Stack: `[]`
5. **Stack is empty**: Return `true`

Now let's trace through `"([)]"` (invalid):

1. **char='('**: Push to stack: `['(']`
2. **char='['**: Push to stack: `['(', '[']`
3. **char=')'**: Pop `'['` → Doesn't match `)` → Return `false`

## Time Complexity

- **O(n)** where n is the length of the string
  - We visit each character exactly once

## Space Complexity

- **O(n)** in worst case
  - If all characters are opening brackets, we store all of them in the stack

## Common Mistakes to Avoid

1. **Not checking for empty stack**: Before popping, always verify the stack isn't empty
2. **Forgetting to check final stack**: Must ensure stack is empty at the end
3. **Wrong match check**: Make sure closing bracket matches the popped opening bracket
4. **Character case**: Problem specifies exact characters - don't check for other types

## Alternative Approaches

### Using a Map for Cleaner Code

Store pairs in a map for more maintainable code:

```typescript
function isValid(s: string): boolean {
    const stack: string[] = [];
    const pairs = new Map([
        [')', '('],
        ['}', '{'],
        [']', '[']
    ]);
    
    for (const char of s) {
        if (pairs.has(char)) {
            // Closing bracket
            if (stack.length === 0 || stack.pop() !== pairs.get(char)) {
                return false;
            }
        } else {
            // Opening bracket
            stack.push(char);
        }
    }
    
    return stack.length === 0;
}
```

## Real-World Applications

This pattern is used in:
- **Compilers**: Parsing code syntax
- **Text Editors**: Matching brackets for code highlighting
- **Expression Evaluation**: Validating mathematical expressions
- **HTML/XML Parsing**: Validating tag nesting

The key insight is recognizing that bracket matching follows a Last-In-First-Out pattern, making the stack data structure the perfect tool for this problem.
