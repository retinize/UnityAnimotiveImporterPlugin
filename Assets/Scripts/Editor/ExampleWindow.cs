using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ExampleWindow : EditorWindow
{
    private const string _StreamingAssets = "StreamingAssets";

    private string _basePath = "";
    public string Path = "/StreamingAssets/RootFolder";

    private bool _validPathChoosen = false;


    [MenuItem("Animotive/Importer")]
    public static void ShowWindow()
    {
        ExampleWindow window = GetWindow<ExampleWindow>("Example");
        window.Show();
    }


    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("Root Folder Path", EditorStyles.boldLabel);
        Path = EditorGUILayout.TextField(Path);

        if (GUILayout.Button("Browse"))
        {
            string str = EditorUtility.OpenFolderPanel("Choose Root Folder", "Assets/StreamingAssets", "");
            if (!string.IsNullOrEmpty(str) && str.Contains(_StreamingAssets))
            {
                string[] splitArray = str.Split(new[] { _StreamingAssets }, StringSplitOptions.None);

                _basePath = splitArray[0];
                string spliitted = splitArray[1];

                if (string.IsNullOrEmpty(spliitted))
                {
                    _validPathChoosen = false;
                    Debug.LogWarning("Can not load from StreamingAssets directly !");
                }
                else
                {
                    Path = string.Concat(_StreamingAssets, "/", splitArray[1]);
                    _validPathChoosen = true;
                    Debug.Log(spliitted);
                }
            }
            else
            {
                Path = null;
                _validPathChoosen = false;
                Debug.LogWarning("Root file should be under Streaming Assets !");
            }
        }

        EditorGUILayout.EndHorizontal();


        if (GUILayout.Button("Create Test scene"))
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }


        EditorGUI.BeginDisabledGroup(!_validPathChoosen);


        EditorGUI.EndDisabledGroup();
    }
}