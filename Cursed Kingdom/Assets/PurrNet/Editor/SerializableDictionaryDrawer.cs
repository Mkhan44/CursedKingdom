#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace PurrNet.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        private const float HeaderHeight = 20f;
        private const float ElementPadding = 2f;
        private const float BottomPadding = 8f;
        private const float ColumnHeaderHeight = 18f;
        private bool _foldout = true;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var keysProp = property.FindPropertyRelative("keys");
            var stringKeysProp = property.FindPropertyRelative("stringKeys");
            var displayKeysProp = (keysProp != null && keysProp.arraySize > 0) ? keysProp : stringKeysProp;

            if (!_foldout) return HeaderHeight;

            float totalHeight = HeaderHeight + ColumnHeaderHeight;
            int count = displayKeysProp?.arraySize ?? 0;
            totalHeight += (EditorGUIUtility.singleLineHeight + ElementPadding) * count;
            totalHeight += BottomPadding;

            return totalHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var keysProp = property.FindPropertyRelative("keys");
            var valuesProp = property.FindPropertyRelative("values");
            var stringKeysProp = property.FindPropertyRelative("stringKeys");
            var stringValuesProp = property.FindPropertyRelative("stringValues");

            var useSerializableTypes = keysProp != null && keysProp.arraySize > 0;
            var displayKeysProp = useSerializableTypes ? keysProp : stringKeysProp;
            var displayValuesProp = useSerializableTypes ? valuesProp : stringValuesProp;

            Rect headerRect = new Rect(position.x, position.y, position.width, HeaderHeight);
            _foldout = EditorGUI.Foldout(headerRect, _foldout, label, true);

            if (_foldout)
            {
                EditorGUI.indentLevel++;
                float yOffset = HeaderHeight;

                Rect headerBgRect = new Rect(position.x, position.y + yOffset, position.width, ColumnHeaderHeight);
                EditorGUI.DrawRect(headerBgRect, new Color(0.7f, 0.7f, 0.7f, 0.1f));

                float headerLabelOffset = 2f;
                Rect keyHeaderRect = new Rect(position.x, position.y + yOffset + headerLabelOffset,
                    position.width * 0.45f, EditorGUIUtility.singleLineHeight);
                Rect valueHeaderRect = new Rect(position.x + position.width * 0.5f,
                    position.y + yOffset + headerLabelOffset, position.width * 0.45f,
                    EditorGUIUtility.singleLineHeight);

                EditorGUI.LabelField(keyHeaderRect, "Key");
                EditorGUI.LabelField(valueHeaderRect, "Value");

                yOffset += ColumnHeaderHeight;

                if (displayKeysProp != null && displayValuesProp != null)
                {
                    int count = displayKeysProp.arraySize;
                    for (int i = 0; i < count; i++)
                    {
                        float elementHeight = EditorGUIUtility.singleLineHeight;
                        Rect keyRect = new Rect(position.x, position.y + yOffset, position.width * 0.45f,
                            elementHeight);
                        Rect valueRect = new Rect(position.x + position.width * 0.5f, position.y + yOffset,
                            position.width * 0.45f, elementHeight);

                        if (i % 2 == 1)
                        {
                            Rect rowBgRect = new Rect(position.x, position.y + yOffset, position.width, elementHeight);
                            EditorGUI.DrawRect(rowBgRect, new Color(0.7f, 0.7f, 0.7f, 0.05f));
                        }

                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUI.PropertyField(keyRect, displayKeysProp.GetArrayElementAtIndex(i),
                                GUIContent.none);
                            EditorGUI.PropertyField(valueRect, displayValuesProp.GetArrayElementAtIndex(i),
                                GUIContent.none);
                        }

                        yOffset += elementHeight + ElementPadding;
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif