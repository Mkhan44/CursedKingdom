#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace PurrNet
{
    [CustomEditor(typeof(NetworkPrefabs))]
    public class NetworkPrefabsEditor : UnityEditor.Editor
    {
        private NetworkPrefabs networkPrefabs;
        private SerializedProperty prefabs;
        private bool? allPoolingState = null;
        private ReorderableList reorderableList;
        
        private const float POOL_TOGGLE_WIDTH = 45f;
        const float WARMUP_COUNT_WIDTH = 60f;
        private const float SPACING = 8f;
        private const float REORDERABLE_LIST_BUTTON_WIDTH = 25f;
        
        private void OnEnable()
        {
            networkPrefabs = (NetworkPrefabs)target;
            prefabs = serializedObject.FindProperty("prefabs");
            
            if (networkPrefabs.autoGenerate)
                networkPrefabs.Generate();
                
            UpdateAllPoolingState();
            SetupReorderableList();
        }

        private void SetupReorderableList()
        {
            reorderableList = new ReorderableList(serializedObject, prefabs, true, true, true, true);
            reorderableList.elementHeight = EditorGUIUtility.singleLineHeight;
            
            reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                float fullWidth = rect.width - REORDERABLE_LIST_BUTTON_WIDTH;
                CalculateWidths(fullWidth, out float prefabWidth, out float poolWidth, out float warmupWidth);

                EditorGUI.LabelField(new Rect(rect.x, rect.y, prefabWidth, rect.height), "Prefab");
                EditorGUI.LabelField(new Rect(rect.x + prefabWidth + SPACING, rect.y, poolWidth + warmupWidth, rect.height), "Pool");
            };

            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = prefabs.GetArrayElementAtIndex(index);
                SerializedProperty prefabProp = element.FindPropertyRelative("prefab");
                SerializedProperty poolProp = element.FindPropertyRelative("pooled");
                SerializedProperty warmupCountProp = element.FindPropertyRelative("warmupCount");

                float fullWidth = rect.width - REORDERABLE_LIST_BUTTON_WIDTH;
                CalculateWidths(fullWidth, out float prefabWidth, out float poolWidth, out float warmupWidth);

                EditorGUI.PropertyField(new Rect(rect.x, rect.y, prefabWidth, rect.height), prefabProp, GUIContent.none);
                poolProp.boolValue = EditorGUI.Toggle(new Rect(rect.x + prefabWidth + SPACING, rect.y, poolWidth, rect.height), poolProp.boolValue);

                if (poolProp.boolValue)
                {
                    EditorGUI.PropertyField(new Rect(rect.x + prefabWidth + poolWidth + (SPACING * 2), rect.y, warmupWidth, rect.height), warmupCountProp, GUIContent.none);
                }
            };

            reorderableList.onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Add Empty Entry"), false, () =>
                {
                    int index = list.count;
                    list.serializedProperty.arraySize++;
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);
                    element.FindPropertyRelative("prefab").objectReferenceValue = null;
                    element.FindPropertyRelative("pooled").boolValue = networkPrefabs.poolByDefault;
                    element.FindPropertyRelative("warmupCount").intValue = 5;
                    serializedObject.ApplyModifiedProperties();
                    UpdateAllPoolingState();
                });
                
                menu.AddItem(new GUIContent("Add Selected Prefabs"), false, () =>
                {
                    bool addedAny = false;
                    foreach (var obj in Selection.gameObjects)
                    {
                        if (PrefabUtility.IsPartOfPrefabAsset(obj))
                        {
                            addedAny = true;
                            int index = list.count;
                            list.serializedProperty.arraySize++;
                            var element = list.serializedProperty.GetArrayElementAtIndex(index);
                            element.FindPropertyRelative("prefab").objectReferenceValue = obj;
                            element.FindPropertyRelative("pooled").boolValue = networkPrefabs.poolByDefault;
                            element.FindPropertyRelative("warmupCount").intValue = 5;
                        }
                    }
                    if (addedAny)
                    {
                        serializedObject.ApplyModifiedProperties();
                        UpdateAllPoolingState();
                    }
                });
                
                menu.ShowAsContext();
            };
        }
        
        private void CalculateWidths(float fullWidth, out float prefabWidth, out float poolWidth, out float warmupWidth)
        {
            float spacing = SPACING;
            poolWidth = 20f;
            warmupWidth = 60f;
            prefabWidth = fullWidth - poolWidth - warmupWidth - (spacing * 2);
        }

        private void UpdateAllPoolingState()
        {
            if (prefabs.arraySize == 0)
            {
                allPoolingState = null;
                return;
            }

            bool firstState = prefabs.GetArrayElementAtIndex(0).FindPropertyRelative("pooled").boolValue;
            allPoolingState = firstState;

            for (int i = 1; i < prefabs.arraySize; i++)
            {
                if (prefabs.GetArrayElementAtIndex(i).FindPropertyRelative("pooled").boolValue != firstState)
                {
                    allPoolingState = null;
                    return;
                }
            }
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Label("Network Prefabs", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
            const string description = "This asset is used to store any prefabs containing a Network Behaviour. " +
                                     "You can add prefabs to this asset manually or auto generate the references. " +
                                     "This list is used by the NetworkManager to spawn network prefabs.";

            GUILayout.Label(description, DescriptionStyle());

            GUILayout.Space(10);

            // Generation Settings
            EditorGUILayout.LabelField("Generation Settings", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("folder"), new GUIContent("Folder"));
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(networkPrefabs);
            }

            // Toggle buttons row
            GUILayout.BeginHorizontal();

            DrawToggleButton("Auto generate", ref networkPrefabs.autoGenerate);
            DrawToggleButton("Networked only", ref networkPrefabs.networkOnly);
            DrawToggleButton("Default pooling", ref networkPrefabs.poolByDefault);

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Generate", GUILayout.Width(1), GUILayout.ExpandWidth(true)))
            {
                networkPrefabs.Generate();
                serializedObject.ApplyModifiedProperties();
                prefabs = serializedObject.FindProperty("prefabs");
                UpdateAllPoolingState();
            }
            
            GUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(networkPrefabs.autoGenerate);
            reorderableList.DoLayoutList();
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(networkPrefabs);
            }
        }

        private void DrawToggleButton(string label, ref bool value)
        {
            GUI.color = value ? Color.green : Color.white;
            if (GUILayout.Button(label, GUILayout.Width(1), GUILayout.ExpandWidth(true)))
            {
                value = !value;
                if (networkPrefabs.autoGenerate)
                {
                    networkPrefabs.Generate();
                    serializedObject.ApplyModifiedProperties();
                    prefabs = serializedObject.FindProperty("prefabs");
                    UpdateAllPoolingState();
                }
                EditorUtility.SetDirty(networkPrefabs);
            }
            GUI.color = Color.white;
        }

        private static GUIStyle DescriptionStyle()
        {
            return new GUIStyle(GUI.skin.label)
            {
                wordWrap = true
            };
        }
    }
}
#endif