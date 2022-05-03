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
    using UnityEngine.SceneManagement;
    using UnityEngine.Timeline;

    public class AnimotiveImporterEditorWindow : EditorWindow
    {
        private const string _fbxPath =
            @"Assets\AnimotivePluginExampleStructure\SampleModels\FrankBshp_Export_Master.fbx";

        private const string _binaryAnimPath =
            @"/Assets/AnimotivePluginExampleStructure/Example Data/Animation/Binary/Frank Character Root_TransformClip_Take1";

        private const string _blendShapeAnimCreatedPath =
            @"Assets/AnimotivePluginExampleStructure/UnityFiles/Animation/Blendshape/blenshapeAnim.anim";

        private const string _blendshapeJsonPath =
            @"\Assets\AnimotivePluginExampleStructure\Example Data\Animation\Json\Frank _FacialParametersAnimation_1_T00_01_00.json";

        private const string _transformAnimPath =
            @"Assets/AnimotivePluginExampleStructure/UnityFiles/Animation/Transform/transforms.anim";

        private const string _playablesCreationPath = @"Assets\AnimotivePluginExampleStructure\UnityFiles\Playables\";


        private void OnGUI()
        {
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

            if (GUILayout.Button("Test Animation Clip"))
            {
                Tuple<GameObject, Animator> fbxTuple = LoadFbx();

                Tuple<IT_CharacterTransformAnimationClip, Dictionary<string, Transform>> clipTuple =
                    PrepareAndGetAnimationData(fbxTuple);

                fbxTuple.Item1.transform.position = clipTuple.Item1.worldPositionHolder;
                fbxTuple.Item1.transform.rotation = clipTuple.Item1.worldRotationHolder;

                DeleteAssetIfExists(_transformAnimPath, typeof(AnimationClip));
                CreateTransformMovementsAnimationClip(clipTuple.Item1, clipTuple.Item2, fbxTuple.Item1);
            }

            if (GUILayout.Button("Test Json BlendShape"))
            {
                HandleBlendShapeAnimationCreation(LoadFbx());
            }
        }

        [MenuItem("Animotive/Importer")]
        public static void ShowWindow()
        {
            AnimotiveImporterEditorWindow window = GetWindow<AnimotiveImporterEditorWindow>("Example");
            window.Show();
        }

        private void CreateScene(string sceneName)
        {
            string hardcodedPath = @"Assets\AnimotivePluginExampleStructure\UnityFiles\Scenes\";

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);


            EditorSceneManager.SaveScene(scene,
                                         string.Concat(hardcodedPath, Path.DirectorySeparatorChar, sceneName,
                                                       Constants.UNITY_SCENE_EXTENSION));
            AssetDatabase.Refresh();
        }

        private void CreateGroups(List<AnimotiveImporterGroupInfo> group)
        {
            for (int i = 0; i < group.Count; i++)
            {
                GameObject obj = new GameObject(string.Format("<group name here : {0}>", i));
                obj.AddComponent<AudioSource>();
                obj.AddComponent<Animator>();
                PlayableDirector playableDirector = obj.AddComponent<PlayableDirector>();
                playableDirector.playableAsset = CreatePlayableAsset(obj, playableDirector);
            }
        }

        private PlayableAsset CreatePlayableAsset(GameObject obj, PlayableDirector playableDirector)
        {
            string assetPath = string.Concat(_playablesCreationPath, obj.GetInstanceID().ToString(), ".playable");
            TimelineAsset asset = CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(asset, assetPath);

            GroupTrack groupTrack = asset.CreateTrack<GroupTrack>();
            groupTrack.name = "GROUP_NAME_HERE";

            AnimatorTrack facialPerformanceAnimationTrack = asset.CreateTrack<AnimatorTrack>();
            facialPerformanceAnimationTrack.SetGroup(groupTrack);
            TimelineClip facialPerformanceClip = facialPerformanceAnimationTrack.CreateClip<AnimatorClip>();
            facialPerformanceClip.displayName = "FACIAL_ANIMATOR_CLIP_DISPLAY_NAME_HERE";
            playableDirector.SetGenericBinding(facialPerformanceAnimationTrack, obj);

            AnimatorTrack bodyPerformanceAnimationTrack = asset.CreateTrack<AnimatorTrack>();
            bodyPerformanceAnimationTrack.SetGroup(groupTrack);
            TimelineClip bodyPerformanceClip = bodyPerformanceAnimationTrack.CreateClip<AnimatorClip>();
            bodyPerformanceClip.displayName = "BODY_ANIMATOR_CLIP_DISPLAY_NAME_HERE";
            playableDirector.SetGenericBinding(bodyPerformanceAnimationTrack, obj);


            SoundTrack soundTrack = asset.CreateTrack<SoundTrack>();
            soundTrack.SetGroup(groupTrack);
            TimelineClip soundClip = soundTrack.CreateClip<SoundClip>();
            soundClip.displayName = "SOUND_CLIP_DISPLAY_NAME_HERE";
            playableDirector.SetGenericBinding(soundTrack, obj);

            AssetDatabase.Refresh();

            PlayableAsset playableAsset =
                AssetDatabase.LoadAssetAtPath(assetPath, typeof(PlayableAsset)) as PlayableAsset;


            return playableAsset;
        }

        private void HandleBlendShapeAnimationCreation(Tuple<GameObject, Animator> tuple)
        {
            string hardCodedJsonPath = string.Concat(Directory.GetCurrentDirectory(), _blendshapeJsonPath);

            StreamReader reader   = new StreamReader(hardCodedJsonPath);
            string       jsonData = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();
            FacialAnimationExportWrapper clip = JsonUtility.FromJson<FacialAnimationExportWrapper>(jsonData);

            CreateBlendShapeAnimationClip(clip, tuple);
        }

        private void CreateBlendShapeAnimationClip(FacialAnimationExportWrapper clip, Tuple<GameObject, Animator> tuple)
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

                    Transform           tr = tuple.Item1.transform.FindChildRecursively(skinnedMeshRendererName);
                    SkinnedMeshRenderer skinnedMeshRenderer = tr.gameObject.GetComponent<SkinnedMeshRenderer>();

                    string   blendshapeName  = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(blendShapeData.bsIndex);
                    float    blendshapeValue = blendShapeData.value;
                    Keyframe keyframe        = new Keyframe(time, blendshapeValue);

                    string relativePath = AnimationUtility.CalculateTransformPath(tr, tuple.Item1.transform);


                    AnimationCurve curve = blendshapeCurves[skinnedMeshRendererName];
                    curve.AddKey(keyframe);


                    string propertyName = string.Concat("blendShape.", blendshapeName);
                    animationClip.SetCurve(relativePath, typeof(SkinnedMeshRenderer), propertyName,
                                           curve);
                }
            }


            tuple.Item2.avatar = null;
            AssetDatabase.CreateAsset(animationClip, _blendShapeAnimCreatedPath);
            AssetDatabase.Refresh();
        }

        private Tuple<IT_CharacterTransformAnimationClip, Dictionary<string, Transform>>
            PrepareAndGetAnimationData(Tuple<GameObject, Animator> tuple)
        {
            string hardcodedAnimationDataPath = string.Concat(Directory.GetCurrentDirectory(), _binaryAnimPath);


            IT_CharacterTransformAnimationClip clip =
                SerializationUtility.DeserializeValue<IT_CharacterTransformAnimationClip>(
                 File.ReadAllBytes(hardcodedAnimationDataPath),
                 DataFormat.Binary);


            Animator animator = tuple.Item2;


            Dictionary<string, Transform> transformsByHumanBoneName =
                new Dictionary<string, Transform>(animator.avatar.humanDescription.human.Length);

            for (int i = 0; i < animator.avatar.humanDescription.human.Length; i++)
            {
                HumanBone bone          = animator.avatar.humanDescription.human[i];
                Transform boneTransform = tuple.Item1.transform.FindChildRecursively(bone.boneName);
                transformsByHumanBoneName.Add(bone.humanName, boneTransform);
            }

            transformsByHumanBoneName.Add("LastBone", tuple.Item1.transform);

            IT_Avatar itAvatar = new IT_Avatar(animator.avatar, transformsByHumanBoneName);

            transformsByHumanBoneName = InitializeItAvatar(clip, itAvatar);

            animator.avatar = null;


            AssetDatabase.Refresh();
            return new
                Tuple<IT_CharacterTransformAnimationClip, Dictionary<string, Transform>>(clip,
                 transformsByHumanBoneName);
        }

        private Dictionary<string, Transform> InitializeItAvatar(IT_CharacterTransformAnimationClip clip,
                                                                 IT_Avatar                          avatar)
        {
            Transform[] physicsTransformsToCapture = new Transform[clip.humanoidBonesEnumThatAreUsed.Length];
            Array humanBodyBones = Enum.GetValues(typeof(HumanBodyBones));
            Dictionary<string, Transform> transformsByHumanBoneName = new Dictionary<string, Transform>(55);

            for (int transformsToCaptureIndex = 0;
                 transformsToCaptureIndex < clip.humanoidBonesEnumThatAreUsed.Length;
                 transformsToCaptureIndex++)
            {
                int            humanBodyBoneIndex = clip.humanoidBonesEnumThatAreUsed[transformsToCaptureIndex];
                HumanBodyBones humanBodyBone      = (HumanBodyBones)humanBodyBones.GetValue(humanBodyBoneIndex);
                Transform      boneTransform      = avatar.transformsByHumanBone[humanBodyBone];
                physicsTransformsToCapture[transformsToCaptureIndex] = boneTransform;
                transformsByHumanBoneName.Add(humanBodyBone.ToString(), boneTransform);
            }

            return transformsByHumanBoneName;
        }

        private AnimationClip CreateTransformMovementsAnimationClip(IT_CharacterTransformAnimationClip clip,
                                                                    Dictionary<string, Transform>
                                                                        transformsByHumanBoneName,
                                                                    GameObject characterRoot)
        {
            AnimationClip animationClip = new AnimationClip();

            AnimationCurve positionCurveX = new AnimationCurve();
            AnimationCurve positionCurveY = new AnimationCurve();
            AnimationCurve positionCurveZ = new AnimationCurve();
            AnimationCurve rotationCurveX = new AnimationCurve();
            AnimationCurve rotationCurveY = new AnimationCurve();
            AnimationCurve rotationCurveZ = new AnimationCurve();
            AnimationCurve rotationCurveW = new AnimationCurve();


            Dictionary<string, List<List<Keyframe>>> pathAndKeyframesDictionary =
                new Dictionary<string, List<List<Keyframe>>>(55);


            for (int frame = clip.initFrame; frame <= clip.lastFrame; frame++)
            {
                int   transformIndex = 0;
                float time           = clip.fixedDeltaTime * frame;


                foreach (KeyValuePair<string, Transform> pair in transformsByHumanBoneName)
                {
                    int indexInCurveOfKey = frame * transformsByHumanBoneName.Count + transformIndex;

                    string relativePath = AnimationUtility.CalculateTransformPath(pair.Value, characterRoot.transform);

                    if (!pathAndKeyframesDictionary.ContainsKey(relativePath))
                    {
                        pathAndKeyframesDictionary.Add(relativePath, new List<List<Keyframe>>());
                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>()); //0
                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>()); //1
                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>()); //2
                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>()); //3
                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>()); //4
                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>()); //5
                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>()); //6
                    }


                    Keyframe localPositionX = new Keyframe(time, clip.physicsKeyframesCurve0[indexInCurveOfKey]);
                    Keyframe localPositionY = new Keyframe(time, clip.physicsKeyframesCurve1[indexInCurveOfKey]);
                    Keyframe localPositionZ = new Keyframe(time, clip.physicsKeyframesCurve2[indexInCurveOfKey]);
                    Keyframe localRotationX = new Keyframe(time, clip.physicsKeyframesCurve3[indexInCurveOfKey]);
                    Keyframe localRotationY = new Keyframe(time, clip.physicsKeyframesCurve4[indexInCurveOfKey]);
                    Keyframe localRotationZ = new Keyframe(time, clip.physicsKeyframesCurve5[indexInCurveOfKey]);
                    Keyframe localRotationW = new Keyframe(time, clip.physicsKeyframesCurve6[indexInCurveOfKey]);

                    pathAndKeyframesDictionary[relativePath][0].Add(localPositionX);
                    pathAndKeyframesDictionary[relativePath][1].Add(localPositionY);
                    pathAndKeyframesDictionary[relativePath][2].Add(localPositionZ);

                    pathAndKeyframesDictionary[relativePath][3].Add(localRotationX);
                    pathAndKeyframesDictionary[relativePath][4].Add(localRotationY);
                    pathAndKeyframesDictionary[relativePath][5].Add(localRotationZ);
                    pathAndKeyframesDictionary[relativePath][6].Add(localRotationW);

                    transformIndex++;
                }
            }


            foreach (KeyValuePair<string, List<List<Keyframe>>> keyValuePair in pathAndKeyframesDictionary)
            {
                string relativePath = keyValuePair.Key;

                positionCurveX = new AnimationCurve(keyValuePair.Value[0].ToArray());
                positionCurveY = new AnimationCurve(keyValuePair.Value[1].ToArray());
                positionCurveZ = new AnimationCurve(keyValuePair.Value[2].ToArray());

                rotationCurveX = new AnimationCurve(keyValuePair.Value[3].ToArray());
                rotationCurveY = new AnimationCurve(keyValuePair.Value[4].ToArray());
                rotationCurveZ = new AnimationCurve(keyValuePair.Value[5].ToArray());
                rotationCurveW = new AnimationCurve(keyValuePair.Value[6].ToArray());

                animationClip.SetCurve(relativePath, typeof(Transform), "localPosition.x", positionCurveX);
                animationClip.SetCurve(relativePath, typeof(Transform), "localPosition.y", positionCurveY);
                animationClip.SetCurve(relativePath, typeof(Transform), "localPosition.z", positionCurveZ);

                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.x", rotationCurveX);
                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.y", rotationCurveY);
                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.z", rotationCurveZ);
                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.w", rotationCurveW);
                animationClip.EnsureQuaternionContinuity();
            }


            AssetDatabase.CreateAsset(animationClip, _transformAnimPath);
            AssetDatabase.Refresh();
            return animationClip;
        }

        private Tuple<GameObject, Animator> LoadFbx()
        {
            GameObject characterRoot = AssetDatabase.LoadAssetAtPath(
                                                                     _fbxPath,
                                                                     typeof(GameObject)) as GameObject;

            characterRoot = Instantiate(characterRoot);

            Animator animator = characterRoot.GetComponent<Animator>();

            return new Tuple<GameObject, Animator>(characterRoot, animator);
        }

        private void DeleteAssetIfExists(string path, Type type)
        {
            if (AssetDatabase.LoadAssetAtPath(path, type) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }
        }
    }
}