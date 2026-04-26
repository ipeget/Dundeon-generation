using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NewGeneration
{
    public class Room
    {
        public readonly Node[] nodes;
        public RectInt totalRect;
        private List<Vector2Int> edge;
        public List<Vector2Int> doors;

        public Room(RectInt totalRect, params Node[] nodes)
        {
            this.totalRect = totalRect;
            this.nodes = nodes;
            edge = new List<Vector2Int>();
            doors = new List<Vector2Int>();
            FindEdge();
        }

        public bool Contains(int x, int y)
        {
            bool found = false;
            foreach (Node node in nodes)
            {
                if (!found)
                    found = node.rect.Contains(x, y);
            }
            return found;
        }

        private RectInt[] GetAllRect()
        {
            RectInt[] rect = new RectInt[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                rect[i] = nodes[i].rect;
            }
            return rect;
        }

        public void FindEdge()
        {
            List<Vector2Int> edge = new List<Vector2Int>();
            foreach (Node node in nodes)
            {
                for (int x = node.rect.xMin; x <= node.rect.xMax; x++)
                {
                    for (int y = node.rect.yMin; y <= node.rect.yMax; y++)
                    {
                        if (node.rect.IsPointOnRectBoundary(x, y))
                            edge.Add(new Vector2Int(x, y));
                    }
                }
            }

            //var result = edge.GroupBy(v => v).Where(g => g.Count() == 1).Select(g => g.Key).ToList();
            this.edge = edge;
        }

        public bool OnEdge(int x, int y)
        {
            if (nodes.Length == 2)
            {
                var r1 = nodes[0].rect;
                var r2 = nodes[1].rect;
                return (!r1.IsTouchPoint(r2, x, y) && !r2.IsTouchPoint(r1, x, y)) && edge.Contains(new(x, y));
            }
            else
                return edge.Contains(new(x, y));
        }

        public void Debug()
        {
            foreach (Node node in nodes)
            {
                var pos = new Vector3(node.rect.center.x, 0, node.rect.center.y) * 4;
                var size = new Vector3(node.rect.width, 1, node.rect.height) * 4;
                Gizmos.DrawCube(pos, size);
            }
        }
    }
    public class Node
    {
        public Node parent;
        public List<Node> children;
        public List<Node> neigbours;

        public RectInt rect;
        public int Area => rect.width * rect.height;

        public Node(Node parent, RectInt rect)
        {
            this.parent = parent;
            this.rect = rect;
            children = new List<Node>();
            neigbours = new List<Node>();
        }

        public void AddChild(Node child)
        {
            children.Add(child);
        }
        public void AddNeigbour(Node neigbour)
        {
            neigbours.Add(neigbour);
        }

        public Node[] GetChildrenWithout(Node node)
        {
            Node[] list = new Node[children.Count - 1];
            int i = 0;
            foreach (Node child in children)
            {
                if (child == node)
                    continue;
                list[i++] = child;
            }
            return list;
        }

        public bool IsFinite => children.Count == 0;
        public bool IsRoot => parent == null;
    }

    public class DungeonCreator
    {
        private int maxDepth;
        private Vector2Int minSize;
        private RectInt rect;
        private int usefulArea = 1250;

        private Node root;
        private List<Node> finites;

        public DungeonCreator(int maxDepth, Vector2Int minSize, RectInt rect)
        {
            this.maxDepth = maxDepth;
            this.minSize = minSize;
            this.rect = rect;
            CreateTree();
        }

        private void BSPSplit(Node node, int depth)
        {
            if (depth >= maxDepth || (node.rect.width <= minSize.x * 2 && node.rect.height <= minSize.y * 2))
            {
                finites.Add(node);
                return;
            }

            bool splitNorizontal = node.rect.width > node.rect.height ? false : node.rect.height > node.rect.width ? true : Random.value > 0.9f;

            if (splitNorizontal)
            {
                int y = Mathf.RoundToInt(node.rect.height * (0.3f + Random.Range(0, 1f) * 0.4f));
                Node top = new Node(node, new RectInt(node.rect.x, node.rect.y, node.rect.width, y));
                Node buttom = new Node(node, new RectInt(node.rect.x, node.rect.y + y, node.rect.width, node.rect.height - y));
                node.AddChild(top);
                node.AddChild(buttom);
                BSPSplit(top, depth + 1);
                BSPSplit(buttom, depth + 1);
            }
            else
            {
                int x = Mathf.RoundToInt(node.rect.width * (0.3f + Random.Range(0, 1f) * 0.4f));
                Node left = new Node(node, new RectInt(node.rect.x, node.rect.y, x, node.rect.height));
                Node right = new Node(node, new RectInt(node.rect.x + x, node.rect.y, node.rect.width - x, node.rect.height));
                node.AddChild(left);
                node.AddChild(right);
                BSPSplit(left, depth + 1);
                BSPSplit(right, depth + 1);
            }
        }

        private void CreateTree()
        {
            finites = new List<Node>();
            root = new Node(null, rect);
            BSPSplit(root, 0);
        }

        public List<Room> CreateRooms(int roomsCount)
        {
            List<Room> rooms = new List<Room>();
            List<Node> processed = new List<Node>();

            int maxArea = 0;
            foreach (Node node in finites)
            {
                if (node.Area > maxArea)
                    maxArea = node.Area;

                foreach (Node neigbour in finites)
                {
                    if (neigbour == node) continue;
                    if (node.rect.Touches(neigbour.rect) && !node.neigbours.Contains(neigbour) && !neigbour.neigbours.Contains(node))
                    {
                        if (node.rect.GetTouchingPoints(neigbour.rect).Count > 2)
                            node.AddNeigbour(neigbour);
                    }
                }
            }

            var result = new List<Node>();
            Queue<Node> queue = new Queue<Node>();
            queue.Enqueue(finites[0]);
            float weight = 0.5f;
            List<Node> addrRooms = new List<Node>();
            var area = 0;

            for (; result.Count < roomsCount && queue.Count > 0;)
            {
                Node node = queue.Dequeue();
                result.Add(node);
                if (node.neigbours.Count > 0)
                {
                    int index = 0;
                    for (int i = 0; i < node.neigbours.Count; i++)
                    {
                        Node neigbour = node.neigbours[i];
                        float w = neigbour.Area / maxArea;
                        if (w > weight)
                        {
                            weight = w;
                            index = i;
                        }
                    }
                    queue.Enqueue(node.neigbours[index]);
                    if (Random.value < 0.9f && node.neigbours.Count > 1)
                    {
                        int i = index;
                        while (i == index)
                            i = Random.Range(0, node.neigbours.Count);
                        if (!result.Contains(node.neigbours[i]))
                        {
                            addrRooms.Add(node.neigbours[i]);
                        }
                    }
                }
            }
            int aCount = addrRooms.Count < Mathf.FloorToInt(roomsCount * 0.4f) ? addrRooms.Count : Mathf.FloorToInt(roomsCount * 0.4f);
            for (int a = 0; a < aCount; a++)
            {
                int i = Random.Range(0, addrRooms.Count);
                result.Add(addrRooms[i]);
                addrRooms.Remove(addrRooms[i]);
            }

            foreach (var node in result)
            {
                if (processed.Contains(node))
                    continue;

                area += node.rect.height * node.rect.width;

                if (Random.value > 0.1f)
                {
                    Node neighbour = null;
                    foreach (var group in node.parent.parent.GetChildrenWithout(node.parent))
                    {
                        if (neighbour != null)
                            break;
                        foreach (var child in group.children)
                        {
                            if (child.rect.GetTouchingPoints(node.rect).Count > 3 && !processed.Contains(child) && child.IsFinite)
                            {
                                neighbour = child;
                                break;
                            }
                        }
                    }

                    if (neighbour != null)
                    {
                        area += neighbour.rect.height * neighbour.rect.width;
                        rooms.Add(new Room(node.rect.Union(neighbour.rect), node, neighbour));
                        processed.Add(neighbour);
                    }
                    else
                    {
                        rooms.Add(new Room(node.rect, node));
                    }
                }
                else
                {
                    rooms.Add(new Room(node.rect, node));
                }
                processed.Add(node);
            }

            foreach (var room in rooms)
            {
                foreach (var node in room.nodes)
                {
                    foreach (var neigbour in node.neigbours)
                    {
                        if (room.nodes.Contains(neigbour)) continue;
                        if (!result.Contains(neigbour)) continue;

                        var touchesPoints = node.rect.GetTouchingPoints(neigbour.rect);
                        room.doors.Add(touchesPoints[(touchesPoints.Count - 1) / 2]);
                    }
                }
            }


            return rooms;
        }

        public int[,,] Convert(List<Room> rooms)
        {
            int[,,] grid = new int[root.rect.width + 1, root.rect.height + 1, 3];

            foreach (Room room in rooms)
            {
                for (int y = room.totalRect.yMin; y <= room.totalRect.yMax; y++)
                {
                    for (int x = room.totalRect.xMin; x <= room.totalRect.xMax; x++)
                    {
                        if (room.Contains(x, y))
                            grid[x, y, 0] = 1;
                        if (room.OnEdge(x, y))
                            grid[x, y, 2] = 1;
                        if (room.doors.Contains(new(x, y)))
                            grid[x, y, 1] = 1;
                    }
                }
            }

            return grid;
        }
    }
}
