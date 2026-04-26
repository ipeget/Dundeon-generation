using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GenerateDungeonAlgorithms
{
    #region RandomWalk
    public static HashSet<Vector2Int> RandomWalkGeneration(Vector2Int startPosition, int iterations, int stepCount)
    {
        var path = new HashSet<Vector2Int>() { startPosition };

        Vector2Int pastPosition = startPosition;

        for (int i = 0; i < iterations; i++)
        {
            for (int j = 0; j < stepCount; j++)
            {
                Vector2Int newPosition = pastPosition + Direction.GetRandomCardinalDirection();
                path.Add(newPosition);
                pastPosition = newPosition;
            }
        }

        return path;
    }


    public static HashSet<Vector2Int> CorridorRandomWalkGeneration(Vector2Int startPosition, int length, out Vector2Int endPosition)
    {
        var path = new HashSet<Vector2Int>() { startPosition };

        Vector2Int direction = Direction.GetRandomCardinalDirection();
        Vector2Int pastPosition = startPosition;
        for (int i = 0; i < length; i++)
        {
            Vector2Int newPosition = pastPosition + direction;
            path.Add(newPosition);
            pastPosition = newPosition;
        }
        endPosition = pastPosition;
        return path;
    }
    #endregion

    #region CellularAutomaton

    private class Cell
    {
        public bool alive;
        public Vector2Int position;
        public List<Cell> neighbors;
        public int State => alive ? 1 : 0;

        public Cell(bool alive, Vector2Int position, List<Cell> neighbors)
        {
            this.alive = alive;
            this.position = position;
            this.neighbors = neighbors;
        }

        public Cell(bool alive, Vector2Int position)
        {
            this.alive = alive;
            this.position = position;
            neighbors = new List<Cell>();
        }

        public Cell() { }

        public void SetCellNeighbors(IEnumerable<Vector2Int> positions, IEnumerable<Cell> cells, IEnumerable<Vector2Int> directions)
        {
            var neighborsPositions = GetNeighbors(position, positions, directions);
            foreach(var neighborPos in neighborsPositions)
            {
                Cell neighborCell = cells.FirstOrDefault(cell => neighborPos == cell.position);
                if(neighborCell != null)
                    neighbors.Add(neighborCell);
            }
        }
    }

    
    public static HashSet<Vector2Int> CellularAutomatonGeneration(Vector2Int startPosition, Vector2Int size, float startPercent, int iterations)
    {
        var cells = new List<Cell>();
        var positions = new HashSet<Vector2Int>();

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                Vector2Int position = new Vector2Int(i, j) + startPosition;
                bool isAlive = Random.value < startPercent;
                cells.Add(new Cell(isAlive, position));
                positions.Add(position);
            }
        }

        foreach (var cell in cells)
        {
            cell.SetCellNeighbors(positions, cells, Direction.allDirections);
        }

        positions.Clear();

        for (int i = 0; i < iterations; i++)
        {
            var nextGeneration = new List<Cell>();

            foreach (var cell in cells)
            {
                int aliveNeighbors = cell.neighbors.Count(n => n.alive);

                if (cell.alive && (aliveNeighbors == 2 || aliveNeighbors == 3))
                {
                    nextGeneration.Add(new Cell(true, cell.position, cell.neighbors));
                }
                else if (!cell.alive && aliveNeighbors == 3)
                {
                    nextGeneration.Add(new Cell(true, cell.position, cell.neighbors));
                }
                else
                {
                    nextGeneration.Add(new Cell(false, cell.position, cell.neighbors));
                }
            }

            cells = nextGeneration;
        }

        foreach (var cell in cells)
        {
            if (cell.alive)
            {
                positions.Add(cell.position);
            }
        }


        return positions;
    }

    #endregion

    #region BSP
    public static HashSet<Vector2Int> GenerateFloorPositionsBSP(Vector2Int dungeonSize, int minRoomSize)
    {
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        List<RectInt> rooms = new List<RectInt>();

        Queue<RectInt> spaces = new Queue<RectInt>();
        spaces.Enqueue(new RectInt(Vector2Int.zero, dungeonSize));

        while (spaces.Count > 0)
        {
            RectInt space = spaces.Dequeue();

            if (space.width >= minRoomSize * 2 || space.height >= minRoomSize * 2)
            {
                bool splitHorizontally = space.width > space.height
                    ? false
                    : space.height > space.width
                        ? true
                        : Random.value > 0.5f;

                RectInt room1, room2;
                if (splitHorizontally)
                {
                    int splitY = Random.Range(minRoomSize, space.height - minRoomSize);
                    room1 = new RectInt(space.xMin, space.yMin, space.width, splitY);
                    room2 = new RectInt(space.xMin, space.yMin + splitY, space.width, space.height - splitY);
                }
                else
                {
                    int splitX = Random.Range(minRoomSize, space.width - minRoomSize);
                    room1 = new RectInt(space.xMin, space.yMin, splitX, space.height);
                    room2 = new RectInt(space.xMin + splitX, space.yMin, space.width - splitX, space.height);
                }

                spaces.Enqueue(room1);
                spaces.Enqueue(room2);
            }
            else
            {
                rooms.Add(space);
            }
        }

        foreach (RectInt room in rooms)
        {
            for (int x = room.xMin + 1; x < room.xMax - 1; x++) 
            {
                for (int y = room.yMin + 1; y < room.yMax - 1; y++)
                {
                    floorPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        return floorPositions;
    }

    public static HashSet<Vector2Int> DivideDungeonIntoRooms(HashSet<Vector2Int> floorPositions, int minRoomSize)
    {
        HashSet<Vector2Int> dividedFloorPositions = new HashSet<Vector2Int>();

        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (Vector2Int pos in floorPositions)
        {
            if (pos.x < minX) minX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y > maxY) maxY = pos.y;
        }

        Vector2Int startPosition = new Vector2Int(minX, minY);
        Vector2Int dungeonSize = new Vector2Int(maxX - minX + 1, maxY - minY + 1);

        Queue<RectInt> spaces = new Queue<RectInt>();
        spaces.Enqueue(new RectInt(startPosition, dungeonSize));

        while (spaces.Count > 0)
        {
            RectInt space = spaces.Dequeue();

            if (space.width >= minRoomSize * 2 || space.height >= minRoomSize * 2)
            {
                bool splitHorizontally = space.width > space.height
                    ? false
                    : space.height > space.width
                        ? true
                        : Random.value > 0.5f;

                RectInt room1, room2;
                if (splitHorizontally)
                {
                    int splitY = Random.Range(minRoomSize, space.height - minRoomSize);
                    room1 = new RectInt(space.xMin, space.yMin, space.width, splitY);
                    room2 = new RectInt(space.xMin, space.yMin + splitY, space.width, space.height - splitY);
                }
                else
                {
                    int splitX = Random.Range(minRoomSize, space.width - minRoomSize);
                    room1 = new RectInt(space.xMin, space.yMin, splitX, space.height);
                    room2 = new RectInt(space.xMin + splitX, space.yMin, space.width - splitX, space.height);
                }

                spaces.Enqueue(room1);
                spaces.Enqueue(room2);
            }
            else
            {
                foreach (Vector2Int position in floorPositions)
                {
                    if (position.x >= space.xMin && position.x < space.xMax &&
                        position.y >= space.yMin && position.y < space.yMax)
                    {
                        dividedFloorPositions.Add(position);
                    }
                }
            }
        }

        return dividedFloorPositions;
    }


    #endregion


    public static IEnumerable<Vector2Int> GetNeighbors(Vector2Int position, IEnumerable<Vector2Int> from, IEnumerable<Vector2Int> directions)
    {
        foreach (var direction in directions)
            if (position.TryGetNeighbor(direction, from, out var neighbor))
                yield return neighbor;
    }

    public static bool TryGetNeighbor(this Vector2Int position, Vector2Int inDirection, IEnumerable<Vector2Int> from, out Vector2Int neighbor)
    {
        neighbor = position + inDirection;
        return from.Contains(neighbor);
    }

    public static List<Vector2Int> GetMapEdge(HashSet<Vector2Int> map, IEnumerable<Vector2Int> directions, out List<Vector2Int> turnDirections)
    {
        var edge = new List<Vector2Int>();
        turnDirections = new List<Vector2Int>();
        foreach (var position in map)
        {
            foreach (var direction in directions)
            {
                if (!position.TryGetNeighbor(direction, map, out var neighbor))
                {
                    edge.Add(neighbor);
                    turnDirections?.Add(direction);
                }
            }
        }
        return edge;
    }


}
