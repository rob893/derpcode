Given the `root` of a binary tree, invert the tree, and return its root.

## What does "invert" mean?

Inverting a binary tree means swapping the left and right children of **every** node in the tree. This creates a mirror image of the original tree.

## Example 1

**Input:**

```
     4
   /   \
  2     7
 / \   / \
1   3 6   9
```

**Output:**

```
     4
   /   \
  7     2
 / \   / \
9   6 3   1
```

## Example 2

**Input:**

```
  2
 / \
1   3
```

**Output:**

```
  2
 / \
3   1
```

## Example 3

**Input:**

```
[]
```

**Output:**

```
[]
```

## Constraints

- The number of nodes in the tree is in the range `[0, 100]`
- `-100 <= Node.val <= 100`

> 💡 **Trivia**: This problem was inspired by a real interview question at Google! The story goes that the original creator of Homebrew was rejected from Google because they couldn't solve this problem on a whiteboard. Don't let that intimidate you though - it's actually quite straightforward once you think about it!
