using UnityEngine;

public static class Direction
{
    public static readonly Vector2Int[] cardinalDirections = new Vector2Int[]
    {
            new(1, 0),
            new(0, 1),
            new(-1, 0),
            new(0, -1)
    };

    public static readonly Vector2Int[] allDirections = new Vector2Int[]
    {
            new(0, 1),
            new(1, 0),
            new(0, -1),
            new(-1, 0),
            new(1, 1),
            new(-1, 1),
            new(1, -1),
            new(-1, -1)
    };

    public static readonly Vector2Int[] diagonalDirections = new Vector2Int[]
    {
            new(1, 1),
            new(-1, 1),
            new(1, -1),
            new(-1, -1)
    };

    public static Vector2Int GetRandomCardinalDirection()
    {
        return cardinalDirections[Random.Range(0, cardinalDirections.Length)];
    }
}
