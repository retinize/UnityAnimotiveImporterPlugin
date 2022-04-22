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
    using UnityEditor.Animations;
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
                    @"C:\Users\Ertan-Laptop\Documents\UnityAnimotiveImporterPlugin\Assets\AnimotivePluginExampleStructure\Example Data\Animation\Binary\\Frank Character Root_TransformClip_Take1";
                IT_CharacterTransformAnimationClip clip =
                    SerializationUtility.DeserializeValue<IT_CharacterTransformAnimationClip>(
                        File.ReadAllBytes(path),
                        DataFormat.Binary);

                GameObject characterRoot = AssetDatabase.LoadAssetAtPath(
                    "Assets\\AnimotivePluginExampleStructure\\SampleModels\\Frank_Export_Master.fbx",
                    typeof(GameObject)) as GameObject;

                characterRoot = Instantiate(characterRoot);
                Animator animator = characterRoot.GetComponent<Animator>();


                Dictionary<string, Transform> transformsByHumanBoneName =
                    new Dictionary<string, Transform>(animator.avatar.humanDescription.human.Length);

                for (int i = 0; i < animator.avatar.humanDescription.human.Length; i++)
                {
                    HumanBone bone = animator.avatar.humanDescription.human[i];
                    Transform boneTransform = characterRoot.transform.FindChildRecursively(bone.boneName);
                    transformsByHumanBoneName.Add(bone.humanName, boneTransform);
                }

                transformsByHumanBoneName.Add("LastBone", characterRoot.transform);

                // var indexInCurveOfKey = _keyIndex * numberOfBonesToAnimate + transformIndex;
                IT_Avatar itAvatar = new IT_Avatar(animator.avatar, transformsByHumanBoneName);

                Initialize(clip, itAvatar);


                AnimationClip animationClip = CreateAnimationClip(clip, transformsByHumanBoneName, characterRoot);

                // AnimatorController controller =
                //     AnimatorController.CreateAnimatorControllerAtPathWithClip("Assets/animController.controller", animationClip);
                // animator.runtimeAnimatorController = controller;
                AssetDatabase.Refresh();
            }
        }


        private void CreateScene(string sceneName)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            string dirName = System.IO.Path.Combine(_fullPath, Constants.UNITY_FILES_FOLDER_NAME);

            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

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

        private void Initialize(IT_CharacterTransformAnimationClip clip, IT_Avatar avatar)
        {
            Transform[] physicsTransformsToCapture = new Transform[clip.humanoidBonesEnumThatAreUsed.Length];
            var humanBodyBones = Enum.GetValues(typeof(HumanBodyBones));
            for (var transformsToCaptureIndex = 0;
                 transformsToCaptureIndex < clip.humanoidBonesEnumThatAreUsed.Length;
                 transformsToCaptureIndex++)
            {
                Int32 humanBodyBoneIndex = clip.humanoidBonesEnumThatAreUsed[transformsToCaptureIndex];
                HumanBodyBones humanBodyBone = (HumanBodyBones)humanBodyBones.GetValue(humanBodyBoneIndex);
                Transform boneTransform = avatar.transformsByHumanBone[humanBodyBone];
                physicsTransformsToCapture[transformsToCaptureIndex] = boneTransform;
            }
        }

        private AnimationClip CreateAnimationClip(IT_CharacterTransformAnimationClip clip,
            Dictionary<string, Transform> transformsByHumanBoneName, GameObject characterRoot)
        {
            AnimationClip animationClip = new AnimationClip();
            AnimationCurve positionCurveX = new AnimationCurve();
            AnimationCurve positionCurveY = new AnimationCurve();
            AnimationCurve positionCurveZ = new AnimationCurve();
            AnimationCurve rotationCurveX = new AnimationCurve();
            AnimationCurve rotationCurveY = new AnimationCurve();
            AnimationCurve rotationCurveZ = new AnimationCurve();
            AnimationCurve rotationCurveW = new AnimationCurve();

            for (int i = clip.initFrame; i < clip.lastFrame - 1; i++)
            {
                int transformIndex = 0;
                foreach (KeyValuePair<string, Transform> pair in transformsByHumanBoneName)
                {
                    var indexInCurveOfKey = i * (transformsByHumanBoneName.Count - 1) + transformIndex;
                    float time = clip.fixedDeltaTime * i;

                    string relativePath = AnimationUtility.CalculateTransformPath(pair.Value, characterRoot.transform);
                    Keyframe localPositionX = new Keyframe(time, clip.physicsKeyframesCurve0[indexInCurveOfKey]);
                    Keyframe localPositionY = new Keyframe(time, clip.physicsKeyframesCurve1[indexInCurveOfKey]);
                    Keyframe localPositionZ = new Keyframe(time, clip.physicsKeyframesCurve2[indexInCurveOfKey]);
                    Keyframe localRotationX = new Keyframe(time, clip.physicsKeyframesCurve3[indexInCurveOfKey]);
                    Keyframe localRotationY = new Keyframe(time, clip.physicsKeyframesCurve4[indexInCurveOfKey]);
                    Keyframe localRotationZ = new Keyframe(time, clip.physicsKeyframesCurve5[indexInCurveOfKey]);
                    Keyframe localRotationW = new Keyframe(time, clip.physicsKeyframesCurve6[indexInCurveOfKey]);

                    positionCurveX.AddKey(localPositionX);
                    positionCurveY.AddKey(localPositionY);
                    positionCurveZ.AddKey(localPositionZ);
                    rotationCurveX.AddKey(localRotationX);
                    rotationCurveY.AddKey(localRotationY);
                    rotationCurveZ.AddKey(localRotationZ);
                    rotationCurveW.AddKey(localRotationW);

                    animationClip.SetCurve(relativePath, typeof(Transform), "localPosition.x", positionCurveX);
                    animationClip.SetCurve(relativePath, typeof(Transform), "localPosition.y", positionCurveY);
                    animationClip.SetCurve(relativePath, typeof(Transform), "localPosition.z", positionCurveZ);
                    animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.x", rotationCurveX);
                    animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.y", rotationCurveY);
                    animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.z", rotationCurveZ);
                    animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.w", rotationCurveW);

                    transformIndex++;
                }
            }

            AssetDatabase.CreateAsset(animationClip, "Assets/test.anim");
            AssetDatabase.Refresh();
            return animationClip;
        }
    }
}