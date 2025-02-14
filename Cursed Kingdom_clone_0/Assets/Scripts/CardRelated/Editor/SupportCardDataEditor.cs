//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.IO;

[CustomEditor(typeof(SupportCardData))]
[CanEditMultipleObjects]
public class SupportCardDataEditor : Editor
{
    SupportCardData supportCardData;
    SerializedProperty numSupportCardTypesProp;

    string[] choices = new string[0];
    List<Type> types = new List<Type>();
    int[] choiceIndeces = new int[0];

    const string SUPPORT_CARD_EFFECTS_FOLDER = "Support Card Effects";

    private void OnEnable()
    {
        int numTypesInInterface = 0;
        types = FindClassesUsingInterface(out numTypesInInterface);
        Array.Resize(ref choices, numTypesInInterface);
        //  Array.Resize(ref choiceIndeces, numTypesInInterface);

        for (int i = 0; i < types.Count; i++)
        {
            choices[i] = types[i].Name;
        }

    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        numSupportCardTypesProp = serializedObject.FindProperty(nameof(SupportCardData.supportCardEffects));
        supportCardData = target as SupportCardData;

        List<Type> currentTypes = new();

        EditorGUILayout.Space();

        GUILayout.Label("FOR EDITOR ONLY -- GENERATE SCRIPTABLES BELOW.");

        Array.Resize(ref choiceIndeces, supportCardData.supportCardEffects.Count);

        for (int i = 0; i < numSupportCardTypesProp.arraySize; i++)
        {
            EditorGUILayout.Space();

            if (supportCardData.supportCardEffects[i].supportTypeStringHidden == string.Empty)
            {
                supportCardData.supportCardEffects[i].supportTypeStringHidden = choices[0];
            }

            if (supportCardData.supportCardEffects[i].supportCardEffectData != null)
            {
                choiceIndeces[i] = Array.IndexOf(choices, supportCardData.supportCardEffects[i].supportCardEffectData.GetType().ToString());
            }
            else
            {
                choiceIndeces[i] = Array.IndexOf(choices, supportCardData.supportCardEffects[i].supportTypeStringHidden);
            }

            choiceIndeces[i] = EditorGUILayout.Popup($"Support Card Type {i}: ", choiceIndeces[i], choices);
            supportCardData.supportCardEffects[i].supportTypeStringHidden = choices[choiceIndeces[i]];
            EditorUtility.SetDirty(target);

        }



        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Support Card Effect Scriptables"))
        {
            if (supportCardData.supportCardEffects.Count < 1)
            {
                Debug.LogWarning("Please add some items to the support card effects list by clicking the + icon.");
                return;
            }

            string supportCardDataParentFolderName = $"{supportCardData.name}";
            string supportCardDataAssetPath = AssetDatabase.GetAssetPath(supportCardData);
            string supportCardDataFolderPath = Path.GetDirectoryName(supportCardDataAssetPath);
            string supportCardEffectNewFolderPath = supportCardDataAssetPath.Replace($"{supportCardData.name}.asset", $"{SUPPORT_CARD_EFFECTS_FOLDER}/");
            string supportCardEffectFolderPath = string.Empty;

            //Check that the supportCardData is in a folder with it's name. If it's not -- create a folder and put it into that folder.
            if (!supportCardDataFolderPath.EndsWith(supportCardDataParentFolderName))
            {
                string newFolderGUID = AssetDatabase.CreateFolder(supportCardDataFolderPath, supportCardDataParentFolderName);
                string newPath = AssetDatabase.GUIDToAssetPath(newFolderGUID);
                UnityEngine.Object assetName = AssetDatabase.LoadMainAssetAtPath(supportCardDataAssetPath);
                string testString = AssetDatabase.MoveAsset(supportCardDataAssetPath, $"{newPath}/{supportCardData.name}.asset");
                AssetDatabase.Refresh();
                supportCardDataAssetPath = AssetDatabase.GetAssetPath(supportCardData);
                supportCardDataFolderPath = Path.GetDirectoryName(supportCardDataAssetPath);
            }


            //Check for the support card effects folder. If it's not there, create it and make that the path for the newly generated assets. If it DOES exist, hit that folder up.
            if (!AssetDatabase.IsValidFolder(supportCardEffectNewFolderPath))
            {
                string newFolderGUID = AssetDatabase.CreateFolder(supportCardDataFolderPath, SUPPORT_CARD_EFFECTS_FOLDER);
                string newPath = AssetDatabase.GUIDToAssetPath(newFolderGUID);
                supportCardEffectNewFolderPath = newPath;
                AssetDatabase.Refresh();
            }

            supportCardEffectFolderPath = supportCardEffectNewFolderPath;

            for (int i = 0; i < supportCardData.supportCardEffects.Count(); i++)
            {

                Type scriptableType = GetTypeFromString(i);
                ScriptableObject scriptableInstance = CreateInstance(scriptableType);
                string scriptableFinalName = TrimStringName($"{scriptableType.ToString()}.asset", supportCardData.name);
                string scriptableTypePath = AssetDatabase.GenerateUniqueAssetPath($"{supportCardEffectFolderPath}/{scriptableFinalName}");
                AssetDatabase.CreateAsset(scriptableInstance, scriptableTypePath);
                string test = (AssetDatabase.GetAssetPath(scriptableInstance));
                AssetDatabase.SaveAssets();
                supportCardData.supportCardEffects[i].supportCardEffectData = scriptableInstance as SupportCardEffectData;
                AssetDatabase.Refresh();

            }
        }

        EditorUtility.SetDirty(serializedObject.targetObject);
        serializedObject.ApplyModifiedProperties();
    }

    private Type GetTypeFromString(int i)
    {
        Type textType = null;
        string typeName = supportCardData.supportCardEffects[i].supportTypeStringHidden;
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            textType = assembly.GetType(typeName);
            if (textType != null)
                break;
        }

        return textType;
    }

    private string TrimStringName(string stringToTrim, string contentToTrimOff = null)
    {
        if (contentToTrimOff == null)
        {
            contentToTrimOff = string.Empty;
        }
        stringToTrim.Replace(contentToTrimOff, string.Empty);

        return stringToTrim;
    }

    private static List<Type> FindClassesUsingInterface(out int numTypes)
    {
        List<Type> theTypes = new();
        numTypes = 0;
        Type type = typeof(ISupportEffect);
        IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(x => x.GetTypes())
                        .Where(x => x.IsClass && type.IsAssignableFrom(x));

        foreach (Type t in types)
        {
            ISupportEffect obj = CreateInstance(t) as ISupportEffect;
            numTypes += 1;
            if (obj.GetType() != typeof(SupportCardEffectData))
            {
                theTypes.Add(t);
            }
            // Debug.Log(t.Name);
        }

        return theTypes;
    }
}
