# Reverse Linked List - Complete Solution Guide

The "Reverse Linked List" problem is a fundamental data structure challenge that every programmer should master. It's one of the most commonly asked interview questions and demonstrates core concepts of pointer manipulation and linked list traversal.

## Problem Understanding

Given a singly linked list, we need to reverse the direction of all the links so that the last node becomes the first, and the first becomes the last. The key challenge is doing this without losing any nodes in the process.

## Visual Example

**Original List:**

```
1 -> 2 -> 3 -> 4 -> 5 -> null
```

**Reversed List:**

```
5 -> 4 -> 3 -> 2 -> 1 -> null
```

At each step, we're changing the direction of the arrow (the `next` pointer).

## Solution Approaches

### Approach 1: Iterative Solution (Recommended)

The iterative approach uses three pointers to keep track of the reversal process:

#### Algorithm:

1. Initialize three pointers: `prev = null`, `current = head`, `next = null`
2. While `current` is not null:
   - Store the next node: `next = current.next`
   - Reverse the link: `current.next = prev`
   - Move pointers forward: `prev = current`, `current = next`
3. Return `prev` (which is now the new head)

#### Time Complexity: O(n)

- We visit each node exactly once
- n = number of nodes in the list

#### Space Complexity: O(1)

- We only use a constant amount of extra space

### Approach 2: Recursive Solution

The recursive approach reverses the list by solving the subproblem first:

#### Algorithm:

1. **Base Case**: If head is null or head.next is null, return head
2. **Recursive Case**:
   - Recursively reverse the rest of the list
   - Fix the connection: `head.next.next = head`
   - Break the old connection: `head.next = null`
   - Return the new head

#### Time Complexity: O(n)

#### Space Complexity: O(n)

- Due to recursion stack depth

### Approach 3: Stack-based Solution

This approach uses a stack to reverse the order:

#### Algorithm:

1. Traverse the list and push all nodes onto a stack
2. Pop nodes from the stack and rebuild the list
3. Return the new head

#### Time Complexity: O(n)

#### Space Complexity: O(n)

- For the stack storage

## Complete C# Implementation

### Iterative Solution

```csharp
public class ListNode
{
    public int val;
    public ListNode next;

    public ListNode(int val = 0, ListNode next = null)
    {
        this.val = val;
        this.next = next;
    }
}

public class Solution
{
    public ListNode ReverseList(ListNode head)
    {
        ListNode prev = null;
        ListNode current = head;

        while (current != null)
        {
            ListNode next = current.next;  // Store next node
            current.next = prev;           // Reverse the link
            prev = current;                // Move prev forward
            current = next;                // Move current forward
        }

        return prev;  // prev is now the new head
    }
}
```

### Recursive Solution

```csharp
public class Solution
{
    public ListNode ReverseList(ListNode head)
    {
        // Base case
        if (head == null || head.next == null)
            return head;

        // Recursively reverse the rest of the list
        ListNode newHead = ReverseList(head.next);

        // Reverse the current connection
        head.next.next = head;
        head.next = null;

        return newHead;
    }
}
```

## Step-by-Step Walkthrough

Let's trace through the iterative solution with `[1,2,3]`:

**Initial State:**

```
prev = null, current = 1 -> 2 -> 3 -> null
```

**Step 1:**

```
next = 2 -> 3 -> null
current.next = prev  // 1 -> null
prev = 1 -> null, current = 2 -> 3 -> null
```

**Step 2:**

```
next = 3 -> null
current.next = prev  // 2 -> 1 -> null
prev = 2 -> 1 -> null, current = 3 -> null
```

**Step 3:**

```
next = null
current.next = prev  // 3 -> 2 -> 1 -> null
prev = 3 -> 2 -> 1 -> null, current = null
```

**Result:** `prev` points to `3 -> 2 -> 1 -> null`

## Common Mistakes to Avoid

1. **Losing the next node**: Always store `next` before modifying `current.next`
2. **Wrong return value**: Return `prev`, not `current` (which will be null)
3. **Forgetting edge cases**: Handle null input and single-node lists
4. **Infinite loops**: Make sure to advance pointers correctly

## Interview Tips

- **Start with the iterative solution** - it's more intuitive and uses less space
- **Draw it out** - visualize the pointer movements on paper or whiteboard
- **Discuss both approaches** - shows understanding of trade-offs
- **Handle edge cases** - empty list, single node, two nodes
- **Optimize space** - mention that iterative is O(1) space vs recursive O(n)

## Practice Variations

Once you master the basic reversal, try these related problems:

- Reverse Linked List II (reverse between positions m and n)
- Reverse Nodes in k-Group
- Palindrome Linked List
- Merge Two Sorted Lists

## Real-World Applications

- **Undo functionality**: Reversing a sequence of operations
- **Browser history**: Going backward through visited pages
- **Text editors**: Reversing character or word order
- **Data processing**: Reversing the order of records

This problem is a gateway to understanding more complex linked list manipulations and is essential for technical interviews!
