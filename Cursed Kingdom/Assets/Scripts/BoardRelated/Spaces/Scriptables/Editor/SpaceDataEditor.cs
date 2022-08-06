using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpaceData))]
public class SpaceDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        SerializedProperty spaceTypesProp = serializedObject.FindProperty(nameof(SpaceData.thisSpaceTypes));

        for (int i = 0; i < spaceTypesProp.arraySize; i++)
        {
            SerializedProperty elementToCheck = spaceTypesProp.GetArrayElementAtIndex(i);
            SpaceData.SpaceType elementToCheckEnum = (SpaceData.SpaceType)elementToCheck.intValue;
            SerializedProperty dataProp;
            switch (elementToCheckEnum)
            {
                case SpaceData.SpaceType.DrawMovementCard:
                    {
                        dataProp = serializedObject.FindProperty(nameof(SpaceData.drawMovementCardAmount));
                        EditorGUILayout.PropertyField(dataProp);
                        break;
                    }
                case SpaceData.SpaceType.DrawSupportCard:
                    {
                        dataProp = serializedObject.FindProperty(nameof(SpaceData.drawSupportCardAmount));
                        EditorGUILayout.PropertyField(dataProp);
                        break;
                    }
                case SpaceData.SpaceType.Poison:
                    {
                        dataProp = serializedObject.FindProperty(nameof(SpaceData.turnsPoisoned));
                        EditorGUILayout.PropertyField(dataProp);
                        break;
                    }
                case SpaceData.SpaceType.Curse:
                    {
                        dataProp = serializedObject.FindProperty(nameof(SpaceData.turnsCursed));
                        EditorGUILayout.PropertyField(dataProp);
                        break;
                    }
                case SpaceData.SpaceType.RecoverHealth:
                    {
                        dataProp = serializedObject.FindProperty(nameof(SpaceData.healthRecoveryAmount));
                        EditorGUILayout.PropertyField(dataProp);
                        break;
                    }
                case SpaceData.SpaceType.LoseHealth:
                    {
                        dataProp = serializedObject.FindProperty(nameof(SpaceData.damageTakenAmount));
                        EditorGUILayout.PropertyField(dataProp);
                        break;
                    }
                case SpaceData.SpaceType.Barricade:
                    {
                        dataProp = serializedObject.FindProperty(nameof(SpaceData.levelToPassBarricade));
                        EditorGUILayout.PropertyField(dataProp);
                        break;
                    }
                case SpaceData.SpaceType.NonDuel:
                    {
                        break;
                    }
                case SpaceData.SpaceType.SpecialAttack:
                    {
                        break;
                    }
                case SpaceData.SpaceType.LevelUp:
                    {
                        break;
                    }
                case SpaceData.SpaceType.ArrowSpace:
                    {
                        break;
                    }
                case SpaceData.SpaceType.Attack:
                    {
                        dataProp = serializedObject.FindProperty(nameof(SpaceData.damageGivenAmount));
                        EditorGUILayout.PropertyField(dataProp);
                        break;
                    }
                case SpaceData.SpaceType.Misc:
                    {
                        break;
                    }
                case SpaceData.SpaceType.StartEffect:
                    {
                        break;
                    }
                case SpaceData.SpaceType.AfterDuel:
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        EditorUtility.SetDirty(serializedObject.targetObject);
        serializedObject.ApplyModifiedProperties();
       
    }
}
