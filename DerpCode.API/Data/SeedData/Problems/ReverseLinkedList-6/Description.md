Given the `head` of a singly linked list, reverse the list, and return the head of the reversed list.

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
