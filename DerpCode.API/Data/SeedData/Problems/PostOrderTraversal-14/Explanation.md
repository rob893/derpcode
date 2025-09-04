# Binary Tree Postorder Traversal - Solution Explanation

## Understanding the Problem

Postorder traversal is a depth-first traversal method where we visit nodes in the following order:

1. **Left subtree** (recursively)
2. **Right subtree** (recursively)
3. **Current node** (process/visit)

This is also known as **Left-Right-Root** traversal.

## Approach 1: Recursive Solution

The most intuitive approach uses recursion, which naturally handles the call stack for us.

```python
def postorderTraversal(root):
    result = []

    def traverse(node):
        if not node:
            return

        traverse(node.left)   # Visit left subtree
        traverse(node.right)  # Visit right subtree
        result.append(node.val)  # Visit current node

    traverse(root)
    return result
```

**Time Complexity:** O(n) - we visit each node exactly once
**Space Complexity:** O(h) - where h is the height of the tree (recursion stack)

## Approach 2: Iterative Solution

We can simulate the recursion using an explicit stack. Since postorder processes the current node _after_ its children, we need a slightly different approach than preorder or inorder traversal.

One technique is to use two stacks or modify our approach:

```python
def postorderTraversal(root):
    if not root:
        return []

    stack = [root]
    result = []

    while stack:
        node = stack.pop()
        result.append(node.val)

        # Add children in left-right order
        if node.left:
            stack.append(node.left)
        if node.right:
            stack.append(node.right)

    # Reverse the result to get postorder
    return result[::-1]
```

**Time Complexity:** O(n)
**Space Complexity:** O(n) - for the stack and result array

## Example Walkthrough

For tree `[1,2,3,4,5,null,6]`:

```
       1
      / \
     2   3
    / \   \
   4   5   6
```

**Postorder traversal steps:**

1. Visit left subtree of 1 → go to node 2
2. Visit left subtree of 2 → go to node 4
3. Node 4 has no children → **visit 4**
4. Back to node 2, visit right subtree → go to node 5
5. Node 5 has no children → **visit 5**
6. Back to node 2, visited both children → **visit 2**
7. Back to node 1, visit right subtree → go to node 3
8. Node 3 has no left child, visit right subtree → go to node 6
9. Node 6 has no children → **visit 6**
10. Back to node 3, visited right child → **visit 3**
11. Back to node 1, visited both children → **visit 1**

**Result:** [4, 5, 2, 6, 3, 1]

## Key Insights

- Postorder is particularly useful for operations where you need to process children before the parent (like calculating directory sizes, deleting nodes safely)
- The recursive solution is more intuitive but uses implicit stack space
- The iterative solution gives you more control over memory usage
- Postorder is the reverse of a modified preorder traversal (Root-Right-Left)

## Common Mistakes

1. **Processing nodes too early** - Remember that in postorder, you process the current node _after_ both children
2. **Incorrect stack operations** - When using iterative approach, be careful about the order of pushing children
3. **Forgetting base cases** - Always handle null/empty nodes properly

Try implementing both recursive and iterative solutions to fully understand the traversal pattern!
