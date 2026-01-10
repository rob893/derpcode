# Binary Tree Preorder Traversal Explanation

## Understanding Preorder Traversal

Preorder traversal is one of the three main ways to traverse a binary tree. The order of visiting nodes is:

1. **Root** - Visit the current node first
2. **Left subtree** - Visit all nodes in the left subtree
3. **Right subtree** - Visit all nodes in the right subtree

This Root-Left-Right pattern gives us the "preorder" name.

## Recursive Approach

The most intuitive solution is recursive:

```python
def preorderTraversal(root):
    result = []

    def preorder(node):
        if node:
            result.append(node.val)     # Root
            preorder(node.left)         # Left
            preorder(node.right)        # Right

    preorder(root)
    return result
```

**Time Complexity:** O(n) where n is the number of nodes  
**Space Complexity:** O(h) where h is the height of the tree (due to recursion stack)

## Iterative Approach

For the iterative solution, we use a stack to simulate the recursion:

```python
def preorderTraversal(root):
    if not root:
        return []

    result = []
    stack = [root]

    while stack:
        current = stack.pop()
        result.append(current.val)  # Visit the node

        # Push right first, then left (since stack is LIFO)
        if current.right:
            stack.append(current.right)
        if current.left:
            stack.append(current.left)

    return result
```

**Time Complexity:** O(n)  
**Space Complexity:** O(h) where h is the height of the tree (due to stack)

## Why Preorder Traversal Matters

- **Expression Trees:** In mathematical expression trees, preorder traversal gives you the prefix notation
- **File System:** Directory listings often use preorder traversal (folder first, then contents)
- **Tree Copying:** Preorder is natural for creating copies of trees (create node first, then children)
- **Tree Serialization:** Many tree serialization formats use preorder traversal

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
2. Visit node 1 → result: [1]
3. No left child, move to right child (node 2)
4. Visit node 2 → result: [1, 2]
5. Node 2 has left child (node 3)
6. Visit node 3 → result: [1, 2, 3]
7. Node 3 has no children, done!

The key insight is that we always visit the current node **before** exploring its children.

## Common Pitfalls

1. **Forgetting the order:** Remember it's Root-Left-Right, not Left-Root-Right (that's inorder)
2. **Stack order in iterative:** Push right child first, then left child (since stack is LIFO)
3. **Null checks:** Always check if a node exists before accessing its properties
4. **Base case:** Handle empty trees (null root) properly

## Comparison with Other Traversals

- **Preorder (Root-Left-Right):** Good for copying trees, prefix expressions
- **Inorder (Left-Root-Right):** Good for BSTs (gives sorted order), infix expressions
- **Postorder (Left-Right-Root):** Good for deleting trees, calculating sizes

Both recursive and iterative solutions are valuable to understand, as they demonstrate different problem-solving approaches and help you think about tree traversal in multiple ways.
