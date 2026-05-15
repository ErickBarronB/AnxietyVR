using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityToolbarExtender;

[InitializeOnLoad]
public class SceneSwitchLeftButton
{
    static SceneSwitchLeftButton()
    {
        ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
    }

    static void OnToolbarGUI()
    {
        GUILayout.Space(5);

        if (DrawCustomSceneButton("Menu", "Menu"))
        {
            SceneHelper.StartScene("MainMenu");
        }

        if (DrawCustomSceneButton("Game", "Game"))
        {
            SceneHelper.StartScene("GameScene");
        }

        if (DrawCustomSceneButton("Select Scene", ""))
        {
            ShowSceneMenu();
        }
    }

    static bool DrawCustomSceneButton(string label, string sceneName)
    {
        string icon = string.IsNullOrEmpty(sceneName) ? "d_ViewToolOrbit" : "d_SceneAsset Icon";
        GUIContent content = EditorGUIUtility.IconContent(icon);
        content.text = label;

        return GUILayout.Button(content, EditorStyles.toolbarButton, GUILayout.Width(110));
    }

    static void ShowSceneMenu()
    {
        GenericMenu menu = new GenericMenu();

        string[] guids = AssetDatabase.FindAssets("t:Scene");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);

            menu.AddItem(new GUIContent(name), false, () => SceneHelper.StartScene(name));
        }

        menu.ShowAsContext();
    }
}

static class SceneHelper
{
    static string sceneToOpen;

    public static void StartScene(string sceneName)
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }

        sceneToOpen = sceneName;
        EditorApplication.update += OnUpdate;
    }

    static void OnUpdate()
    {
        if (sceneToOpen == null ||
            EditorApplication.isPlaying || EditorApplication.isPaused ||
            EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        EditorApplication.update -= OnUpdate;

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            string[] guids = AssetDatabase.FindAssets("t:scene");
            string scenePath = null;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

                if (fileName == sceneToOpen)
                {
                    scenePath = path;
                    break;
                }
            }

            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogWarning("The scene with the exact name could not be found: " + sceneToOpen);
            }
            else
            {
                EditorSceneManager.OpenScene(scenePath);
            }
        }
        sceneToOpen = null;
    }
}
