# Mirror 2D Array

Mirroring a 2D array across the vertical axis is just a fancy way of saying: **reverse each row**.

## Key Idea

For every row:

- swap the leftmost and rightmost elements,
- then move inward until the pointers meet.

If a row has $c$ columns, for each index $j$ (0-based) you swap:

$$
row[j] \leftrightarrow row[c - 1 - j]
$$

You only need to do this for $j$ from $0$ to $\lfloor (c-1)/2 \rfloor$.

## Algorithm

For each row in `matrix`:

1. Set `left = 0`, `right = row.length - 1`.
2. While `left < right`:
   - swap `row[left]` and `row[right]`
   - increment `left`, decrement `right`

## Complexity

- Time: $O(r \cdot c)$
- Space: $O(1)$ if you reverse in-place, otherwise $O(r \cdot c)$ for a new matrix

## Reference C# Implementation

```csharp
public class Solution
{
    public static int[][] Mirror2DArray(int[][] matrix)
    {
        var result = new int[matrix.Length][];

        for (int i = 0; i < matrix.Length; i++)
        {
            var row = (int[])matrix[i].Clone();
            Array.Reverse(row);
            result[i] = row;
        }

        return result;
    }
}
```
