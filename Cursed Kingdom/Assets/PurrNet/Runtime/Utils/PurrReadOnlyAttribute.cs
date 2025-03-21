using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PurrNet.Utils
{
    public class PurrReadOnlyAttribute : PropertyAttribute
    {
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(PurrReadOnlyAttribute))]
    public class PurrReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var old = GUI.enabled;
            if (old) GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            if (old) GUI.enabled = true;
        }
    }
#endif
}