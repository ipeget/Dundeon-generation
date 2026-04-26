using UnityEngine;
using System.Collections.Generic;
using Version3;

public class SimpleDungeonGenerator : DungeonGenerator
{
    [Space]
    [SerializeField] private DungeonData dungeonData;

    private Version3.DungeonCreator dungeonCreator;
    List<NewGeneration.Room> rooms = new List<NewGeneration.Room>();

    private void Start()
    {
        //dungeonCreator = new DungeonCreator(dungeonData.MaxDepth, dungeonData.RoomSize, new RectInt(Vector2Int.zero, dungeonData.DungeonSize));
    }

    public override void Generate()
    {
        //dungeonCreator = new DungeonCreator(dungeonData.MaxDepth, dungeonData.RoomSize, new RectInt(Vector2Int.zero, dungeonData.DungeonSize));
        //rooms = dungeonCreator.CreateRooms(dungeonData.RoomCount);
        //visualizer.Clear();
        //visualizer.Creation(dungeonCreator.Convert(rooms));
        dungeonCreator = new DungeonCreator(dungeonData.RoomCount, 
            new RandomSidePicker(),
            new BoundaryDataSelector(), 
            new FluctuationSizeSelector(),
            new RectIntDefiner());
        dungeonCreator.Create();
    }

    [SerializeField, Range(0, 100)] int debugIndex = 0;
    [SerializeField] bool allBoundaryData = false;

    [Space]
    [SerializeField] bool DebugContainsPosition = false;
    [SerializeField] Transform cube;
    [Space]
    [SerializeField] bool exceptBD;
    [SerializeField, Range(0, 3)] int exceptBDIndex;
    private void OnDrawGizmos()
    {
        if(dungeonCreator == null) return;
        dungeonCreator.DrawDebug(debugIndex, allBoundaryData);

        if(dungeonCreator.dungeon.Count > debugIndex && DebugContainsPosition)
            Debug.Log(dungeonCreator.dungeon[debugIndex].Rect.Contains(Mathf.RoundToInt(cube.position.x), Mathf.RoundToInt(cube.position.z)));

        if (exceptBD)
            dungeonCreator.DrawExceptBoundaryData(exceptBDIndex);
    }
}
