using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DungeonGenerator : MonoBehaviour
{
    [SerializeField] protected DungeonVisualizer visualizer;
    [Space]
    [SerializeField] protected Vector2Int startPosition;

    public abstract void Generate();
}
