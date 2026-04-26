using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class Dungeon
{
    private DungeonData data;
    private Vector2Int size;

    private Room[] rooms;
    private int[,,] grid;

    private int currentRoomCount = -1;

    public Dungeon(DungeonData data)
    {
        this.data = data;
        UpdateValues();
    }

    private void UpdateValues()
    {
        size = data.DungeonSize;
        rooms = new Room[data.RoomCount];
    }

    private void CreateRooms()
    {
        Vector2Int permisSize = size;
        Vector2Int center = Vector2Int.zero;

        CreateRoom(center, Vector2Int.one, permisSize);
    }

    private void CreateRoom(Vector2Int center, Vector2Int rotateDirection, Vector2Int size)
    {
        if (currentRoomCount == data.RoomCount - 1)
            return;

        currentRoomCount++;
        int[,,] grid = new int[size.x, size.y, Room.LAYER_COUNT];
        var strategy = new PolygonalRoomStrategy();
        strategy.SquarePolygonRoom(grid);
        if (rotateDirection != Vector2.zero || rotateDirection != Vector2Int.one)
            RotateMatrixToDirection(grid, rotateDirection, ref center);
        rooms[currentRoomCount] = new Room(center, size);
        rooms[currentRoomCount].grid = grid;
        rooms[currentRoomCount].GetRoomEdge();
        rooms[currentRoomCount].SetWallsAlongEdge();
        
        foreach (var pair in strategy.NextRoomPosition())
        {
            if(pair.Item1 == null || pair.Item2 == rotateDirection * -1) 
                return;

            CreateRoom(pair.Item1 + center, pair.Item2, size);
        }
    }

    private void CreateGrid()
    {
        size = data.AutoSize(rooms) * 2;
        grid = new int[size.x, size.y, Room.LAYER_COUNT];

        for (int layer = 0; layer < Room.LAYER_COUNT; layer++)
        {
            for (int i = 0; i < rooms.Length; i++)
            {
                if (rooms[i] == null) continue;

                int[,] matrix = rooms[i].GetLayer((LayerLevel)layer);
                int dx = rooms[i].rect.xMin + size.x / 2;
                int dy = rooms[i].rect.yMin + size.y / 2;

                for (int y = 0; y < matrix.GetLength(1); y++)
                {
                    for (int x = 0; x < matrix.GetLength(0); x++)
                    {
                        grid[x + dx, y + dy, layer] = matrix[x, y];
                    }
                }
            }
        }
    }

    public int[,,] Create()
    {
        UpdateValues();
        CreateRooms();
        CreateGrid();
        return grid;
    }

    public void RotateMatrixToDirection(int[,,] matrix, Vector2Int direction, ref Vector2Int center)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        int[,,] result = new int[rows, cols, Room.LAYER_COUNT];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int targetX = x;
                int targetY = y;

                if (direction.x == -1)
                    targetX = cols - 1 - x;

                if (direction.y == -1)
                    targetY = rows - 1 - y;

                result[targetY, targetX, 0] = matrix[y, x, 0];
            }
        }

        if (direction.x == -1)
            center.x -= rows;
        if (direction.y == -1)
            center.y -= cols;

        matrix = result;
    }
}

public class Room
{
    public const int LAYER_COUNT = 3;

    public int[,,] grid; // x, y, layerLevel
    private HashSet<Vector2Int> edge;
    private HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
    public RectInt rect;

    public Room(RectInt rect)
    {
        this.rect = rect;
        grid = new int[rect.width, rect.height, LAYER_COUNT];
    }
    public Room(Vector2Int center, Vector2Int size)
    {
        rect = new RectInt(center, size);
        grid = new int[size.x, size.y, LAYER_COUNT];
    }
    public Room(int x, int y, int width, int height)
    {
        rect = new RectInt(x, y, width, height);
        grid = new int[width, height, LAYER_COUNT];
    }

    public void FillAll()
    {
        for (int y = 0; y < rect.height; y++)
        {
            for (int x = 0; x < rect.width; x++)
            {
                floorPositions.Add(new Vector2Int(x, y));
                grid[x, y, (int)LayerLevel.Floor] = 1;
            }
        }
    }

    public void SetWallsAlongEdge()
    {
        foreach (var pos in edge)
        {
            if (grid[pos.x, pos.y, (int)LayerLevel.Door] != 0)
                continue;

            grid[pos.x, pos.y, (int)LayerLevel.Wall] = 1;
        }
    }

    public void SetDoor(Vector2Int pos)
    {
        if (edge.Contains(pos))
            grid[pos.x, pos.y, (int)LayerLevel.Door] = 1;
    }

    public HashSet<Vector2Int> GetRoomEdge()
    {
        if (edge == null)
            edge = FindEdge(GetLayer(LayerLevel.Floor));
        return edge;
    }

    public int[,] GetLayer(LayerLevel layer)
    {
        int l0 = grid.GetLength(0);
        int l1 = grid.GetLength(1);

        int[,] matrix = new int[l0, l1];

        for (int y = 0; y < l1; y++)
        {
            for (int x = 0; x < l0; x++)
            {
                matrix[x, y] = grid[x, y, (int)layer];
            }
        }

        return matrix;
    }

    private static HashSet<Vector2Int> FindEdge(int[,] matrix)
    {
        var edge = new HashSet<Vector2Int>();
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        (int dx, int dy)[] dirs = { (0, 1), (1, 0), (0, -1), (-1, 0) };

        for (int y = 0; y < cols; y++)
        {
            for (int x = 0; x < rows; x++)
            {
                if (matrix[x, y] == 0) continue;
                foreach (var (dx, dy) in dirs)
                {
                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx < 0 || nx >= rows || ny < 0 || ny >= cols || matrix[nx, ny] == 0)
                        edge.Add(new Vector2Int(x, y));
                }
            }
        }

        return edge;
    }

    public void Fill(Vector2Int pos, LayerLevel level)
    {
        grid[pos.x, pos.y, (int)level] = 1;
    }

    public void Remove(Vector2Int pos, LayerLevel level)
    {
        grid[pos.x, pos.y, (int)level] = 0;
    }

    public bool Fill(Vector2Int pos)
    {
        foreach (var neighbor in pos.GetNeighbors())
        {
            if (floorPositions.Contains(neighbor))
            {
                grid[pos.x, pos.y, (int)LayerLevel.Floor] = 1;
                return floorPositions.Add(pos);
            }
        }
        return false;
    }

    public bool Remove(Vector2Int pos)
    {
        grid[pos.x, pos.y, (int)LayerLevel.Floor] = 0;
        return floorPositions.Remove(pos);
    }
}


public enum LayerLevel
{
    Floor,
    Door,
    Wall,
    Decoration
}

public static class Vector2IntExtensions
{
    public static IEnumerable<Vector2Int> GetNeighbors(this Vector2Int pos, bool includeDiagonals = false)
    {
        if (includeDiagonals)
        {
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    if (dx != 0 || dy != 0)
                        yield return new Vector2Int(pos.x + dx, pos.y + dy);
        }
        else
        {
            yield return new Vector2Int(pos.x + 1, pos.y);
            yield return new Vector2Int(pos.x - 1, pos.y);
            yield return new Vector2Int(pos.x, pos.y + 1);
            yield return new Vector2Int(pos.x, pos.y - 1);
        }
    }
}