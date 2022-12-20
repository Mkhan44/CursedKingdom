using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.IO;

[CustomEditor(typeof(SpaceData))]
[CanEditMultipleObjects]
public class SpaceDataEditor : Editor
{
    SpaceData spaceData;
    SerializedProperty numSpaceTypesProp;

    string[] choices = new string[0];
    List<Type> types = new List<Type>();
    int[] choiceIndeces = new int[0];

    const string SPACE_EFFECTS_FOLDER = "Space Effects";

    private void OnEnable()
    {
        int numTypesInInterface = 0;
        types = FindClassesUsingInterface(out numTypesInInterface);
        Array.Resize(ref choices, numTypesInInterface);
      //  Array.Resize(ref choiceIndeces, numTypesInInterface);

        for(int i = 0; i < types.Count; i++)
        {
            choices[i] = types[i].Name;
        }
        
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        numSpaceTypesProp = serializedObject.FindProperty(nameof(SpaceData.spaceTypeWrappers));
        spaceData = target as SpaceData;

        List<Type> currentTypes = new();

        EditorGUILayout.Space();

        GUILayout.Label("FOR EDITOR ONLY -- GENERATE SCRIPTABLES BELOW.");

        Array.Resize(ref choiceIndeces, spaceData.spaceTypeWrappers.Count);

        for (int i = 0; i < numSpaceTypesProp.arraySize; i++)
        {
            EditorGUILayout.Space();

            if(spaceData.spaceTypeWrappers[i].spaceTypeStringHidden == string.Empty)
            {
                spaceData.spaceTypeWrappers[i].spaceTypeStringHidden = choices[0];
            }

            if (spaceData.spaceTypeWrappers[i].spaceEffectData != null)
            {
                choiceIndeces[i] = Array.IndexOf(choices, spaceData.spaceTypeWrappers[i].spaceEffectData.GetType().ToString());
            }
            else
            {
                choiceIndeces[i] = Array.IndexOf(choices, spaceData.spaceTypeWrappers[i].spaceTypeStringHidden);
            }

            choiceIndeces[i] = EditorGUILayout.Popup($"Space Type {i}: ", choiceIndeces[i], choices);
            spaceData.spaceTypeWrappers[i].spaceTypeStringHidden = choices[choiceIndeces[i]];
            EditorUtility.SetDirty(target);

        }



        EditorGUILayout.Space();
 
        if (GUILayout.Button("Generate scriptable"))
        {
            string spaceDataParentFolderName = $"{spaceData.name}";
            string spaceDataAssetPath = AssetDatabase.GetAssetPath(spaceData);
            string spaceDataFolderPath = Path.GetDirectoryName(spaceDataAssetPath);
            string spaceEffectNewFolderPath = spaceDataAssetPath.Replace($"{spaceData.name}.asset", $"{SPACE_EFFECTS_FOLDER}/");
            string spaceEffectFolderPath = string.Empty;

            //Check that the spaceData is in a folder with it's name. If it's not -- create a folder and put it into that folder.
            if(!spaceDataFolderPath.EndsWith(spaceDataParentFolderName))
            {
                string newFolderGUID = AssetDatabase.CreateFolder(spaceDataFolderPath, spaceDataParentFolderName);
                string newPath = AssetDatabase.GUIDToAssetPath(newFolderGUID);
                UnityEngine.Object assetName = AssetDatabase.LoadMainAssetAtPath(spaceDataAssetPath);
                string testString = AssetDatabase.MoveAsset(spaceDataAssetPath, $"{newPath}/{spaceData.name}.asset");
                AssetDatabase.Refresh();
                spaceDataAssetPath = AssetDatabase.GetAssetPath(spaceData);
                spaceDataFolderPath = Path.GetDirectoryName(spaceDataAssetPath);
            }

            
            //Check for the space effects folder. If it's not there, create it and make that the path for the newly generated assets. If it DOES exist, hit that folder up.
            if (!AssetDatabase.IsValidFolder(spaceEffectNewFolderPath))
            {
                string newFolderGUID = AssetDatabase.CreateFolder(spaceDataFolderPath, SPACE_EFFECTS_FOLDER);
                string newPath = AssetDatabase.GUIDToAssetPath(newFolderGUID);
                spaceEffectNewFolderPath = newPath;
                AssetDatabase.Refresh();
            }

            spaceEffectFolderPath = spaceEffectNewFolderPath;

            for (int i = 0; i < spaceData.spaceTypeWrappers.Count(); i++)
            {

                Type scriptableType = GetTypeFromString(i);
                ScriptableObject scriptableInstance = CreateInstance(scriptableType);
                string scriptableTypePath = AssetDatabase.GenerateUniqueAssetPath($"{spaceEffectFolderPath}/{spaceData.name} {scriptableType.ToString()}.asset");
                AssetDatabase.CreateAsset(scriptableInstance, scriptableTypePath);
                string test = (AssetDatabase.GetAssetPath(scriptableInstance));
                Debug.Log(test);
                //AssetDatabase.RenameAsset(scriptableTypePath, spaceData.name + " " + scriptableType.ToString());
                AssetDatabase.SaveAssets();
                spaceData.spaceTypeWrappers[i].spaceEffectData = scriptableInstance as SpaceEffectData;
                AssetDatabase.Refresh();

            }
        }

        EditorUtility.SetDirty(serializedObject.targetObject);
        serializedObject.ApplyModifiedProperties();
    }

    private Type GetTypeFromString(int i)
    {
        Type textType = null;
        string typeName = spaceData.spaceTypeWrappers[i].spaceTypeStringHidden;
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            textType = assembly.GetType(typeName);
            if (textType != null)
                break;
        }

        return textType;
    }

    private static List<Type> FindClassesUsingInterface(out int numTypes)
    {
        List<Type> theTypes = new();
        numTypes = 0;
        Type type = typeof(ISpaceEffect);
        IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(x => x.GetTypes())
                        .Where(x => x.IsClass && type.IsAssignableFrom(x));

        foreach (Type t in types)
        {
            ISpaceEffect obj = CreateInstance(t) as ISpaceEffect;
            numTypes += 1;
            theTypes.Add(t);
            // Debug.Log(t.Name);
        }

        return theTypes;
    }
}
