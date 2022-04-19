using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnimotiveImporterEditor
{
    using System.IO;
    using OdinSerializer;
    using UnityEngine.Animations;
    using UnityEngine.Playables;
    using UnityEngine.Timeline;
    using Object = UnityEngine.Object;


    public class AnimotiveImporterEditorWindow : EditorWindow
    {
        private string _basePath = "";
        public string Path = "/StreamingAssets/RootFolder";
        private string _fullPath = "";

        private bool _validPathChoosen = false;


        [MenuItem("Animotive/Importer")]
        public static void ShowWindow()
        {
            AnimotiveImporterEditorWindow window = GetWindow<AnimotiveImporterEditorWindow>("Example");
            window.Show();
        }


        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();


            GUILayout.Label("Root Folder Path", EditorStyles.boldLabel);
            Path = EditorGUILayout.TextField(Path);

            if (GUILayout.Button("Browse"))
            {
                _fullPath = EditorUtility.OpenFolderPanel("Choose Root Folder", "Assets/StreamingAssets", "");
                if (!string.IsNullOrEmpty(_fullPath) && _fullPath.Contains(Constants.STREAMING_ASSETS_FOLDER_NAME))
                {
                    string[] splitArray =
                        _fullPath.Split(new[] { Constants.STREAMING_ASSETS_FOLDER_NAME }, StringSplitOptions.None);

                    _basePath = splitArray[0];
                    string spliitted = splitArray[1];

                    if (string.IsNullOrEmpty(spliitted))
                    {
                        _validPathChoosen = false;
                        Debug.LogWarning("Can not load from StreamingAssets directly !");
                    }
                    else
                    {
                        Path = string.Concat(Constants.STREAMING_ASSETS_FOLDER_NAME, "/", splitArray[1]);
                        _validPathChoosen = true;

                        CreateScene("___scene_name_here___");
                        CreateGroups(new List<AnimotiveImporterGroupInfo>()
                        {
                            new AnimotiveImporterGroupInfo(),
                            new AnimotiveImporterGroupInfo(),
                            new AnimotiveImporterGroupInfo(),
                            new AnimotiveImporterGroupInfo(),
                            new AnimotiveImporterGroupInfo(),
                            new AnimotiveImporterGroupInfo(),
                            new AnimotiveImporterGroupInfo()
                        });
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

            if (GUILayout.Button("TESTTTTTTTTTTT"))
            {
                string path =
                    @"C:\\Users\\Ertan\\Desktop\\Unity\\UnityAnimotiveImporterPlugin\\Assets\\AnimotivePluginExampleStructure\\Example Data\\Animation\\Binary\\Frank Character Root_TransformClip_Take1";
                IT_CharacterTransformAnimationClip clip =
                    SerializationUtility.DeserializeValue<IT_CharacterTransformAnimationClip>(
                        File.ReadAllBytes(path),
                        DataFormat.Binary);
                for (int i = 0; i < clip.physicsKeyframesCurve0.Length; i++)
                {
                    Debug.Log(clip.physicsKeyframesCurve0[i]);
                }

            }
        }

        private void CreateScene(string sceneName)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            string dirName = System.IO.Path.Combine(_fullPath, Constants.UNITY_FILES_FOLDER_NAME);
            EditorSceneManager.SaveScene(scene,
                string.Concat(dirName, System.IO.Path.DirectorySeparatorChar, sceneName,
                    Constants.UNITY_SCENE_EXTENSION));
            AssetDatabase.Refresh();
        }

        private void CreateGroups(List<AnimotiveImporterGroupInfo> group)
        {
            for (int i = 0; i < group.Count; i++)
            {
                GameObject obj = new GameObject("<group name here>");
                PlayableDirector playableDirector = obj.AddComponent<PlayableDirector>();
                playableDirector.playableAsset = CreatePlayableAsset(obj);
            }
        }


        private PlayableAsset CreatePlayableAsset(GameObject obj)
        {
            string assetPath = string.Concat("Assets/", obj.GetInstanceID().ToString(), ".playable");
            TimelineAsset asset = CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(asset, assetPath);

            var track = asset.CreateTrack<TestTrack>();
            var clip = track.CreateClip<TestClip>();
            clip.displayName = "DISPLAY_NAME_HERE";


            AssetDatabase.Refresh();

            PlayableAsset playableAsset =
                AssetDatabase.LoadAssetAtPath(assetPath, typeof(PlayableAsset)) as PlayableAsset;


            return playableAsset;
        }
    }
}


