# Binary Tree Level Order Traversal - Solution Explanation

## Understanding the Problem

Level order traversal, also known as **breadth-first search (BFS)**, visits nodes level by level from left to right. Unlike the depth-first traversals (inorder, preorder, postorder), level order traversal explores all nodes at the current depth before moving to nodes at the next depth level.

For this problem, we want to return a **flat array** containing all node values in level order.

## Approach 1: Iterative Solution with Queue

The most common and intuitive approach uses a queue data structure to process nodes level by level.

```python
def levelOrder(root):
    if not root:
        return []

    result = []
    queue = [root]

    while queue:
        # Process current node
        node = queue.pop(0)
        result.append(node.val)

        # Add children for next level
        if node.left:
            queue.append(node.left)
        if node.right:
            queue.append(node.right)

    return result
```

**Time Complexity:** O(n) - we visit each node exactly once
**Space Complexity:** O(w) - where w is the maximum width of the tree (queue size)

## Example Walkthrough

For tree `[1,2,3,4,5,null,6]`:

```
       1
      / \
     2   3
    / \   \
   4   5   6
```

**Level order traversal steps:**

1. Start with root 1 in queue: [1]
2. Process 1, add its children: queue becomes [2, 3], result = [1]
3. Process 2, add its children: queue becomes [3, 4, 5], result = [1, 2]
4. Process 3, add its children: queue becomes [4, 5, 6], result = [1, 2, 3]
5. Process 4 (no children): queue becomes [5, 6], result = [1, 2, 3, 4]
6. Process 5 (no children): queue becomes [6], result = [1, 2, 3, 4, 5]
7. Process 6 (no children): queue becomes [], result = [1, 2, 3, 4, 5, 6]

**Result:** [1, 2, 3, 4, 5, 6] ## Key Insights

- Level order traversal is naturally iterative and uses a queue (FIFO) data structure
- Each node is processed exactly once, making it very efficient
- The queue ensures we process all nodes at the current level before moving to the next level
- This traversal is particularly useful for problems involving tree levels, finding the width of a tree, or printing trees level by level

## Common Mistakes

1. **Using a stack instead of a queue** - This would give you a different traversal order
2. **Not handling null nodes properly** - Always check if a node exists before adding it to the queue
3. **Forgetting base cases** - Handle empty trees correctly

The level order traversal is fundamental for many tree algorithms and is worth mastering!

```

**Time Complexity:** O(n)
**Space Complexity:** O(h) - where h is the height of the tree (recursion stack)

## Example Walkthrough

For tree `[1,2,3,4,5,null,6]`:

```

       1
      / \
     2   3
    / \   \

4 5 6

```

**Iterative approach steps:**
1. Start with queue = [1], level 0
2. Process level 0: queue = [1] → result = [[1]], next queue = [2, 3]
3. Process level 1: queue = [2, 3] → result = [[1], [2, 3]], next queue = [4, 5, 6]
4. Process level 2: queue = [4, 5, 6] → result = [[1], [2, 3], [4, 5, 6]], next queue = []
5. Queue empty, return result

**Result:** [[1], [2, 3], [4, 5, 6]]

## Key Insights

- **Queue-based approach**: Natural for level-order traversal since queues are FIFO (first-in, first-out)
- **Level tracking**: The key is to process all nodes at the current level before moving to the next
- **2D result structure**: Each level gets its own sub-array in the final result
- **BFS vs DFS**: Level order is BFS, while inorder/preorder/postorder are DFS approaches

## Common Variations

1. **Right-to-left level order**: Process nodes from right to left at each level
2. **Zigzag level order**: Alternate between left-to-right and right-to-left
3. **Level order bottom-up**: Return levels from bottom to top
4. **Level averages**: Calculate the average value at each level

## Performance Considerations

- **Iterative approach**: Better space complexity for balanced trees
- **Recursive approach**: More intuitive but uses call stack space
- **Queue choice**: Use `collections.deque` in Python for O(1) append/popleft operations
- **Memory optimization**: For very wide trees, consider streaming approaches

Try implementing both iterative and recursive solutions to understand the different ways to think about tree traversal!
```
