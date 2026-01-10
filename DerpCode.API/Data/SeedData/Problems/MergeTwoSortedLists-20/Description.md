You are given the heads of two sorted linked lists `list1` and `list2`.

Merge the two lists into one **sorted** list. The list should be made by splicing together the nodes of the first two lists.

Return the head of the merged linked list.

## Requirements

Implement a function with the following signature:

- `ListNode? mergeTwoLists(ListNode? list1, ListNode? list2)` - Return the head of the merged sorted linked list

### Constraints:

- Both `list1` and `list2` are sorted in **non-decreasing order**
- The number of nodes in both lists is in the range `[0, 50]`
- The merged list should be created by re-using the existing nodes (not creating new nodes)

## Examples

```java
// Example 1:
list1 = [1, 2, 4]
list2 = [1, 3, 4]
mergeTwoLists(list1, list2);  // returns [1, 1, 2, 3, 4, 4]

// Example 2:
list1 = []
list2 = []
mergeTwoLists(list1, list2);  // returns []

// Example 3:
list1 = []
list2 = [0]
mergeTwoLists(list1, list2);  // returns [0]
```

## Key Concepts

- **Sorted Lists**: Both input lists are already sorted
- **Node Reuse**: Don't create new nodes; reuse existing ones
- **Merge Operation**: Combine two sorted sequences into one sorted sequence
