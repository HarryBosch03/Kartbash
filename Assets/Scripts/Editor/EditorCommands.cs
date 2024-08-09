using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Editor
{
    [InitializeOnLoad]
    public static class EditorCommands
    {
        static EditorCommands()
        {
            EditorApplication.update += Update;
        }
        
        private static void Update()
        {
            var kb = Keyboard.current;

            if (kb.leftShiftKey.isPressed && kb.rKey.wasPressedThisFrame)
            {
                var scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);
                Debug.Log($"Reloading Scene \"{scene.name}\"");
            }
        }
    }
}