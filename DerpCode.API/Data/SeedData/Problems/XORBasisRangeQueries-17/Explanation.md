This problem is basically: “support point updates and answer maximum possible XOR of any subset in a range.”

That’s a classic combination:

1. A **linear basis over XOR** (also called a “XOR basis” or “Gaussian elimination over $GF(2)$”) to represent all subset XOR values of a set.
2. A **segment tree** to merge bases for ranges and support updates.

## 1) XOR Linear Basis

If you take a multiset of integers and consider all possible XORs of its subsets, those values form a vector space over $GF(2)$.

A linear basis stores a small set of vectors (numbers) such that:

- Any subset XOR can be expressed as XOR of some basis vectors.
- The number of vectors is at most the number of bits (here we use 31 bits: 0..30).

### Insert

We keep an array `basis[bit]` where `basis[b]` holds a number whose highest set bit is `b`.

To insert `x`:

- For `bit` from high to low:
  - If `x` doesn’t have that bit, continue.
  - If `basis[bit] == 0`, set it to `x` and stop.
  - Otherwise eliminate the bit: `x ^= basis[bit]` and continue.

This is the XOR-version of Gaussian elimination.

### Maximum subset XOR

Once you have a basis, the maximum possible XOR value can be obtained greedily:

- Start `res = 0`.
- For `bit` from high to low:
  - If `res XOR basis[bit]` is larger than `res`, take it.

This works because each `basis[bit]` can only affect that highest bit and lower.

## 2) Segment Tree of Bases

For range queries with updates:

- Each leaf holds the basis for a single element.
- Each internal node stores the merged basis of its two children.

### Merge

To merge two bases `A` and `B`:

- Copy `A`.
- Insert every vector from `B` into the copy.

The basis size is at most 31, so merging is cheap.

### Complexity

Let $W$ be the bit-width (31) and $N$ the array size.

- Build: $O(N \cdot W)$
- Update: $O(\log N \cdot W)$
- Query: $O(\log N \cdot W)$

This passes comfortably even for large `N` and `Q`.

## Common pitfalls

- Remember queries are **1-indexed** in the statement; convert to 0-index internally.
- Do a **deep** comparison for arrays in drivers; reference-equality will fail even if values match.
