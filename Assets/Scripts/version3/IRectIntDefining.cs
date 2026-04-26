using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Version3
{
    public interface IRectIntDefiner
    {
        public RectInt Execute(Vector2Int size, SizeSelectionBoundaryData boundaryData);
    }

    public class RectIntDefiner : IRectIntDefiner
    {
        public RectInt Execute(Vector2Int size, SizeSelectionBoundaryData boundaryData)
        {
            var point = GetGaussianElement(boundaryData.AllowedSidePoints);
            var dp = (point - boundaryData.StartPoint);

            Vector2Int position = point;
            Vector2Int allowed = boundaryData.Tangent + boundaryData.Normal;

            if (allowed == new Vector2Int(1, -1))
                position.y -= size.y;
            else if (allowed == new Vector2Int(-1, -1))
                position -= size;
            else if (allowed == new Vector2Int(-1, 1))
                position.x -= size.x;

            var cv = boundaryData.Tangent * (dp + size);
            if (boundaryData.TangentLength * boundaryData.TangentLength < cv.sqrMagnitude)
            {
                var ds = (cv - boundaryData.TangentLength * boundaryData.Tangent);
                size -= ds;
            }

            return new RectInt(position, size);
        }

        private Vector2Int GetGaussianElement(List<Vector2Int> list)
        {
            if (list.Count == 1) return list[0];

            float u1 = 1 - Random.value;
            float u2 = 1 - Random.value;
            float stdNormal = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Sin(2f * Mathf.PI * u2);

            float mean = (list.Count - 1) / 2f;
            float stdDev = list.Count / 6f;
            float gaussIndex = mean + stdDev * stdNormal;

            int index = Mathf.RoundToInt(gaussIndex);
            index = Mathf.Max(0, Mathf.Min(list.Count - 1, index));

            return list[index];
        }
    }
}
