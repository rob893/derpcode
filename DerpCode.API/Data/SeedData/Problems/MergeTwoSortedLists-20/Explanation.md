# Merge Two Sorted Lists

Merging two sorted linked lists is a fundamental operation that demonstrates important linked list manipulation techniques. This problem is similar to the merge step in merge sort.

## Key Concepts

The optimal solution can be implemented either iteratively or recursively, both achieving O(n + m) time complexity:

1. **Iterative Approach**: Use a dummy node and pointer to build the result list
2. **Recursive Approach**: Compare heads and recursively merge the remainder
3. **In-Place Merging**: Reuse existing nodes without allocating new memory

## Why This Approach Works

- **Dummy Node**: Simplifies edge cases by providing a consistent starting point
- **Pointer Manipulation**: By updating `next` pointers, we build the merged list
- **Comparison**: Since both lists are sorted, we only need to compare current nodes

## Algorithm Walkthrough

### Iterative Approach

1. Create a dummy node to serve as the starting point
2. Use a `current` pointer to track where we are in the result list
3. While both lists have nodes:
   - Compare the values of the current nodes in both lists
   - Attach the smaller node to `current.next`
   - Move the pointer in the list we took from
   - Move `current` forward
4. Attach any remaining nodes from either list
5. Return `dummy.next` (the actual head of the merged list)

### Recursive Approach

1. Base cases: If one list is empty, return the other
2. Compare the heads of both lists
3. The smaller head becomes part of the result
4. Recursively merge the rest: `smaller.next = mergeTwoLists(smaller.next, other)`
5. Return the smaller head

## Complete TypeScript Implementation (Iterative)

```typescript
class ListNode {
    val: number;
    next: ListNode | null;
    
    constructor(val?: number, next?: ListNode | null) {
        this.val = val === undefined ? 0 : val;
        this.next = next === undefined ? null : next;
    }
}

function mergeTwoLists(list1: ListNode | null, list2: ListNode | null): ListNode | null {
    // Create a dummy node to simplify edge cases
    const dummy = new ListNode(0);
    let current = dummy;
    
    // Traverse both lists
    while (list1 !== null && list2 !== null) {
        if (list1.val <= list2.val) {
            current.next = list1;
            list1 = list1.next;
        } else {
            current.next = list2;
            list2 = list2.next;
        }
        current = current.next;
    }
    
    // Attach remaining nodes
    current.next = list1 !== null ? list1 : list2;
    
    return dummy.next;
}
```

## Recursive Implementation

```typescript
function mergeTwoLists(list1: ListNode | null, list2: ListNode | null): ListNode | null {
    // Base cases
    if (list1 === null) return list2;
    if (list2 === null) return list1;
    
    // Choose smaller node and recursively merge
    if (list1.val <= list2.val) {
        list1.next = mergeTwoLists(list1.next, list2);
        return list1;
    } else {
        list2.next = mergeTwoLists(list1, list2.next);
        return list2;
    }
}
```

## Step-by-Step Example

Let's merge `[1, 2, 4]` and `[1, 3, 4]`:

1. **Initial**: list1→1→2→4, list2→1→3→4, dummy→null
2. **Compare 1 and 1**: Take list1's 1, dummy→1
3. **Compare 2 and 1**: Take list2's 1, dummy→1→1
4. **Compare 2 and 3**: Take list1's 2, dummy→1→1→2
5. **Compare 4 and 3**: Take list2's 3, dummy→1→1→2→3
6. **Compare 4 and 4**: Take list1's 4, dummy→1→1→2→3→4
7. **list1 empty**: Attach remaining list2, dummy→1→1→2→3→4→4

Result: `[1, 1, 2, 3, 4, 4]`

## Time Complexity

- **O(n + m)** where n and m are the lengths of the two lists
  - We visit each node exactly once

## Space Complexity

- **Iterative**: O(1) - Only using a constant amount of extra space
- **Recursive**: O(n + m) - Due to recursion call stack depth

## Common Mistakes to Avoid

1. **Forgetting the dummy node**: Makes edge cases harder to handle
2. **Not handling empty lists**: Always check for null inputs
3. **Creating new nodes**: The problem asks to reuse existing nodes
4. **Forgetting remaining nodes**: After one list is exhausted, attach the other

## When to Use Each Approach

- **Iterative**: Better space complexity, more straightforward
- **Recursive**: More elegant, easier to understand the logic

Both approaches are valid, but iterative is generally preferred in production code due to better space complexity and avoiding potential stack overflow issues with very long lists.

The key insight is recognizing that since both lists are already sorted, we only need to compare the current heads and choose the smaller one at each step.
