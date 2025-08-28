# Invert Binary Tree - Complete Solution Guide

The "Invert Binary Tree" problem is a classic tree manipulation challenge that demonstrates fundamental tree traversal concepts. The goal is to create a mirror image of the given binary tree by swapping the left and right children of every node.

## Problem Understanding

When we "invert" a binary tree, we're essentially creating its mirror image. For every node in the tree:

1. The left child becomes the right child
2. The right child becomes the left child
3. This operation is applied recursively to all nodes

## Visual Example

**Original Tree:**

```
     4
   /   \
  2     7
 / \   / \
1   3 6   9
```

**Inverted Tree:**

```
     4
   /   \
  7     2
 / \   / \
9   6 3   1
```

Notice how at each level, the children are swapped:

- Node 4: children 2,7 become 7,2
- Node 2: children 1,3 become 3,1
- Node 7: children 6,9 become 9,6

## Solution Approaches

### Approach 1: Recursive Solution (Recommended)

The recursive approach is the most intuitive and elegant solution:

#### Algorithm:

1. **Base Case**: If the current node is null, return null
2. **Recursive Case**:
   - Recursively invert the left subtree
   - Recursively invert the right subtree
   - Swap the left and right children of the current node
   - Return the current node

#### Time Complexity: O(n)

- We visit each node exactly once
- n = number of nodes in the tree

#### Space Complexity: O(h)

- h = height of the tree (due to recursion stack)
- Best case: O(log n) for balanced tree
- Worst case: O(n) for skewed tree

### Approach 2: Iterative Solution using Queue (BFS)

This approach uses a queue to perform level-order traversal:

#### Algorithm:

1. If root is null, return null
2. Create a queue and add the root
3. While queue is not empty:
   - Dequeue a node
   - Swap its left and right children
   - Add non-null children to the queue
4. Return the original root

#### Time Complexity: O(n)

#### Space Complexity: O(w)

- w = maximum width of the tree (queue size)

### Approach 3: Iterative Solution using Stack (DFS)

Similar to the queue approach but uses a stack for depth-first traversal:

#### Algorithm:

1. If root is null, return null
2. Create a stack and push the root
3. While stack is not empty:
   - Pop a node
   - Swap its left and right children
   - Push non-null children to the stack
4. Return the original root

## Complete C# Implementation

### Recursive Solution

```csharp
public class TreeNode
{
    public int val;
    public TreeNode left;
    public TreeNode right;

    public TreeNode(int val = 0, TreeNode left = null, TreeNode right = null)
    {
        this.val = val;
        this.left = left;
        this.right = right;
    }
}

public class Solution
{
    public TreeNode InvertTree(TreeNode root)
    {
        // Base case: if node is null, return null
        if (root == null)
            return null;

        // Recursively invert left and right subtrees
        TreeNode left = InvertTree(root.left);
        TreeNode right = InvertTree(root.right);

        // Swap the children
        root.left = right;
        root.right = left;

        return root;
    }
}
```

### Iterative Solution (Queue)

```csharp
using System.Collections.Generic;

public class Solution
{
    public TreeNode InvertTree(TreeNode root)
    {
        if (root == null)
            return null;

        Queue<TreeNode> queue = new Queue<TreeNode>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            TreeNode current = queue.Dequeue();

            // Swap left and right children
            TreeNode temp = current.left;
            current.left = current.right;
            current.right = temp;

            // Add children to queue for processing
            if (current.left != null)
                queue.Enqueue(current.left);
            if (current.right != null)
                queue.Enqueue(current.right);
        }

        return root;
    }
}
```

## Common Mistakes to Avoid

1. **Forgetting the base case**: Always check if the current node is null
2. **Not returning the root**: The function should return the root of the inverted tree
3. **Modifying in wrong order**: In recursive solution, make sure to invert subtrees before swapping
4. **Infinite recursion**: Ensure you're calling the function on children, not the current node

## Practice Variations

Once you master the basic inversion, try these related problems:

- Mirror Tree (check if two trees are mirrors of each other)
- Symmetric Tree (check if a tree is symmetric)
- Flatten Binary Tree to Linked List
- Convert Binary Tree to Doubly Linked List

## Interview Tips

- **Start with the recursive solution** - it's cleaner and easier to explain
- **Draw the tree on paper** - visual representation helps with understanding
- **Discuss time and space complexity** - interviewers love this
- **Mention the Google/Homebrew story** - shows you know the problem's history
- **Consider edge cases**: empty tree, single node, skewed tree

This problem is an excellent introduction to tree manipulation and showcases the elegance of recursive solutions for tree problems!
