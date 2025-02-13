using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PurrNet.Editor
{
    [CustomEditor(typeof(NetworkIdentity), true)]
    [CanEditMultipleObjects]
#if TRI_INSPECTOR_PACKAGE
    public class NetworkIdentityInspector : TriInspector.Editors.TriEditor
#else
    public class NetworkIdentityInspector : UnityEditor.Editor
#endif
    {
        private SerializedProperty _networkRules;
        private SerializedProperty _visitiblityRules;
        
#if TRI_INSPECTOR_PACKAGE
        protected override void OnEnable()
#else
        protected virtual void OnEnable()
#endif
        {
#if TRI_INSPECTOR_PACKAGE
            base.OnEnable();
#endif
            try
            {
                _networkRules = serializedObject.FindProperty("_networkRules");
                _visitiblityRules = serializedObject.FindProperty("_visitiblityRules");
            }
            catch
            {
                // ignored
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            return null;
        }

        public override void OnInspectorGUI()
        {
            var identity = (NetworkIdentity)target;
            bool hasNetworkManagerAsChild = identity && identity.GetComponentInChildren<NetworkManager>();

            if (hasNetworkManagerAsChild)
                EditorGUILayout.HelpBox("NetworkIdentity is a child of a NetworkManager. This is not supported.", MessageType.Error);
            
            base.OnInspectorGUI();
            
            DrawIdentityInspector();
            
            serializedObject.ApplyModifiedProperties();
        }

        protected void DrawIdentityInspector()
        {
            GUILayout.Space(5);

            var identities = targets.Length;
            var identity = (NetworkIdentity)target;
            
            if (!identity)
            {
                EditorGUILayout.LabelField("Invalid identity");
                return;
            }

            
            HandleOverrides(identity, identities > 1);
            HandleStatus(identity, identities > 1);
        }
        
        private bool _foldoutVisible;

        private void HandleOverrides(NetworkIdentity identity, bool multi)
        {
            if (multi || identity.isSpawned)
                GUI.enabled = false;
            
            string label = "Override Defaults";

            if (!multi)
            {
                bool isNetworkRulesOverridden = _networkRules.objectReferenceValue != null;
                bool isVisibilityRulesOverridden = _visitiblityRules.objectReferenceValue != null;

                int overridenCount = (isNetworkRulesOverridden ? 1 : 0) + (isVisibilityRulesOverridden ? 1 : 0);

                if (overridenCount > 0)
                {
                    label += " (";

                    if (isNetworkRulesOverridden)
                    {
                        label += overridenCount > 1 ? "P," : "P";
                    }

                    if (isVisibilityRulesOverridden)
                        label += "V";

                    label += ")";
                }
            }
            else
            {
                label += " (...)";
            }

            var old = GUI.enabled;
            GUI.enabled = !multi;
            _foldoutVisible = EditorGUILayout.BeginFoldoutHeaderGroup(_foldoutVisible, label);
            GUI.enabled = old;
            if (!multi && _foldoutVisible)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_networkRules, new GUIContent("Permissions Override"));
                EditorGUILayout.PropertyField(_visitiblityRules, new GUIContent("Visibility Override"));
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        
        private bool _debuggingVisible;
        private bool _observersVisible;

        private void HandleStatus(NetworkIdentity identity, bool multi)
        {
            if (multi)
            {
                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.LabelField("...");
                EditorGUILayout.EndHorizontal();
            }
            else if (identity.isSpawned)
            {
                if (identity.isServer)
                {
                    var old = GUI.enabled;
                    GUI.enabled = true;
                    PrintObserversDropdown(identity);
                    GUI.enabled = old;
                }

                EditorGUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(false));
                GUILayout.Label($"ID: {identity.id}", GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                GUILayout.Label($"Owner ID: {(identity.owner.HasValue ? identity.owner.Value.ToString() : "None")}", GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                GUILayout.Label($"Local Player: {(identity.localPlayer.HasValue ? identity.localPlayer.Value.ToString() : "None")}", GUILayout.ExpandWidth(false));
                EditorGUILayout.EndHorizontal();
            }
            else if (Application.isPlaying)
            {
                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.LabelField("Not Spawned");
                EditorGUILayout.EndHorizontal();
            }
            
#if PURRNET_DEBUG_NETWORK_IDENTITY
            var old2 = GUI.enabled;
            GUI.enabled = false;

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(false));
            
            EditorGUILayout.LabelField($"prefabId: {identity.prefabId}");
            EditorGUILayout.LabelField($"componentIndex: {identity.componentIndex}");
            EditorGUILayout.LabelField($"shouldBePooled: {identity.shouldBePooled}");
            EditorGUILayout.ObjectField("parent", identity.parent, typeof(NetworkIdentity), true);
            
            string path = "";

            if (identity.invertedPathToNearestParent != null)
            {
                for (var index = 0; index < identity.invertedPathToNearestParent.Length; index++)
                {
                    var parent = identity.invertedPathToNearestParent[index];
                    bool isLast = index == identity.invertedPathToNearestParent.Length - 1;
                    path += parent + (isLast ? ";" : " -> ");
                }
            }

            EditorGUILayout.LabelField($"pathToNearestParent: {path}");
            EditorGUILayout.LabelField($"Direct Children ({identity.directChildren?.Count ?? 0}):");
            
            if (identity.directChildren != null)
            {
                EditorGUI.indentLevel++;
                foreach (var child in identity.directChildren)
                {
                    EditorGUILayout.ObjectField(child, typeof(NetworkIdentity), true);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            GUI.enabled = old2;
#endif
        }

        private void PrintObserversDropdown(NetworkIdentity identity)
        {
            _observersVisible = EditorGUILayout.BeginFoldoutHeaderGroup(_observersVisible, $"Observers ({identity.observers.Count})");

            if (_observersVisible)
            {
                EditorGUI.indentLevel++;
                foreach (var observer in identity.observers)
                {
                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField(observer.ToString());
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}