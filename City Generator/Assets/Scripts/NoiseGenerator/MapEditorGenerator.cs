using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (MapGenerator))]
public class MapEditorGenerator : Editor
{
    public override void OnInspectorGUI()
    {
        // Don't call base.OnInspectorGUI() or original gui will be shown
        MapGenerator mapGenerator = (MapGenerator) target;

        if (DrawDefaultInspector ()) {
            if (mapGenerator.autoUpdate){
                mapGenerator.GenerateMap();
            }
        }

        if (GUILayout.Button("Generate")) {
            mapGenerator.GenerateMap();
        }
    }
}
