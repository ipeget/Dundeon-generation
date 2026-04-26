using UnityEngine;

namespace Version3
{
    public interface ISizeSelector
    {
        public Vector2Int Execute(Region region, SizeSelectionBoundaryData boundaryData);
    }

    public class FluctuationSizeSelector : ISizeSelector
    {
        private Vector2Int current = new Vector2Int(10, 10);
        private Vector2Int min = new Vector2Int(6, 6);
        private Vector2Int max = new Vector2Int(12, 12);
        private Vector2Int ds = new Vector2Int(3, 3);

        public Vector2Int Execute(Region current, SizeSelectionBoundaryData boundaryData)
        {
            Vector2Int cSize = this.current; // 
            int dym = boundaryData.Tangent.y != 0 ? boundaryData.TangentLength : boundaryData.NormalLength;
            int dxm = boundaryData.Tangent.x != 0 ? boundaryData.TangentLength : boundaryData.NormalLength;

            int rx = Random.Range(cSize.x - ds.x, cSize.x + ds.x + 1);
            rx = Mathf.Max(rx, min.x);
            rx = Mathf.Min(rx, dxm);

            int ry = Random.Range(cSize.y - ds.y, cSize.y + ds.y + 1);
            ry = Mathf.Max(ry, min.y);
            ry = Mathf.Min(ry, dym);

            return new Vector2Int(rx, ry);
        }
    }
}
