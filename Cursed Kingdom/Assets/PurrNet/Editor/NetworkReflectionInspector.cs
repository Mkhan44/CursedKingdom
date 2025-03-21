using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PurrNet.Editor
{
    [CustomEditor(typeof(NetworkReflection), true)]
    public class NetworkReflectionInspector : NetworkIdentityInspector
    {
        private SerializedProperty _trackedBehaviour;
        private SerializedProperty _trackedFields;
        private SerializedProperty _ownerAuth;

        protected override void OnEnable()
        {
            base.OnEnable();

            _trackedBehaviour = serializedObject.FindProperty("_trackedBehaviour");
            _trackedFields = serializedObject.FindProperty("_trackedFields");
            _ownerAuth = serializedObject.FindProperty("_ownerAuth");
        }

        static readonly List<ReflectionData> _validNames = new List<ReflectionData>();

        void GetAllValidNames(List<ReflectionData> validNames)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var reflection = (NetworkReflection)target;
            var trackedType = reflection.trackedType;
            var fields = trackedType.GetFields(flags);
            var properties = trackedType.GetProperties(flags);

            foreach (var field in fields)
            {
                validNames.Add(new ReflectionData
                {
                    name = field.Name,
                    type = ReflectionType.Field
                });
            }

            foreach (var property in properties)
            {
                if (property.SetMethod == null || property.GetMethod == null)
                    continue;

                validNames.Add(new ReflectionData
                {
                    name = property.Name,
                    type = ReflectionType.Property
                });
            }
        }

        int GetIndexOfName(string propName)
        {
            for (var i = 0; i < _validNames.Count; i++)
            {
                if (_validNames[i].name == propName)
                    return i;
            }

            return -1;
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();
            DrawDefaultInspector();

            var reflection = (NetworkReflection)target;
            var trackedType = reflection.trackedType;

            EditorGUILayout.PropertyField(_trackedBehaviour);
            EditorGUILayout.PropertyField(_ownerAuth);

            if (trackedType == null)
            {
                EditorGUILayout.HelpBox("Tracked behaviour is null", MessageType.Error);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            _validNames.Clear();
            GetAllValidNames(_validNames);

            string[] names = new string[_validNames.Count];

            for (var i = 0; i < _validNames.Count; i++)
                names[i] = $"{_validNames[i].type}/{_validNames[i].name}";

            if (reflection.trackedFields == null)
            {
                reflection.trackedFields = new List<ReflectionData>();
                EditorUtility.SetDirty(reflection);
            }

            EditorGUI.BeginProperty(Rect.zero, GUIContent.none, _trackedFields);
            EditorGUILayout.BeginVertical("helpbox");
            for (var i = 0; i < reflection.trackedFields.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                int idx = GetIndexOfName(reflection.trackedFields[i].name);

                GUI.color = idx == -1 ? Color.yellow : Color.white;
                int newIdx = EditorGUILayout.Popup(idx, names);

                if (newIdx != idx)
                {
                    Undo.RecordObject(reflection, "Change field");
                    reflection.trackedFields[i] = _validNames[newIdx];
                    EditorUtility.SetDirty(reflection);
                }

                GUI.color = Color.white;

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Remove", GUILayout.ExpandWidth(false), GUILayout.Width(100)))
                {
                    Undo.RecordObject(reflection, "Remove field");
                    reflection.trackedFields.RemoveAt(i);
                    EditorUtility.SetDirty(reflection);
                    i--;
                }

                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Add", GUILayout.ExpandWidth(false), GUILayout.Width(100)))
            {
                Undo.RecordObject(reflection, "Add field");
                reflection.trackedFields.Add(new ReflectionData());
                EditorUtility.SetDirty(reflection);
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUI.EndProperty();

            DrawIdentityInspector();

            serializedObject.ApplyModifiedProperties();
        }
    }
}