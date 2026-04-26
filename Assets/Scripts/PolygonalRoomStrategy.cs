using System.Collections.Generic;
using UnityEngine;

public class PolygonalRoomStrategy
{
    private int width;
    private int height;
    private RectInt[] squares;

    public void SquarePolygonRoom(int[,,] grid)
    {
        if(Random.Range(0, 1f) < 0.5f)
        {
            SquareRoom(grid);
            return;
        }

        width = grid.GetLength(0);
        height = grid.GetLength(1);

        squares = new RectInt[]
        {
            new RectInt(width / 2 - Mathf.RoundToInt(height * 0.1f), 0, width / 2 - Mathf.RoundToInt(height * 0.1f), height),
            new RectInt(0, height / 2 - Mathf.RoundToInt(width * 0.1f), width, height / 2 - Mathf.RoundToInt(width * 0.1f))
        };

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (squares[0].Contains(x, y) || squares[1].Contains(x, y))
                {
                    grid[x, y, 0] = 1;
                }
            }
        }
    }

    public IEnumerable<(Vector2Int, Vector2Int)> NextRoomPosition()
    {
        if (squares == null)
            yield break;

        RectInt intersection = squares[0].Intersect(squares[1]);
        Vector2Int[] corners = new Vector2Int[]
        {
            intersection.max,
            new Vector2Int(intersection.xMax, intersection.yMin - 1),
            new Vector2Int(intersection.xMin - 1, intersection.yMax),
            intersection.min - Vector2Int.one
        };
        foreach (var corner in corners)
        {
            foreach (var dir in Direction.diagonalDirections)
            {
                Vector2Int point = corner + dir;

                if (!squares[0].Contains(point) && !squares[1].Contains(point))
                {
                    yield return (point, dir);
                }
            }
        }
    }

    private static void SquareRoom(int[,,] grid)
    {
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                grid[x, y, 0] = 1;
            }
        }
    }
}
