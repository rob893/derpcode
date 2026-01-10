# Binary Tree Postorder Traversal

Given the `root` of a binary tree, return the **postorder traversal** of its nodes' values.

**Postorder traversal** visits nodes in the following order:

1. Left subtree
2. Right subtree
3. Current node

## Tree Node Definition

The binary tree is represented using the following structure:

```java
class TreeNode {
    int val;
    TreeNode left;
    TreeNode right;
    TreeNode() {}
    TreeNode(int val) { this.val = val; }
    TreeNode(int val, TreeNode left, TreeNode right) {
        this.val = val;
        this.left = left;
        this.right = right;
    }
}
```

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

Output: [3,2,1]

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

Output: [4,5,2,6,3,1]

## Constraints

- The number of nodes in the tree is in the range `[0, 100]`.
- `-100 <= Node.val <= 100`

## Challenge

Can you solve it both recursively and iteratively?
