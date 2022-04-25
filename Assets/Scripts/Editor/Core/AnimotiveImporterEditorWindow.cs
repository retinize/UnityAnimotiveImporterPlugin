namespace AnimotiveImporterEditor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using OdinSerializer;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.Playables;
    using UnityEngine.Timeline;

    public class AnimotiveImporterEditorWindow : EditorWindow
    {
        public string Path = "/StreamingAssets/RootFolder";
        private string _basePath = "";
        private string _fullPath = "";

        private bool _validPathChoosen;

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
                    var splitArray =
                        _fullPath.Split(new[] { Constants.STREAMING_ASSETS_FOLDER_NAME }, StringSplitOptions.None);

                    _basePath = splitArray[0];
                    var spliitted = splitArray[1];

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
                        CreateGroups(new List<AnimotiveImporterGroupInfo>
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

            if (GUILayout.Button("Test Animation Clip")) HandleCharacterAnimationCreation();

            if (GUILayout.Button("Test Json Blendshape")) HandleBlendshapeAnimationCreation();
        }

        [MenuItem("Animotive/Importer")]
        public static void ShowWindow()
        {
            var window = GetWindow<AnimotiveImporterEditorWindow>("Example");
            window.Show();
        }

        private void CreateScene(string sceneName)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var dirName = System.IO.Path.Combine(_fullPath, Constants.UNITY_FILES_FOLDER_NAME);

            if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);

            EditorSceneManager.SaveScene(scene,
                string.Concat(dirName, System.IO.Path.DirectorySeparatorChar, sceneName,
                    Constants.UNITY_SCENE_EXTENSION));
            AssetDatabase.Refresh();
        }

        private void CreateGroups(List<AnimotiveImporterGroupInfo> group)
        {
            for (var i = 0; i < group.Count; i++)
            {
                var obj = new GameObject("<group name here>");
                var playableDirector = obj.AddComponent<PlayableDirector>();
                playableDirector.playableAsset = CreatePlayableAsset(obj);
            }
        }

        private PlayableAsset CreatePlayableAsset(GameObject obj)
        {
            var assetPath = string.Concat("Assets/", obj.GetInstanceID().ToString(), ".playable");
            var asset = CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(asset, assetPath);

            var track = asset.CreateTrack<TestTrack>();
            var clip = track.CreateClip<TestClip>();
            clip.displayName = "DISPLAY_NAME_HERE";


            AssetDatabase.Refresh();

            var playableAsset =
                AssetDatabase.LoadAssetAtPath(assetPath, typeof(PlayableAsset)) as PlayableAsset;


            return playableAsset;
        }

        private void HandleBlendshapeAnimationCreation()
        {
            var hardCodedJsonPath =
                @"C:\Users\Ertan-Laptop\Documents\UnityAnimotiveImporterPlugin\Assets\AnimotivePluginExampleStructure\Example Data\Animation\Json\Paddy.json";

            var reader = new StreamReader(hardCodedJsonPath);
            var jsonData = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();
            var clip = JsonUtility.FromJson<FacialAnimationExportWrapper>(jsonData);
            Debug.Log(clip);
        }

        private void HandleCharacterAnimationCreation()
        {
            var hardcodedAnimationDataPath =
                @"C:\Users\Ertan-Laptop\Documents\UnityAnimotiveImporterPlugin\Assets\AnimotivePluginExampleStructure\Example Data\Animation\Binary\\Frank Character Root_TransformClip_Take1";

            var clip =
                SerializationUtility.DeserializeValue<IT_CharacterTransformAnimationClip>(
                    File.ReadAllBytes(hardcodedAnimationDataPath),
                    DataFormat.Binary);

            var characterRoot = AssetDatabase.LoadAssetAtPath(
                "Assets\\AnimotivePluginExampleStructure\\SampleModels\\Frank_Export_Master.fbx",
                typeof(GameObject)) as GameObject;

            characterRoot = Instantiate(characterRoot);
            var animator = characterRoot.GetComponent<Animator>();


            var transformsByHumanBoneName =
                new Dictionary<string, Transform>(animator.avatar.humanDescription.human.Length);

            for (var i = 0; i < animator.avatar.humanDescription.human.Length; i++)
            {
                var bone = animator.avatar.humanDescription.human[i];
                var boneTransform = characterRoot.transform.FindChildRecursively(bone.boneName);
                transformsByHumanBoneName.Add(bone.humanName, boneTransform);
            }

            transformsByHumanBoneName.Add("LastBone", characterRoot.transform);

            // var indexInCurveOfKey = _keyIndex * numberOfBonesToAnimate + transformIndex;
            var itAvatar = new IT_Avatar(animator.avatar, transformsByHumanBoneName);

            InitializeItAvatar(clip, itAvatar);

            CreateAnimationClip(clip, transformsByHumanBoneName, characterRoot);

            AssetDatabase.Refresh();
        }

        private void InitializeItAvatar(IT_CharacterTransformAnimationClip clip, IT_Avatar avatar)
        {
            var physicsTransformsToCapture = new Transform[clip.humanoidBonesEnumThatAreUsed.Length];
            var humanBodyBones = Enum.GetValues(typeof(HumanBodyBones));
            for (var transformsToCaptureIndex = 0;
                 transformsToCaptureIndex < clip.humanoidBonesEnumThatAreUsed.Length;
                 transformsToCaptureIndex++)
            {
                var humanBodyBoneIndex = clip.humanoidBonesEnumThatAreUsed[transformsToCaptureIndex];
                var humanBodyBone = (HumanBodyBones)humanBodyBones.GetValue(humanBodyBoneIndex);
                var boneTransform = avatar.transformsByHumanBone[humanBodyBone];
                physicsTransformsToCapture[transformsToCaptureIndex] = boneTransform;
            }
        }

        private AnimationClip CreateAnimationClip(IT_CharacterTransformAnimationClip clip,
            Dictionary<string, Transform> transformsByHumanBoneName, GameObject characterRoot)
        {
            var animationClip = new AnimationClip();
            var positionCurveX = new AnimationCurve();
            var positionCurveY = new AnimationCurve();
            var positionCurveZ = new AnimationCurve();
            var rotationCurveX = new AnimationCurve();
            var rotationCurveY = new AnimationCurve();
            var rotationCurveZ = new AnimationCurve();
            var rotationCurveW = new AnimationCurve();

            for (var i = clip.initFrame; i < clip.lastFrame - 1; i++)
            {
                var transformIndex = 0;
                foreach (var pair in transformsByHumanBoneName)
                {
                    var indexInCurveOfKey = i * (transformsByHumanBoneName.Count - 1) + transformIndex;
                    var time = clip.fixedDeltaTime * i;

                    var relativePath = AnimationUtility.CalculateTransformPath(pair.Value, characterRoot.transform);
                    var localPositionX = new Keyframe(time, clip.physicsKeyframesCurve0[indexInCurveOfKey]);
                    var localPositionY = new Keyframe(time, clip.physicsKeyframesCurve1[indexInCurveOfKey]);
                    var localPositionZ = new Keyframe(time, clip.physicsKeyframesCurve2[indexInCurveOfKey]);
                    var localRotationX = new Keyframe(time, clip.physicsKeyframesCurve3[indexInCurveOfKey]);
                    var localRotationY = new Keyframe(time, clip.physicsKeyframesCurve4[indexInCurveOfKey]);
                    var localRotationZ = new Keyframe(time, clip.physicsKeyframesCurve5[indexInCurveOfKey]);
                    var localRotationW = new Keyframe(time, clip.physicsKeyframesCurve6[indexInCurveOfKey]);

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