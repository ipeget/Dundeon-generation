using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonData", menuName = "Scriptable Objects/DungeonData")]
public class DungeonData : ScriptableObject
{
    [SerializeField] private int roomCount;
    public int RoomCount => roomCount;

    [SerializeField] private int maxDepth;
    public int MaxDepth => maxDepth;

    [SerializeField] private Vector2Int roomSize;
    public Vector2Int RoomSize => roomSize;

    [SerializeField] private Vector2Int dungeonSize;
    public Vector2Int DungeonSize => dungeonSize;

    [SerializeField] private int rows;
    public int Rows => rows;

    public delegate void RoomView(int[,,] grid);

    public RoomView[] RoomViews = new RoomView[]
    {
        SquareRoom,
        EllipseRoom,
    };
    
    public Vector2Int AutoSize(Room[] rooms)
    {
        int x = 0;
        int y = 0;
        for (int i = 0; i < rooms.Length; i++)
        {
            if(rooms[i] == null) continue;

            if(x < rooms[i].rect.xMax)
                x = rooms[i].rect.xMax;
            if(y < rooms[i].rect.yMax)
                y = rooms[i].rect.yMax;
        }
        return new Vector2Int(x, y);
    }

    private static void SquareRoom(int[,,] grid)
    {
        for(int y = 0; y < grid.GetLength(1); y++)
        {
            for(int x = 0; x < grid.GetLength(0); x++)
            {
                grid[x, y, 0] = 1;
            }
        }
    }

    private static void EllipseRoom(int[,,] grid)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        Vector2 center = new Vector2((width - 1) / 2f, (height - 1) / 2f);
        float a = width / 2f;
        float b = height / 2f;
        float epsilon = 0.1f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = (x - center.x) / a;
                float ny = (y - center.y) / b;
                float v = nx * nx + ny * ny;

                if(v <= 1 + epsilon)
                {
                    grid[x, y, 0] = 1;
                }
            }
        }
    }

   
}
