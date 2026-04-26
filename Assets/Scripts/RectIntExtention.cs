using System.Collections.Generic;
using UnityEngine;

public static class RectIntExtention
{
    public static bool Contains(this RectInt rect, float x, float y)
    {
        return x >= rect.min.x && x <= rect.max.x && y >= rect.min.y && y <= rect.max.y;
    }

    public static RectInt Turn(this RectInt rect, Vector2Int dir)
    {
        int x = rect.xMin - (dir.x < 0 ? rect.width : 0);
        int y = rect.yMin - (dir.y < 0 ? rect.height : 0);
        return new RectInt(x, y, rect.width, rect.height);
    }

    public static RectInt Intersect(this RectInt a, RectInt b)
    {
        int x1 = Mathf.Max(a.xMin, b.xMin);
        int x2 = Mathf.Min(a.xMax, b.xMax);
        int y1 = Mathf.Max(a.yMin, b.yMin);
        int y2 = Mathf.Min(a.yMax, b.yMax);

        if (x2 >= x1 && y2 >= y1)
            return new RectInt(x1, y1, x2 - x1, y2 - y1);

        return new RectInt(0, 0, 0, 0);

    }

    public static bool Touches(this RectInt a, RectInt b)
    {
        return ((a.xMax == b.xMin || a.xMin == b.xMax) && (a.yMin < b.yMax && a.yMax > b.yMin)) ||
        ((a.yMax == b.yMin || a.yMin == b.yMax) && (a.xMin < b.xMax && a.xMax > b.xMin));
    }

    public static bool IsPointOnContactLine(this RectInt rect1, RectInt rect2, int x, int y)
    {
        if (rect1.xMax + 1 == rect2.xMin || rect2.xMax + 1 == rect1.xMin)
        {
            int contactX = Mathf.Min(rect1.xMax, rect2.xMax) + 1;

            if (x == contactX || x == contactX - 1)
            {
                int yMin = Mathf.Max(rect1.yMin, rect2.yMin);
                int yMax = Mathf.Min(rect1.yMax, rect2.yMax);

                return y >= yMin && y <= yMax;
            }
        }
        else if (rect1.yMax + 1 == rect2.yMin || rect2.yMax + 1 == rect1.yMin)
        {
            int contactY = Mathf.Min(rect1.yMax, rect2.yMax) + 1;

            if (y == contactY || y == contactY - 1)
            {
                int xMin = Mathf.Max(rect1.xMin, rect2.xMin);
                int xMax = Mathf.Min(rect1.xMax, rect2.xMax);

                return x >= xMin && x <= xMax;
            }
        }

        return false;
    }

    public static bool IsTouchPoint(this RectInt a, RectInt b, int x, int y)
    {
        bool horizontalTouch = (a.xMax == b.xMin || a.xMin == b.xMax) &&
                              (a.yMin < b.yMax && a.yMax > b.yMin);

        if (horizontalTouch)
        {
            int touchX = a.xMax == b.xMin ? a.xMax : a.xMin;
            return x == touchX && y > Mathf.Max(a.yMin, b.yMin) && y < Mathf.Min(a.yMax, b.yMax);
        }

        bool verticalTouch = (a.yMax == b.yMin || a.yMin == b.yMax) &&
                            (a.xMin < b.xMax && a.xMax > b.xMin);

        if (verticalTouch)
        {
            int touchY = a.yMax == b.yMin ? a.yMax : a.yMin;
            return y == touchY && x > Mathf.Max(a.xMin, b.xMin) && x < Mathf.Min(a.xMax, b.xMax);
        }

        return false;
    }

    public static List<Vector2Int> GetTouchingPoints(this RectInt a, RectInt b)
    {
        List<Vector2Int> contactPoints = new List<Vector2Int>();

        if ((a.xMax == b.xMin || a.xMin == b.xMax) && (a.yMin <= b.yMax && a.yMax >= b.yMin))
        {
            int contactX = a.xMax == b.xMin ? a.xMax : a.xMin;
            int startY = Mathf.Max(a.yMin, b.yMin);
            int endY = Mathf.Min(a.yMax, b.yMax);

            for (int y = startY; y <= endY; y++)
            {
                contactPoints.Add(new Vector2Int(contactX, y));
            }
        }

        if ((a.yMax == b.yMin || a.yMin == b.yMax) && (a.xMin <= b.xMax && a.xMax >= b.xMin))
        {
            int contactY = a.yMax == b.yMin ? a.yMax : a.yMin;
            int startX = Mathf.Max(a.xMin, b.xMin);
            int endX = Mathf.Min(a.xMax, b.xMax);

            for (int x = startX; x <= endX; x++)
            {
                contactPoints.Add(new Vector2Int(x, contactY));
            }
        }

        return contactPoints;
    }

    public static bool IsPointOnRectBoundary(this RectInt rect, int x, int y)
    {
        bool onXBoundary = x == rect.xMin || x == rect.xMax;
        bool onYBoundary = y == rect.yMin || y == rect.yMax;

        return onXBoundary || onYBoundary;
    }

    public static RectInt Union(this RectInt a, RectInt b)
    {
        int xMin = Mathf.Min(a.xMin, b.xMin);
        int yMin = Mathf.Min(a.yMin, b.yMin);
        int xMax = Mathf.Max(a.xMax, b.xMax);
        int yMax = Mathf.Max(a.yMax, b.yMax);

        return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    public static List<Vector2Int> GetSidePoints(this RectInt a, Vector2Int dir)
    {
        var points = new List<Vector2Int>();

        if ((dir.x == 0 && dir.y == 0) || (dir.x != 0 && dir.y != 0))
            throw new System.Exception("Wrong side direction!");

        if (dir.x != 0) 
        {
            int x = dir.x > 0 ? a.xMax : a.xMin;
            for (int y = a.yMin; y <= a.yMax; y++)
            {
                points.Add(new Vector2Int(x, y));
            }
        }
        else if (dir.y != 0) 
        {
            int y = dir.y > 0 ? a.yMax : a.yMin;
            for (int x = a.xMin; x <= a.xMax; x++)
            {
                points.Add(new Vector2Int(x, y));
            }
        }

        return points;
    }

}