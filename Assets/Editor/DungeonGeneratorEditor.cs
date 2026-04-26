using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonGenerator), true)]
public class DungeonGeneratorEditor : Editor
{
    DungeonGenerator generator;

    private void Awake()
    {
        generator = (DungeonGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate dangeon"))
            generator.Generate();
    }
}
