using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

namespace Version3
{
    public interface ISidePicker
    {
        public List<Vector2Int> Execute(Region region, out Vector2Int direction, List<Vector2Int> except);
    }

    public class RandomSidePicker : ISidePicker
    {
        private float[] sideWeights = new float[]
        {
            0.25f,
            0.25f,
            0.25f,
            0.25f
        };

        private HashSet<Vector2Int> localExcept = new HashSet<Vector2Int>(); // ?

        public List<Vector2Int> Execute(Region region, out Vector2Int direction, List<Vector2Int> except)
        {
            float totalWeight = 0;
            for (int i = 0; i < Direction.cardinalDirections.Length; i++)
            {
                if (except != null && except.Contains(Direction.cardinalDirections[i])
                    || localExcept.Contains(Direction.cardinalDirections[i]))
                    continue;
                totalWeight += sideWeights[i];
            }

            float randomWeight = Random.value * totalWeight;
            float currentWeight = 0;
            direction = new Vector2Int();

            for (int i = 0; i < Direction.cardinalDirections.Length; i++)
            {
                if (except != null && except.Contains(Direction.cardinalDirections[i])
                    || localExcept.Contains(Direction.cardinalDirections[i]))
                    continue;
                currentWeight += sideWeights[i];
                if (randomWeight <= currentWeight)
                {
                    direction = Direction.cardinalDirections[i];
                    //float d = sideWeights[i] * 0.5f;
                    //for (int j = 0; j < Direction.cardinalDirections.Length; j++)
                    //{
                    //    if (Direction.cardinalDirections[j] == -direction)
                    //    {
                    //        sideWeights[j] -= d;
                    //        sideWeights[j] = Mathf.Max(sideWeights[j], 0);
                    //    }
                    //    else
                    //    {
                    //        sideWeights[j] += d / 3;
                    //        sideWeights[j] = Mathf.Max(sideWeights[j], 0.33333f);
                    //    }
                    //}
                    break;
                }
            }
                
            return region.Rect.GetSidePoints(direction);
        }

    }

    public class SpiralSidePicker : ISidePicker
    {
        private int i = 0;
        private int counter = 0;
        private readonly int resetIndicator = 2;

        public List<Vector2Int> Execute(Region region, out Vector2Int direction, List<Vector2Int> except)
        {
            counter++;
            if (counter == resetIndicator)
            {
                i++;
                counter = 0;
            }
            direction = Direction.cardinalDirections[i % Direction.cardinalDirections.Length];
            return region.Rect.GetSidePoints(direction);
        }
    }
}
