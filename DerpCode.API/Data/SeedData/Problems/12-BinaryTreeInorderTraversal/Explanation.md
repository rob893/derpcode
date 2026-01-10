# Binary Tree Inorder Traversal Explanation

## Understanding Inorder Traversal

Inorder traversal is one of the three main ways to traverse a binary tree. The order of visiting nodes is:

1. **Left subtree** - Visit all nodes in the left subtree first
2. **Root** - Visit the current node
3. **Right subtree** - Visit all nodes in the right subtree

This Left-Root-Right pattern gives us the "inorder" name.

## Recursive Approach

The most intuitive solution is recursive:

```python
def inorderTraversal(root):
    result = []

    def inorder(node):
        if node:
            inorder(node.left)    # Left
            result.append(node.val)  # Root
            inorder(node.right)   # Right

    inorder(root)
    return result
```

**Time Complexity:** O(n) where n is the number of nodes  
**Space Complexity:** O(h) where h is the height of the tree (due to recursion stack)

## Iterative Approach

For the iterative solution, we use a stack to simulate the recursion:

```python
def inorderTraversal(root):
    result = []
    stack = []
    current = root

    while current or stack:
        # Go to the leftmost node
        while current:
            stack.append(current)
            current = current.left

        # Current must be None here, so we backtrack
        current = stack.pop()
        result.append(current.val)  # Visit the node

        # Move to right subtree
        current = current.right

    return result
```

**Time Complexity:** O(n)  
**Space Complexity:** O(h) where h is the height of the tree (due to stack)

## Why Inorder Traversal Matters

- **Binary Search Trees (BST):** Inorder traversal of a BST gives you the values in sorted ascending order
- **Expression Trees:** In mathematical expression trees, inorder traversal can give you the original infix expression
- **Memory Management:** It's useful in tree serialization and deserialization

## Step-by-Step Example

For the tree `[1,null,2,3]`:

```
   1
    \
     2
    /
   3
```

1. Start at node 1
2. No left child, visit node 1 → result: [1]
3. Move to right child (node 2)
4. Node 2 has left child (node 3)
5. Visit node 3 → result: [1, 3]
6. Node 3 has no children, backtrack to node 2
7. Visit node 2 → result: [1, 3, 2]
8. Done!

The key insight is that we always go as far left as possible before visiting a node, then explore the right subtree.

## Common Pitfalls

1. **Forgetting the order:** Remember it's Left-Root-Right, not Root-Left-Right
2. **Stack management:** In the iterative approach, make sure to push nodes onto the stack before going left
3. **Null checks:** Always check if a node exists before accessing its properties
4. **Base case:** Handle empty trees (null root) properly

Both recursive and iterative solutions are valuable to understand, as they demonstrate different problem-solving approaches and help you think about tree traversal in multiple ways.
