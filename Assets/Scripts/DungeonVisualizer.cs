using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

public class DungeonVisualizer : MonoBehaviour
{
    [SerializeField] private GameObject floor;
    [SerializeField] private Transform floorParent;
    [Space]
    [SerializeField] private GameObject wall;
    [SerializeField] private Transform wallParent;
    [Space]
    [SerializeField] private GameObject door;
    [SerializeField] private Transform doorParent;
    [Space]
    [SerializeField] private float positionMultiplier = 1;
    [SerializeField] private float wallPositionMultiplier = 1;

    [SerializeField] private ObjectPool<GameObject> floorPool;
    [SerializeField] private ObjectPool<GameObject> wallPool;
    [SerializeField] private ObjectPool<GameObject> doorPool;

    private void Awake()
    {
        if (floorPool == null && wallPool == null && doorPool == null)
            InitializePools();
    }

    private void OnValidate()
    {
        if (floorPool == null && wallPool == null)
            InitializePools();
    }

    [ContextMenu("Initialize Pools")]
    public void InitializePools()
    {
        floorPool = new ObjectPool<GameObject>(
                () =>
                {
                    var floor = Instantiate(this.floor, floorParent);
                    floor.SetActive(false);
                    return floor;
                },
                (GameObject floor) =>
                {
                    floor.SetActive(true);
                },
                (GameObject floor) =>
                {
                    floor.SetActive(false);
                },
                (GameObject floor) =>
                {
                    Destroy(floor);
                },
                collectionCheck: true,
                defaultCapacity: 10,
                maxSize: 10000
                );

        wallPool = new ObjectPool<GameObject>(
                () =>
                {
                    var wall = Instantiate(this.wall, wallParent);
                    wall.SetActive(false);
                    return wall;
                },
                (GameObject wall) =>
                {
                    wall.SetActive(true);
                },
                (GameObject wall) =>
                {
                    wall.SetActive(false);
                },
                (GameObject wall) =>
                {
                    Destroy(wall);
                },
                collectionCheck: true,
                defaultCapacity: 10,
                maxSize: 10000
                );

        doorPool = new ObjectPool<GameObject>(
                () =>
                {
                    var door = Instantiate(this.door, doorParent);
                    door.SetActive(false);
                    return door;
                },
                (GameObject door) =>
                {
                    door.SetActive(true);
                },
                (GameObject door) =>
                {
                    door.SetActive(false);
                },
                (GameObject door) =>
                {
                    Destroy(door);
                },
                collectionCheck: true,
                defaultCapacity: 10,
                maxSize: 10000
                );

    }

    public void Creation(int[,,] grid)
    {
        Vector3 spawn = Vector3.zero;

        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                spawn = new Vector3(x, 0, y) * positionMultiplier;

                if (grid[x, y, (int)LayerLevel.Floor] != 0)
                {
                    floorPool.Get().transform.position = spawn;
                }

                if (grid[x, y, (int)LayerLevel.Wall] != 0 && grid[x, y, (int)LayerLevel.Door] != 0)
                {
                    Vector2 tan = Vector2.zero;
                    foreach (var dir in Direction.cardinalDirections)
                    {
                        if (grid[x + dir.x, y + dir.y, (int)LayerLevel.Wall] == 0)
                            tan = dir;
                    }
                    Vector3 n = new(tan.x, 0, tan.y);
                    doorPool.Get().transform.SetPositionAndRotation(spawn, Quaternion.LookRotation(n));
                }

                if (grid[x, y, (int)LayerLevel.Wall] != 0 && grid[x, y, (int)LayerLevel.Door] == 0)
                {
                    wallPool.Get().transform.position = new Vector3(x, 0, y) * wallPositionMultiplier;
                    
                    //foreach (var dir in Direction.cardinalDirections)
                    //{
                    //    int dx = x + dir.x;
                    //    int dy = y + dir.y;
                    //    if (dx >= 0 && dy >= 0 && dx < grid.GetLength(0) && dy < grid.GetLength(1))
                    //    {
                    //        if (grid[dx, dy, 0] == 0)
                    //        {
                    //            wallPool.Get().transform.SetPositionAndRotation(spawn, Quaternion.LookRotation(-new Vector3(dir.x, 0, dir.y)));
                    //        }
                            
                    //    }
                    //    else
                    //    {
                    //        wallPool.Get().transform.SetPositionAndRotation(spawn, Quaternion.LookRotation(-new Vector3(dir.x, 0, dir.y)));
                    //    }
                    //}


                    //int nCount = 0;
                    //Vector2 res = new Vector2();
                    //Vector2 empres = new Vector2();
                    //foreach (var dir in Direction.allDirections)
                    //{
                    //    int dx = x + dir.x;
                    //    int dy = y + dir.y;
                    //    if (dx >= 0 && dy >= 0 && dx < grid.GetLength(0) && dy < grid.GetLength(1))
                    //    {
                    //        if (grid[dx, dy, 2] != 0 || grid[dx, dy, 1] != 0)
                    //        {
                    //            nCount++;
                    //            res += dir;
                    //        }
                    //        if (grid[dx, dy, 2] == 0 && grid[dx, dy, 1] == 0)
                    //        {
                    //            empres += dir;
                    //        }
                    //    }
                    //}

                    //if(nCount > 3 && res.sqrMagnitude != 0)
                    //{
                    //    empres.Normalize();
                    //    wallPool.Get().transform.SetPositionAndRotation(spawn, Quaternion.LookRotation(new Vector3Int(Mathf.RoundToInt(empres.x), 0, Mathf.RoundToInt(empres.y))));
                    //}

                }
            }
        }
    }

    public void Clear()
    {
        foreach (Transform child in floorParent)
        {
            if (child.gameObject.activeSelf)
                floorPool.Release(child.gameObject);
        }
        foreach (Transform child in wallParent)
        {
            if (child.gameObject.activeSelf)
                wallPool.Release(child.gameObject);
        }
        foreach (Transform child in doorParent)
        {
            if (child.gameObject.activeSelf)
                doorPool.Release(child.gameObject);
        }
    }

    [ContextMenu("DestroyClear")]
    private void DestroyClear()
    {
        foreach (Transform child in floorParent)
        {
            DestroyImmediate(child.gameObject);
        }
        foreach (Transform child in wallParent)
        {
            DestroyImmediate(child.gameObject);
        }
        floorPool = null;
        wallPool = null;
        doorPool = null;
    }
}
