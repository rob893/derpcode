# Binary Tree Level Order Traversal

Given the `root` of a binary tree, return the **level order traversal** of its nodes' values. (i.e., from left to right, level by level).

**Level order traversal** returns an array of the values of the node in level order.

## Examples

**Example 1:**

Input: root = [1,null,2,3]

```
   1
    \
     2
    /
   3
```

Output: [1,2,3]

**Example 2:**

Input: root = []

Output: []

**Example 3:**

Input: root = [1]

Output: [1]

**Example 4:**

Input: root = [1,2,3,4,5,null,6]

```
       1
      / \
     2   3
    / \   \
   4   5   6
```

Output: [1,2,3,4,5,6]

## Constraints

- The number of nodes in the tree is in the range `[0, 2000]`.
- `-1000 <= Node.val <= 1000`

## Challenge

Can you solve it using both iterative and recursive approaches?
