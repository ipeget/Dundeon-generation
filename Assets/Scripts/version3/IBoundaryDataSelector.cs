using UnityEngine;
using System.Collections.Generic;

namespace Version3
{
    public interface IBoundaryDataSelector
    {
        public SizeSelectionBoundaryData Execute(IReadOnlyList<Region> regions, Passage passage, List<Vector2Int> points, Vector2Int direction);
    }

    public class BoundaryDataSelector : IBoundaryDataSelector
    {
        private int maxLenght = 25;
        private int minLenght = 6;
        private Region except;

        public SizeSelectionBoundaryData Execute(IReadOnlyList<Region> regions, Passage passage, List<Vector2Int> sidePoints, Vector2Int sideDirection)
        {
            int nlength;
            int tlength;

            if (passage == null)
            {
                return new SizeSelectionBoundaryData()
                {
                    AllowedSidePoints = sidePoints,
                    Normal = sideDirection,
                    NormalLength = maxLenght,
                    Tangent = GetAllowedTangent(regions, sidePoints[sidePoints.Count / 2], sideDirection, out tlength),
                    TangentLength = tlength,
                    StartPoint = sidePoints[sidePoints.Count / 2],
                    Allowed = true
                };
            }


            except = passage.To;
            List<Vector2Int> allowedPoints = new();
            for (int i = 1; i < sidePoints.Count - 1; i++)
            {
                Vector2Int r = sidePoints[i + 1];
                Vector2Int l = sidePoints[i - 1];
                Vector2Int d = sidePoints[i];

                bool cornR = passage.TouchPoints.Contains(r);
                bool cornL = passage.TouchPoints.Contains(l);

                if (cornR && cornL)
                    continue;

                allowedPoints.Add(d);
            }

            int verNlength = CorrectDirectionLenght(regions, allowedPoints, sideDirection, out var point);
            Vector2Int tangent = GetAllowedTangent(regions, point, sideDirection, out _);

            tlength = CorrectDirectionLenght(regions, point, tangent, sideDirection, verNlength);
            nlength = CorrectDirectionLenght(regions, point, sideDirection, tangent, tlength);

            return new SizeSelectionBoundaryData()
            {
                AllowedSidePoints = allowedPoints, // подумать над определением допустимых точек
                Normal = sideDirection,
                NormalLength = nlength,
                Tangent = tangent,
                TangentLength = tlength,
                StartPoint = point,
                Allowed = tlength >= minLenght && nlength >= minLenght
            };
        }

        private int CorrectDirectionLenght(IReadOnlyList<Region> regions, List<Vector2Int> points, Vector2Int direction, out Vector2Int point)
        {
            point = default;
            if (points.Count < 3) return 0;

            int left = 1;
            int right = points.Count - 2;
            int rl = GetLengthAllowedDirection(regions, points[right], direction);
            int ll = GetLengthAllowedDirection(regions, points[left], direction);

            int mid;
            while (left <= right)
            {
                mid = left + (right - left) / 2;
                if (rl > ll)
                {
                    right = mid - 1;
                    rl = GetLengthAllowedDirection(regions, points[right], direction);
                }
                else if (rl < ll)
                {
                    left = mid + 1;
                    ll = GetLengthAllowedDirection(regions, points[left], direction);
                }
                else if (rl == ll)
                    break;
            }

            point = points[left];
            return GetLengthAllowedDirection(regions, point, direction);

            //int optimal = maxLenght;
            //int index = 0;
            //for (int i = 0; i < points.Count; i++)
            //{
            //    int l = GetLengthAllowedDirection(regions, points[i], direction);
            //    if(l < optimal)
            //    {
            //        optimal = l;
            //        index = i;
            //    }
            //}
            //point = points[index];
            //return optimal;
        }

        private int CorrectDirectionLenght(IReadOnlyList<Region> regions, Vector2Int startPoint, Vector2Int direction, Vector2Int orthogonal, int checkLength)
        {
            List<Vector2Int> list = new List<Vector2Int>();
            for(int i = 0; i < checkLength; i++)
                list.Add(startPoint + i * orthogonal);

            return CorrectDirectionLenght(regions, list, direction, out var point);
        }

        private Vector2Int GetTangent(Vector2Int normal)
        {
            Vector2 fdirection = Quaternion.Euler(0, 0, 90) * new Vector3(normal.x, normal.y);
            Vector2Int tangent = new Vector2Int(Mathf.RoundToInt(fdirection.x), Mathf.RoundToInt(fdirection.y));
            return tangent;
        }

        private Vector2Int GetAllowedTangent(IReadOnlyList<Region> regions, Vector2Int point, Vector2Int normal, out int length)
        {
            Vector2Int tangent = GetTangent(normal);
            int l1 = GetLengthAllowedDirection(regions, point, tangent);
            int l2 = GetLengthAllowedDirection(regions, point, -tangent);
            length = Mathf.Max(l1, l2);
            int sign = l1 > l2 ? 1 : -1;

            if (length > minLenght)
                tangent *= sign;

            return tangent;
        }


        private int GetLengthAllowedDirection(IReadOnlyList<Region> regions, Vector2Int startPoint, Vector2 direction)
        {
            bool found = false;
            int i = 1;
            for (; i < maxLenght; i++)
            {
                if (found) break;

                for (int j = 0; j < regions.Count - 1; j++)
                {
                    if (regions[j] == except) continue;

                    var point = startPoint + i * direction;
                    if (regions[j].Rect.Contains(point.x, point.y))
                    {
                        found = true;
                        break;
                    }
                }
            }
            return found ? i - 1 : maxLenght;
        }
    }
}
