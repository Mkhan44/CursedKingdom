using System;
using UnityEditor;
using UnityEngine;

namespace PurrNet.Editor
{
    [CustomEditor(typeof(StatisticsManager), true)]
    public class StatisticsManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty _scriptProp;
        private SerializedProperty _placementProp;
        private SerializedProperty _displayTypeProp;
        private SerializedProperty _fontSizeProp;
        private SerializedProperty _textColorProp;
        private bool _displaySettingsFoldout = true;

        private void OnEnable()
        {
            _scriptProp = serializedObject.FindProperty("m_Script");
            _placementProp = serializedObject.FindProperty("placement");
            _displayTypeProp = serializedObject.FindProperty("displayType");
            _fontSizeProp = serializedObject.FindProperty("fontSize");
            _textColorProp = serializedObject.FindProperty("textColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var statisticsManager = (StatisticsManager)target;

            GUI.enabled = false;
            EditorGUILayout.PropertyField(_scriptProp, true);
            GUI.enabled = true;

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Collection Settings", EditorStyles.boldLabel);
            statisticsManager.checkInterval =
                EditorGUILayout.Slider("Check Rate In Seconds", statisticsManager.checkInterval, 0.05f, 1f);

            GUILayout.Space(10);
            _displaySettingsFoldout = EditorGUILayout.Foldout(_displaySettingsFoldout, "Display Settings", true);
            if (_displaySettingsFoldout)
            {
                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();
                _placementProp.enumValueIndex =
                    (int)(StatisticsManager.StatisticsPlacement)EditorGUILayout.EnumPopup("Placement",
                        (StatisticsManager.StatisticsPlacement)_placementProp.enumValueIndex);
                _displayTypeProp.enumValueIndex =
                    (int)(StatisticsManager.StatisticsDisplayType)EditorGUILayout.EnumPopup("Display Type",
                        (StatisticsManager.StatisticsDisplayType)_displayTypeProp.enumValueIndex);

                float newFontSize = EditorGUILayout.Slider("Font Size", _fontSizeProp.floatValue, 8f, 32f);
                if (Math.Abs(newFontSize - _fontSizeProp.floatValue) > 0.01f)
                {
                    _fontSizeProp.floatValue = newFontSize;
                }

                EditorGUILayout.PropertyField(_textColorProp, new GUIContent("Text Color"));

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Statistics Preview", EditorStyles.boldLabel);
            RenderStatistics(statisticsManager);

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }

            Repaint();
        }

        private void RenderStatistics(StatisticsManager statisticsManager)
        {
            serializedObject.Update();

            if (!statisticsManager.connectedServer && !statisticsManager.connectedClient)
            {
                EditorGUILayout.LabelField("Awaiting connection");
                return;
            }

            if (statisticsManager.connectedClient)
            {
                GUILayout.BeginHorizontal();
                DrawLed(GetPingStatus(statisticsManager));
                EditorGUILayout.LabelField($"Ping:");
                EditorGUILayout.LabelField($"{statisticsManager.ping}ms");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                DrawLed(GetJitterStatus(statisticsManager));
                EditorGUILayout.LabelField($"Jitter:");
                EditorGUILayout.LabelField($"{statisticsManager.jitter}ms");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                DrawLed(GetPacketLossStatus(statisticsManager));
                EditorGUILayout.LabelField($"Packet Loss:");
                EditorGUILayout.LabelField($"{statisticsManager.packetLoss}%");
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            DrawLed(Status.green);
            EditorGUILayout.LabelField($"Upload:");
            EditorGUILayout.LabelField($"{statisticsManager.upload}KB/s");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawLed(Status.green);
            EditorGUILayout.LabelField($"Download:");
            EditorGUILayout.LabelField($"{statisticsManager.download}KB/s");
            GUILayout.EndHorizontal();
        }

        private static Status GetPingStatus(StatisticsManager statisticsManager)
        {
            return statisticsManager.ping switch
            {
                < 50 => Status.green,
                < 100 => Status.yellow,
                < 200 => Status.orange,
                _ => Status.red
            };
        }

        private Status GetJitterStatus(StatisticsManager statisticsManager)
        {
            if (statisticsManager.jitter < 10)
                return Status.green;
            if (statisticsManager.jitter < 20)
                return Status.yellow;
            if (statisticsManager.jitter < 40)
                return Status.orange;
            return Status.red;
        }

        private Status GetPacketLossStatus(StatisticsManager statisticsManager)
        {
            if (statisticsManager.packetLoss < 11)
                return Status.green;
            if (statisticsManager.packetLoss < 21)
                return Status.yellow;
            if (statisticsManager.packetLoss < 31)
                return Status.orange;
            return Status.red;
        }

        static void DrawLed(Status status)
        {
            var white = Texture2D.whiteTexture;
            var color = status switch
            {
                Status.green => Color.green,
                Status.yellow => Color.yellow,
                Status.orange => new Color(1, 0.5f, 0),
                _ => Color.red
            };

            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            var rect = GUILayoutUtility.GetLastRect();
            rect.height = EditorGUIUtility.singleLineHeight;

            const float padding = 5;

            rect.x += padding;
            rect.y += padding;

            rect.width -= padding * 2;
            rect.height -= padding * 2;

            GUI.DrawTexture(rect, white, ScaleMode.StretchToFill, true, 1f, color, 0, 10f);
        }

        private enum Status
        {
            green,
            yellow,
            orange,
            red
        }
    }
}