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
        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();


            GUILayout.Label("Root Folder Path", EditorStyles.boldLabel);

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Create scene and playables"))
            {
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
            string hardcodedPath = @"Assets\AnimotivePluginExampleStructure\UnityFiles\Scenes\";

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);


            EditorSceneManager.SaveScene(scene,
                                         string.Concat(hardcodedPath, Path.DirectorySeparatorChar, sceneName,
                                                       Constants.UNITY_SCENE_EXTENSION));
            AssetDatabase.Refresh();
        }

        private void CreateGroups(List<AnimotiveImporterGroupInfo> group)
        {
            for (var i = 0; i < group.Count; i++)
            {
                var obj              = new GameObject("<group name here>");
                var playableDirector = obj.AddComponent<PlayableDirector>();
                playableDirector.playableAsset = CreatePlayableAsset(obj);
            }
        }

        private PlayableAsset CreatePlayableAsset(GameObject obj)
        {
            string assetPath = string.Concat(@"Assets\AnimotivePluginExampleStructure\UnityFiles\Playables\",
                                          obj.GetInstanceID().ToString(), ".playable");
            TimelineAsset asset = CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(asset, assetPath);

            TestTrack track = asset.CreateTrack<TestTrack>();
            TimelineClip       clip  = track.CreateClip<TestClip>();
            
            clip.displayName = "DISPLAY_NAME_HERE";


            AssetDatabase.Refresh();

            PlayableAsset playableAsset =
                AssetDatabase.LoadAssetAtPath(assetPath, typeof(PlayableAsset)) as PlayableAsset;
            

            return playableAsset;
        }

        private void HandleBlendshapeAnimationCreation()
        {
            var hardCodedJsonPath =
                @"C:\Users\Ertan\Desktop\Unity\UnityAnimotiveImporterPlugin\Assets\AnimotivePluginExampleStructure\Example Data\Animation\Json\Paddy.json";

            var reader   = new StreamReader(hardCodedJsonPath);
            var jsonData = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();
            FacialAnimationExportWrapper clip = JsonUtility.FromJson<FacialAnimationExportWrapper>(jsonData);

            CreateBlendshapeAnimationClip(clip);
        }

        private void CreateBlendshapeAnimationClip(FacialAnimationExportWrapper clip)
        {
            AnimationClip animationClip = new AnimationClip();

            Dictionary<string, AnimationCurve> blendshapeCurves =
                new Dictionary<string, AnimationCurve>(clip.characterGeos.Count);

            for (int i = 0; i < clip.characterGeos.Count; i++)
            {
                blendshapeCurves.Add(clip.characterGeos[i].name, new AnimationCurve());
            }

            for (int i = 0; i < clip.facialAnimationFrames.Count; i++)
            {
                float time = i * clip.fixedDeltaTimeBetweenKeyFrames;
                for (int j = 0; j < clip.facialAnimationFrames[i].blendShapesUsed.Count; j++)
                {
                    IT_SpecificCharacterBlendShapeData
                        blendShapeData = clip.facialAnimationFrames[i].blendShapesUsed[j];

                    CharacterGeoDescriptor characterGeoDescriptor = clip.characterGeos[blendShapeData.geo];

                    string skinnedMeshRendererName = characterGeoDescriptor.name;
                    string blendshapeName          = characterGeoDescriptor.blendShapeNames[blendShapeData.bsIndex];
                    float  blendshapeValue         = blendShapeData.value;

                    Keyframe keyframe = new Keyframe(time, blendshapeValue);

                    AnimationCurve curve = blendshapeCurves[skinnedMeshRendererName];
                    curve.AddKey(keyframe);

                    string propertyName = string.Concat("Shape.shapes.", blendshapeName);
                    animationClip.SetCurve(skinnedMeshRendererName, typeof(SkinnedMeshRenderer), propertyName, curve);
                }
            }

            AssetDatabase.CreateAsset(animationClip,
                                      "Assets/AnimotivePluginExampleStructure/UnityFiles/Animation/Blendshape/blenshapeAnim.anim");
            AssetDatabase.Refresh();
        }

        private void HandleCharacterAnimationCreation()
        {

            string hardcodedAnimationDataPath = string.Concat(Directory.GetCurrentDirectory(),
                                                             @"/Assets/AnimotivePluginExampleStructure/Example Data/Animation/Binary/Frank Character Root_TransformClip_Take1");

            
            
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
                var bone          = animator.avatar.humanDescription.human[i];
                var boneTransform = characterRoot.transform.FindChildRecursively(bone.boneName);
                transformsByHumanBoneName.Add(bone.humanName, boneTransform);
            }

            transformsByHumanBoneName.Add("LastBone", characterRoot.transform);

            // var indexInCurveOfKey = _keyIndex * numberOfBonesToAnimate + transformIndex;
            var itAvatar = new IT_Avatar(animator.avatar, transformsByHumanBoneName);

            InitializeItAvatar(clip, itAvatar);

            CreateTransformMovementsAnimationClip(clip, transformsByHumanBoneName, characterRoot);

            AssetDatabase.Refresh();
        }

        private void InitializeItAvatar(IT_CharacterTransformAnimationClip clip, IT_Avatar avatar)
        {
            var physicsTransformsToCapture = new Transform[clip.humanoidBonesEnumThatAreUsed.Length];
            var humanBodyBones             = Enum.GetValues(typeof(HumanBodyBones));
            for (var transformsToCaptureIndex = 0;
                transformsToCaptureIndex < clip.humanoidBonesEnumThatAreUsed.Length;
                transformsToCaptureIndex++)
            {
                var humanBodyBoneIndex = clip.humanoidBonesEnumThatAreUsed[transformsToCaptureIndex];
                var humanBodyBone      = (HumanBodyBones)humanBodyBones.GetValue(humanBodyBoneIndex);
                var boneTransform      = avatar.transformsByHumanBone[humanBodyBone];
                physicsTransformsToCapture[transformsToCaptureIndex] = boneTransform;
            }
        }

        private AnimationClip CreateTransformMovementsAnimationClip(IT_CharacterTransformAnimationClip clip,
                                                                    Dictionary<string, Transform>
                                                                        transformsByHumanBoneName,
                                                                    GameObject characterRoot)
        {
            AnimationClip  animationClip  = new AnimationClip();
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
                    int indexInCurveOfKey = i * (transformsByHumanBoneName.Count - 1) + transformIndex;
                    var time              = clip.fixedDeltaTime * i;

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

            AssetDatabase.CreateAsset(animationClip, "Assets/AnimotivePluginExampleStructure/UnityFiles/Animation/Transform/transforms.anim");
            AssetDatabase.Refresh();
            return animationClip;
        }
    }
}