# LRU Cache Implementation

An LRU (Least Recently Used) cache is a data structure that maintains a fixed capacity and evicts the least recently used item when the capacity is exceeded. This is a fundamental caching strategy used in operating systems, databases, and web applications.

## Key Concepts

The optimal solution combines two data structures to achieve O(1) time complexity for both get and put operations:

1. **Hash Table (Dictionary)**: Provides O(1) access to cache entries by key
2. **Doubly Linked List**: Maintains the order of usage, with most recently used items at the head

## Why This Combination Works

- **Hash Table**: Gives us instant access to any node by key, eliminating the need to traverse a list
- **Doubly Linked List**: Allows us to efficiently move nodes to the front and remove nodes from the back in O(1) time
- **Dummy Nodes**: Using dummy head and tail nodes simplifies edge cases when adding/removing nodes

## Algorithm Walkthrough

### Get Operation

1. Check if the key exists in the hash table
2. If it exists:
   - Retrieve the node from the hash table
   - Move the node to the head of the linked list (mark as most recently used)
   - Return the node's value
3. If it doesn't exist, return -1

### Put Operation

1. If the key already exists:
   - Update the node's value
   - Move the node to the head (mark as most recently used)
2. If the key doesn't exist:
   - Create a new node
   - If cache is at capacity, remove the tail node (least recently used) and its hash table entry
   - Add the new node to the head of the list
   - Add the new node to the hash table

### Linked List Operations

- **Add to Head**: Insert node right after the dummy head
- **Remove Node**: Update the previous and next pointers to bypass the node
- **Move to Head**: Remove the node from its current position and add it to the head
- **Remove Tail**: Remove the node just before the dummy tail

## Complete C# Implementation

```csharp
using System;
using System.Collections.Generic;

public class LRUCache
{
    private readonly int capacity;
    private readonly Dictionary<int, Node> cache;
    private readonly Node head;
    private readonly Node tail;

    public LRUCache(int capacity)
    {
        this.capacity = capacity;
        this.cache = new Dictionary<int, Node>();

        // Create dummy head and tail nodes to simplify edge cases
        this.head = new Node(-1, -1);
        this.tail = new Node(-1, -1);
        this.head.Next = this.tail;
        this.tail.Prev = this.head;
    }

    public int Get(int key)
    {
        if (this.cache.ContainsKey(key))
        {
            Node node = this.cache[key];
            // Move to head since it was accessed (mark as most recently used)
            this.MoveToHead(node);
            return node.Value;
        }
        return -1;
    }

    public void Put(int key, int value)
    {
        if (this.cache.ContainsKey(key))
        {
            // Update existing key
            Node node = this.cache[key];
            node.Value = value;
            this.MoveToHead(node);
        }
        else
        {
            Node newNode = new Node(key, value);

            if (this.cache.Count >= this.capacity)
            {
                // Remove least recently used item (tail)
                Node tailNode = this.RemoveTail();
                this.cache.Remove(tailNode.Key);
            }

            this.cache[key] = newNode;
            this.AddToHead(newNode);
        }
    }

    private class Node
    {
        public int Key { get; set; }
        public int Value { get; set; }
        public Node Prev { get; set; }
        public Node Next { get; set; }

        public Node(int key, int value)
        {
            this.Key = key;
            this.Value = value;
        }
    }

    private void AddToHead(Node node)
    {
        // Add node right after head
        node.Prev = this.head;
        node.Next = this.head.Next;
        this.head.Next.Prev = node;
        this.head.Next = node;
    }

    private void RemoveNode(Node node)
    {
        // Remove an existing node from the linked list
        node.Prev.Next = node.Next;
        node.Next.Prev = node.Prev;
    }

    private void MoveToHead(Node node)
    {
        // Move certain node to head (most recently used)
        this.RemoveNode(node);
        this.AddToHead(node);
    }

    private Node RemoveTail()
    {
        // Remove the last node (least recently used)
        Node lastNode = this.tail.Prev;
        this.RemoveNode(lastNode);
        return lastNode;
    }
}
```

## Step-by-Step Example

Let's trace through the example with capacity = 2:

1. `LRUCache(2)` - Initialize empty cache

   - Cache: `{}`, List: `head ↔ tail`

2. `put(1, 1)` - Add first item

   - Cache: `{1: Node(1,1)}`, List: `head ↔ 1 ↔ tail`

3. `put(2, 2)` - Add second item

   - Cache: `{1: Node(1,1), 2: Node(2,2)}`, List: `head ↔ 2 ↔ 1 ↔ tail`

4. `get(1)` - Access key 1, move to head

   - Returns: `1`, List: `head ↔ 1 ↔ 2 ↔ tail`

5. `put(3, 3)` - Capacity exceeded, evict LRU (key 2)

   - Cache: `{1: Node(1,1), 3: Node(3,3)}`, List: `head ↔ 3 ↔ 1 ↔ tail`

6. `get(2)` - Key 2 was evicted
   - Returns: `-1`

## Time Complexity

- **Get**: O(1) - Hash table lookup + O(1) linked list operations
- **Put**: O(1) - Hash table operations + O(1) linked list operations

## Space Complexity

- **O(capacity)** - We store exactly `capacity` nodes in both the hash table and linked list

## Alternative Approaches

### Language-Specific Optimizations

- **JavaScript/TypeScript**: Can use `Map` which maintains insertion order
- **Python**: Can use `OrderedDict` which combines hash table with ordering
- **Java**: Can use `LinkedHashMap` with custom `removeEldestEntry`

These built-in data structures provide simpler implementations but the core concept remains the same: combining fast access with ordering information.

## Common Mistakes to Avoid

1. **Forgetting to update both structures**: Always keep the hash table and linked list in sync
2. **Not handling edge cases**: Empty cache, single item, capacity of 1
3. **Incorrect pointer management**: Be careful with prev/next pointer updates
4. **Missing dummy nodes**: They simplify boundary conditions significantly

The key insight is that we need both **fast access** (hash table) and **ordering information** (linked list) to achieve O(1) performance for an LRU cache.
