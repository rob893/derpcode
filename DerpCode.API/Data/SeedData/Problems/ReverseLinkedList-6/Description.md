Given the `head` of a singly linked list, reverse the list, and return the reversed list.

## Example 1

**Input:** `head = [1,2,3,4,5]`
**Output:** `[5,4,3,2,1]`

```
Original:  1 -> 2 -> 3 -> 4 -> 5 -> null
Reversed:  5 -> 4 -> 3 -> 2 -> 1 -> null
```

## Example 2

**Input:** `head = [1,2]`
**Output:** `[2,1]`

```
Original:  1 -> 2 -> null
Reversed:  2 -> 1 -> null
```

## Example 3

**Input:** `head = []`
**Output:** `[]`

## Constraints

- The number of nodes in the list is the range `[0, 5000]`
- `-5000 <= Node.val <= 5000`

## Follow-up

A linked list can be reversed either iteratively or recursively. Could you implement both?

## Function Signature

You need to implement a function that takes the head of a linked list and returns the head of the reversed list.

> 💡 **Key Insight**: The trick is to keep track of three pointers as you traverse the list: the previous node, the current node, and the next node. This allows you to reverse the link direction without losing track of where you are in the list.

## Approaches to Consider

- **Iterative Approach**: Use three pointers to reverse links one by one
- **Recursive Approach**: Reverse the rest of the list first, then fix the current connection
- **Stack Approach**: Push all nodes onto a stack, then pop them to rebuild the list

## Edge Cases to Handle

- **Empty list**: Return null
- **Single node**: Return the same node
- **Two nodes**: Simple swap of next pointers
