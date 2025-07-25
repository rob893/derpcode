Design a data structure that follows the constraints of a **Least Recently Used (LRU) cache**.

## Requirements

Implement the `LRUCache` class:

- `LRUCache(int capacity)` - Initialize the LRU cache with positive size capacity
- `int get(int key)` - Return the value of the key if the key exists, otherwise return `-1`
- `void put(int key, int value)` - Update the value of the key if the key exists. Otherwise, add the key-value pair to the cache. If the number of keys exceeds the capacity from this operation, evict the least recently used key

> ⚡ **Performance Requirement**: The functions `get` and `put` must each run in **O(1)** average time complexity.

## Example

```java
LRUCache lRUCache = new LRUCache(2);
lRUCache.put(1, 1); // cache is {1=1}
lRUCache.put(2, 2); // cache is {1=1, 2=2}
lRUCache.get(1);    // return 1, cache is {2=2, 1=1} (1 becomes most recent)
lRUCache.put(3, 3); // LRU key was 2, evicts key 2, cache is {1=1, 3=3}
lRUCache.get(2);    // returns -1 (not found)
lRUCache.put(4, 4); // LRU key was 1, evicts key 1, cache is {3=3, 4=4}
lRUCache.get(1);    // return -1 (not found)
lRUCache.get(3);    // return 3
lRUCache.get(4);    // return 4
```

## Key Concepts

- **Least Recently Used**: When the cache reaches capacity, the item that hasn't been accessed for the longest time gets evicted
- **Access**: Both `get()` and `put()` operations count as accessing an item
- **Recency Order**: Recently accessed items should be considered "most recent"",
