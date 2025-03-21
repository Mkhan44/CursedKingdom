using UnityEditor;
using UnityEngine;

namespace PurrNet.Editor
{
    [InitializeOnLoad]
    public class CustomSceneViewDelete
    {
        static CustomSceneViewDelete()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            bool isPlaying = Application.isPlaying;

            if (!isPlaying)
                return;

            var currentEvent = Event.current;

            switch (currentEvent.type)
            {
                case EventType.ExecuteCommand when currentEvent.commandName == "Paste":
                case EventType.ExecuteCommand when currentEvent.commandName == "Duplicate":
                {
                    EditorApplication.delayCall += () =>
                    {
                        foreach (var go in Selection.gameObjects)
                            NetworkIdentity.SpawnInternal(go);
                    };
                    break;
                }
                // Check if Delete or Backspace is pressed
                case EventType.KeyDown when
                    currentEvent.keyCode is KeyCode.Delete or KeyCode.Backspace:
                {
                    // Get selected objects
                    var selectedObjects = Selection.objects;

                    if (selectedObjects.Length > 0)
                    {
                        if (PurrDeleteHandler.CustomDeleteLogic(selectedObjects))
                            currentEvent.Use();
                    }

                    break;
                }
            }
        }
    }
}