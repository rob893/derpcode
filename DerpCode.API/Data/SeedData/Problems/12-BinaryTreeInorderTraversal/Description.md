Given the root of a binary tree, return the **inorder traversal** of its node values.

## Requirements

Implement a function with the following signature:

- `List<int> inorderTraversal(TreeNode root)` - Return the inorder traversal of the binary tree

The inorder traversal visits nodes in the following order: **Left, Root, Right**.

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

### Example 1:

```
Input: root = [1,null,2,3]
   1
    \
     2
    /
   3

Output: [1,3,2]
```

### Example 2:

```
Input: root = []
Output: []
```

### Example 3:

```
Input: root = [1]
Output: [1]
```

### Example 4:

```
Input: root = [1,2,3,4,5,null,6]
       1
      / \
     2   3
    / \   \
   4   5   6

Output: [4,2,5,1,3,6]
```

## Constraints

- The number of nodes in the tree is in the range `[0, 100]`
- `-100 <= Node.val <= 100`

## Follow-up

While the recursive solution is trivial, could you solve it iteratively?
