using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace QuiteSensible
{
    [CustomEditor(typeof(LevelCreator), true)]
    public class LevelCreatorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelCreator lcScript = (LevelCreator)target;
            if (GUILayout.Button("Gen/Regen Landscape"))
                lcScript.CreateLandscapeMesh();
        }
    }
}