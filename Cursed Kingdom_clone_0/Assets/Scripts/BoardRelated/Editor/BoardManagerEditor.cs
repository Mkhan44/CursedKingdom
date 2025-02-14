using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoardManager))]
public class BoardManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        BoardManager boardManager = target as BoardManager;

        GUILayout.Space(30f);

        if (GUILayout.Button("Generate Board Layout"))
        {
            
            if(boardManager == null)
            {
                return;
            }
            boardManager.StartupSetupSpaces();
            EditorUtility.SetDirty(target);
            AssetDatabase.Refresh();
        }

        GUILayout.Space(40f);

        if(GUILayout.Button("Manually save changes"))
        {
            EditorUtility.SetDirty(target);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Notice", "Changes to the prefab have been saved.", "Cool!");
            Debug.Log("Changed have been saved.");
        }

    }
}
