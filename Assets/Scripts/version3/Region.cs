using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

namespace Version3
{
    public class Region
    {
        public RectInt Rect;
        public List<Passage> passages;
        public Passage MainPassage => passages.Count == 0 ? null : passages[0];
        public SizeSelectionBoundaryData boundaryData;

        public Region(RectInt rect, SizeSelectionBoundaryData boundaryData)
        {
            Rect = rect;
            passages = new List<Passage>();
            this.boundaryData = boundaryData;
        }

        public void Shift(Vector2Int offset)
        {
            Rect.position += offset;
            if(passages.Count > 0)
            {
                foreach(var passage in passages)
                {
                    for (int i = 0; i < passage.TouchPoints.Count; i++)
                    {
                        passage.TouchPoints[i] = passage.TouchPoints[i] + offset;
                    }
                }
            }
            if (boundaryData.Allowed)
            {
                for(int i = 0; i < boundaryData.AllowedSidePoints.Count; i++)
                {
                    boundaryData.AllowedSidePoints[i] = boundaryData.AllowedSidePoints[i] + offset;
                }
                boundaryData.StartPoint += offset;
            }
        }

    }

    public class Passage
    {
        public Region From;
        public Region To;
        public List<Vector2Int> TouchPoints;
        public Vector2Int Direction;

        public Passage(Region from, Region to, List<Vector2Int> list, Vector2Int direction)
        {
            From = from;
            To = to;
            TouchPoints = list;
            Direction = direction;
        }
    }

    public struct SizeSelectionBoundaryData
    {
        public Vector2Int Normal;
        public Vector2Int Tangent;
        public List<Vector2Int> AllowedSidePoints;
        public Vector2Int StartPoint;

        public int NormalLength;
        public int TangentLength;

        public bool Allowed;
    }

    public class DungeonCreator
    {
        private int roomCount = 10;
        private Queue<Region> processing;

        public List<Region> dungeon;
        public List<SizeSelectionBoundaryData> debugExceptBoundaryData;

        private ISidePicker sidePicker;
        private IBoundaryDataSelector boundaryDataSelector;
        private ISizeSelector sizeSelector;
        private IRectIntDefiner rectIntDefiner;

        public DungeonCreator(int roomCount, ISidePicker sidePicker, IBoundaryDataSelector boundaryDataSelector,
            ISizeSelector sizeSelector, IRectIntDefiner rectIntDefiner)
        {
            this.roomCount = roomCount;
            dungeon = new List<Region>();
            processing = new Queue<Region>();
            debugExceptBoundaryData = new List<SizeSelectionBoundaryData>();

            this.sidePicker = sidePicker;
            this.boundaryDataSelector = boundaryDataSelector;
            this.sizeSelector = sizeSelector;
            this.rectIntDefiner = rectIntDefiner;
        }

        public void Create()
        {
            Region first = new Region(new RectInt(0, 0, 10, 10), new SizeSelectionBoundaryData());
            processing.Enqueue(first);
            dungeon.Add(first);
            Main();
            //CreateCorridors();
        }

        private void Main()
        {
            for (int i = 0; dungeon.Count < roomCount; i++)
            {
                Region current = processing.Dequeue();
                Region add = RegionCalculation(current);

                if (add == null)
                {
                    Debug.LogError("Генерация прервана. Рассчитанный регион равен null");
                    break;
                }
                processing.Enqueue(add);
                dungeon.Add(add);
            }
        }

        private Region RegionCalculation(Region current)
        {
            List<Vector2Int> sidePoints = sidePicker.Execute(current, out Vector2Int direction, null);
            var boundaryData = boundaryDataSelector.Execute(dungeon, current.MainPassage, sidePoints, direction);

            try
            {
                if (boundaryData.Allowed == false)
                {
                    List<Vector2Int> except = new List<Vector2Int>() { direction };
                    debugExceptBoundaryData.Add(boundaryData);
                    for (int d = 0; d < 4; d++)
                    {
                        sidePoints = sidePicker.Execute(current, out direction, except);
                        boundaryData = boundaryDataSelector.Execute(dungeon, current.MainPassage, sidePoints, direction);
                        if (boundaryData.Allowed == false)
                        {
                            except.Add(direction);
                            debugExceptBoundaryData.Add(boundaryData);
                        }
                        else break;
                    }
                }
            }
            catch
            {
                Debug.LogWarning($"Не найдена подходящая сторона у {dungeon.Count} комнаты. Производится откат");
                debugExceptBoundaryData.Clear();
                return Rollback(5);
            }

            debugExceptBoundaryData.Clear();
            Vector2Int size = sizeSelector.Execute(current, boundaryData);
            RectInt rect = rectIntDefiner.Execute(size, boundaryData);

            Region region = new Region(rect, boundaryData);
            Passage passage = new Passage(current, region, rect.GetTouchingPoints(current.Rect), direction);
            region.passages.Add(passage);
            return region;
        }

        private Region Rollback(int count)
        {
            dungeon.RemoveRange(dungeon.Count - 1 - count, count + 1);
            return dungeon[^1];
        }

        private void Branching(Region forkRegion, int length)
        {
            processing.Enqueue(forkRegion);
            Main();
        }

        private void CreateCorridors()
        {
            for (int i = 0; i < dungeon.Count; i++)
            {
                if (Random.value > 0.5f) continue;

                Region current = dungeon[i];
                if(current.MainPassage == null) continue;

                Passage passage = current.MainPassage;
                Vector2Int position = passage.TouchPoints[0];
                int lenght = Random.Range(2, 5);
                Vector2Int size = passage.Direction * lenght + (passage.TouchPoints[1] - passage.TouchPoints[0]);

                Region corridor = new Region(new RectInt(position, size), new SizeSelectionBoundaryData());
                
                dungeon.Insert(i, corridor);
                i++;

                for(int j = i; j < dungeon.Count; j++)
                {
                    dungeon[j].Shift(passage.Direction * lenght);
                }
            }
        }


        #region Debug
        public void DrawDebug(int index, bool allBoundaryData)
        {
            for (int i = 0; i < dungeon.Count; i++)
            {
                Gizmos.color = Color.black;
                DrawRegion(dungeon[i]);
                if (allBoundaryData && i != 0)
                    DrawBoundaryData(dungeon[i].boundaryData);
            }

            if (index < dungeon.Count)
            {
                Gizmos.color = Color.green;
                DrawRegion(dungeon[index]);
                if (!allBoundaryData && index != 0)
                    DrawBoundaryData(dungeon[index].boundaryData);
            }

        }

        private void DrawRegion(Region region)
        {
            var center = new Vector3(region.Rect.center.x, 0, region.Rect.center.y);
            var size = new Vector3(region.Rect.width, 1, region.Rect.height);
            Gizmos.DrawWireCube(center, size);

            if(region.MainPassage != null)
                DrawPassage(region.MainPassage);
        }

        private void DrawPassage(Passage passage)
        {
            if(passage.TouchPoints.Count < 2) return;
            Gizmos.color = Color.blue;
            var p1 = new Vector3(passage.TouchPoints[0].x, 0.6f, passage.TouchPoints[0].y);
            var p2 = new Vector3(passage.TouchPoints[^1].x, 0.6f, passage.TouchPoints[^1].y);
            Gizmos.DrawLine(p1, p2);
        }

        public void DrawExceptBoundaryData(int index) //Попробовать бинарный поиск
        {
            if(index < debugExceptBoundaryData.Count)
                DrawBoundaryData(debugExceptBoundaryData[index]);
        }

        private void DrawBoundaryData(SizeSelectionBoundaryData data)
        {
            if (!data.Allowed && data.AllowedSidePoints == null) return;

            var start = new Vector3(data.StartPoint.x, 1.2f, data.StartPoint.y);
            var tangent = new Vector3(data.Tangent.x, 0, data.Tangent.y);
            var normal = new Vector3(data.Normal.x, 0, data.Normal.y);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(start, tangent);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(start, normal);

            var allowedPoint1 = new Vector3(data.AllowedSidePoints[0].x, 1.1f, data.AllowedSidePoints[0].y);
            var allowedPoint2 = new Vector3(data.AllowedSidePoints[^1].x, 1.1f, data.AllowedSidePoints[^1].y);

            Gizmos.DrawLine(allowedPoint1, allowedPoint2);

            var start2 = start - new Vector3(0, 0.6f, 0);

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(start2, tangent * data.TangentLength);
            Gizmos.DrawRay(start2, normal * data.NormalLength);
        }

        #endregion
    }
}
