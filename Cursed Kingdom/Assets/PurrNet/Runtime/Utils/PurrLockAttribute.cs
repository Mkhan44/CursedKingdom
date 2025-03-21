using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PurrNet.Utils
{
    public class PurrLockAttribute : PropertyAttribute
    {
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(PurrLockAttribute))]
    public class PurrLockDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool shouldLock = Application.isPlaying &&
                              !PrefabUtility.IsPartOfPrefabAsset(property.serializedObject.targetObject);

            if (shouldLock)
            {
                var old = GUI.enabled;
                if (old) GUI.enabled = false;
                EditorGUI.PropertyField(position, property, label);
                if (old) GUI.enabled = true;
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
#endif
}