using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StreetGenerator))]
public class StreetGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Get a reference to the script
        StreetGenerator script = (StreetGenerator)target;

        // Add a button to the inspector
        if (GUILayout.Button("Remove Everything"))
        {
            // Call the method you want to execute
            script.RemoveEverything();
        }
        if (GUILayout.Button("Generate Streets"))
        {
            // Call the method you want to execute
            script.GenerateStreets();
        }
        if (GUILayout.Button("Generate Buildings"))
        {
            // Call the method you want to execute
            script.GenerateBuildings();
        }
    }
}