using UnityEditor;
using UnityEngine;

namespace PurrNet.Editor
{
    [CustomEditor(typeof(NetworkRules))]
    public class NetworkRulesInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Label("Network Rules", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
            const string description = "This asset is used to set the default rules of a Network manager. " +
                                       "Modifying these rules will change how things act over the network. ";

            GUILayout.Label(description, DescriptionStyle());
            GUILayout.Space(10);

            DrawDefaultInspector();
        }

        private static GUIStyle DescriptionStyle()
        {
            var headerStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true
            };

            return headerStyle;
        }
    }
}