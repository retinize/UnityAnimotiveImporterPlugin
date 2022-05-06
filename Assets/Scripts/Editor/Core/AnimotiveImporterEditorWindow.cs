namespace AnimotiveImporterEditor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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

        private const string _blendshapeJsonPath =
            @"\Assets\AnimotivePluginExampleStructure\Example Data\Animation\Json\Frank _FacialParametersAnimation_1_T00_01_00.json";

        private const string _binaryAnimPath =
            @"/Assets/AnimotivePluginExampleStructure/Example Data/Animation/Binary/FrankErtanTest Character Root_TransformClip_Take1";

        private const string _blendShapeAnimCreatedPath =
            @"Assets/AnimotivePluginExampleStructure/UnityFiles/Animation/Blendshape/blenshapeAnim.anim";

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

                Tuple<IT_CharacterTransformAnimationClip, Dictionary<HumanBodyBones, Transform>> clipTuple =
                    PrepareAndGetAnimationData(fbxTuple);


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

        private Tuple<IT_CharacterTransformAnimationClip, Dictionary<HumanBodyBones, Transform>>
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


            animator.avatar = null;


            AssetDatabase.Refresh();
            return new
                Tuple<IT_CharacterTransformAnimationClip, Dictionary<HumanBodyBones, Transform>>(clip,
                 InitializeItAvatar(clip, itAvatar));
        }

        private Dictionary<HumanBodyBones, Transform> InitializeItAvatar(IT_CharacterTransformAnimationClip clip,
                                                                         IT_Avatar                          avatar)
        {
            Transform[] physicsTransformsToCapture = new Transform[clip.humanoidBonesEnumThatAreUsed.Length];
            Array       humanBodyBones             = Enum.GetValues(typeof(HumanBodyBones));
            Dictionary<HumanBodyBones, Transform> transformsByHumanBoneName =
                new Dictionary<HumanBodyBones, Transform>(55);

            for (int transformsToCaptureIndex = 0;
                 transformsToCaptureIndex < clip.humanoidBonesEnumThatAreUsed.Length;
                 transformsToCaptureIndex++)
            {
                int            humanBodyBoneIndex = clip.humanoidBonesEnumThatAreUsed[transformsToCaptureIndex];
                HumanBodyBones humanBodyBone      = (HumanBodyBones)humanBodyBones.GetValue(humanBodyBoneIndex);
                Transform      boneTransform      = avatar.transformsByHumanBone[humanBodyBone];
                physicsTransformsToCapture[transformsToCaptureIndex] = boneTransform;
                transformsByHumanBoneName.Add(humanBodyBone, boneTransform);
            }

            return transformsByHumanBoneName;
        }

        private void CreateTransformMovementsAnimationClip(IT_CharacterTransformAnimationClip clip,
                                                           Dictionary<HumanBodyBones, Transform>
                                                               transformsByHumanBoneName,
                                                           GameObject characterRoot)
        {
            Dictionary<Transform, HumanBodyBones> humanoidBoneByTransform =
                new Dictionary<Transform, HumanBodyBones>(55);

            foreach (KeyValuePair<HumanBodyBones, Transform> pair in transformsByHumanBoneName)
            {
                humanoidBoneByTransform.Add(pair.Value, pair.Key);
            }

            AnimationClip animationClip = new AnimationClip();


            Dictionary<string, List<List<Keyframe>>> pathAndKeyframesDictionary =
                new Dictionary<string, List<List<Keyframe>>>(55);


            Dictionary<HumanBodyBones, List<Quaternion>> localQuaternionsByFrame =
                GetLocalQuaternionsFromAnimFile(clip, transformsByHumanBoneName);

            Dictionary<HumanBodyBones, List<Quaternion>> globalQuaternionsByFrame =
                GetGlobalRotationsFromAnimFile(transformsByHumanBoneName, humanoidBoneByTransform,
                                               localQuaternionsByFrame,   clip);

//HARDCODE !
            string path = string.Concat(Directory.GetCurrentDirectory(),
                                        @"\Assets\AnimotivePluginExampleStructure\Example Data\Animation\Poses");

            string animotiveTPoseText = File.ReadAllText(string.Concat(path, "\\AnimotiveTPoseFrank.json"));
            string editorTPoseText    = File.ReadAllText(string.Concat(path, "\\EditorTPoseFrank.json"));


            TransformInfoList animotiveTPoseTransformInfoList =
                JsonUtility.FromJson<TransformInfoList>(animotiveTPoseText);
            TransformInfoList editorTPoseTransformInfoList = JsonUtility.FromJson<TransformInfoList>(editorTPoseText);


            for (int frame = clip.initFrame; frame <= clip.lastFrame; frame++)
            {
                int   transformIndex = 0;
                float time           = clip.fixedDeltaTime * frame;


                foreach (KeyValuePair<HumanBodyBones, Transform> pair in transformsByHumanBoneName)
                {
                    if (pair.Key == HumanBodyBones.LastBone)
                    {
                        continue;
                    }

                    // int indexInCurveOfKey = frame * transformsByHumanBoneName.Count + transformIndex;

                    string relativePath = AnimationUtility.CalculateTransformPath(pair.Value, characterRoot.transform);

                    if (!pathAndKeyframesDictionary.ContainsKey(relativePath))
                    {
                        pathAndKeyframesDictionary.Add(relativePath, new List<List<Keyframe>>(55));

                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>(clip.lastFrame + 1)); //0
                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>(clip.lastFrame + 1)); //1
                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>(clip.lastFrame + 1)); //2
                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>(clip.lastFrame + 1)); //3
                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>(clip.lastFrame + 1)); //4
                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>(clip.lastFrame + 1)); //5
                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>(clip.lastFrame + 1)); //6
                    }


                    Quaternion boneGlobalRotationThisFrameFromAnimFile = globalQuaternionsByFrame[pair.Key][frame];
                    // Quaternion boneLocalRotationThisFrameFromAnimFile  = localQuaternionsByFrame[pair.Key][frame];

                    Quaternion editorTPoseRotationForThisBone =
                        editorTPoseTransformInfoList.TransformsByStrings.Where(a => a.Name == pair.Key).ToList()[0]
                                                    .GlobalRotation;

                    Quaternion animotiveTPoseRotationForThisBone =
                        animotiveTPoseTransformInfoList.TransformsByStrings.Where(a => a.Name == pair.Key).ToList()[0]
                                                       .GlobalRotation;

                    Quaternion inverseAnimotiveTPoseRotationForThisBone =
                        Quaternion.Inverse(animotiveTPoseRotationForThisBone);


                    Quaternion globalRotationOfParentBoneThisFrameFromAnimFile = Quaternion.identity;
                    Transform  parent                                          = pair.Value.parent;
                    if (parent != null)
                    {
                        while (!humanoidBoneByTransform.ContainsKey(parent))
                        {
                            parent = parent.parent;
                        }

                        HumanBodyBones bone = humanoidBoneByTransform[parent];
                        globalRotationOfParentBoneThisFrameFromAnimFile =
                            globalQuaternionsByFrame[bone][frame];
                    }


                    Quaternion boneRotation = inverseAnimotiveTPoseRotationForThisBone *
                                              boneGlobalRotationThisFrameFromAnimFile  * editorTPoseRotationForThisBone;

                    pair.Value.rotation = boneRotation;

                    Quaternion inversedParentBoneRotation = Quaternion.Inverse(pair.Value.parent == null
                                                                                   ? Quaternion.identity
                                                                                   : pair.Value.parent.rotation);

                    Quaternion finalLocalRotation = inversedParentBoneRotation
                                                    *
                                                    boneRotation;


                    Keyframe localRotationX = new Keyframe(time, finalLocalRotation.x);
                    Keyframe localRotationY = new Keyframe(time, finalLocalRotation.y);
                    Keyframe localRotationZ = new Keyframe(time, finalLocalRotation.z);
                    Keyframe localRotationW = new Keyframe(time, finalLocalRotation.w);

                    // pathAndKeyframesDictionary[relativePath][0].Add(localPositionX);
                    // pathAndKeyframesDictionary[relativePath][1].Add(localPositionY);
                    // pathAndKeyframesDictionary[relativePath][2].Add(localPositionZ);

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

                // AnimationCurve positionCurveX = new AnimationCurve(keyValuePair.Value[0].ToArray());
                // AnimationCurve positionCurveY = new AnimationCurve(keyValuePair.Value[1].ToArray());
                // AnimationCurve positionCurveZ = new AnimationCurve(keyValuePair.Value[2].ToArray());

                AnimationCurve rotationCurveX = new AnimationCurve(keyValuePair.Value[3].ToArray());
                AnimationCurve rotationCurveY = new AnimationCurve(keyValuePair.Value[4].ToArray());
                AnimationCurve rotationCurveZ = new AnimationCurve(keyValuePair.Value[5].ToArray());
                AnimationCurve rotationCurveW = new AnimationCurve(keyValuePair.Value[6].ToArray());

                // animationClip.SetCurve(relativePath, typeof(Transform), "localPosition.x", positionCurveX);
                // animationClip.SetCurve(relativePath, typeof(Transform), "localPosition.y", positionCurveY);
                // animationClip.SetCurve(relativePath, typeof(Transform), "localPosition.z", positionCurveZ);

                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.x", rotationCurveX);
                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.y", rotationCurveY);
                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.z", rotationCurveZ);
                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.w", rotationCurveW);
                animationClip.EnsureQuaternionContinuity();
            }


            AssetDatabase.CreateAsset(animationClip, _transformAnimPath);
            AssetDatabase.Refresh();
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


        private Dictionary<HumanBodyBones, List<Quaternion>> GetGlobalRotationsFromAnimFile(
            Dictionary<HumanBodyBones, Transform> transformByHumanoidBone,
            Dictionary<Transform, HumanBodyBones> humanoidBoneByTransform,
            Dictionary<HumanBodyBones, List<Quaternion>>
                localQuaternionsByFrame,
            IT_CharacterTransformAnimationClip clip)
        {
            Dictionary<HumanBodyBones, List<Quaternion>> globalQuaternionsByFrame =
                new Dictionary<HumanBodyBones, List<Quaternion>>(55);

            for (int frame = clip.initFrame; frame <= clip.lastFrame; frame++)
            {
                foreach (HumanBodyBones humanBone in Enum.GetValues(typeof(HumanBodyBones)))
                {
                    if (!transformByHumanoidBone.ContainsKey(humanBone))
                    {
                        continue;
                    }

                    Transform  boneTransform            = transformByHumanoidBone[humanBone];
                    Quaternion globalRotationOfThisBone = Quaternion.identity;

                    while (boneTransform != null)
                    {
                        if (humanoidBoneByTransform.ContainsKey(boneTransform))
                        {
                            HumanBodyBones bone = humanoidBoneByTransform[boneTransform];

                            Quaternion localRotationInAnimFile =
                                localQuaternionsByFrame[bone][frame];
                            globalRotationOfThisBone = localRotationInAnimFile * globalRotationOfThisBone;
                        }

                        boneTransform = boneTransform.parent;
                    }

                    if (!globalQuaternionsByFrame.ContainsKey(humanBone))
                    {
                        globalQuaternionsByFrame.Add(humanBone, new List<Quaternion>(clip.lastFrame + 1));
                    }

                    globalQuaternionsByFrame[humanBone].Add(globalRotationOfThisBone);
                }
            }

            return globalQuaternionsByFrame;
        }

        private Dictionary<HumanBodyBones, List<Quaternion>> GetLocalQuaternionsFromAnimFile(
            IT_CharacterTransformAnimationClip    clip,
            Dictionary<HumanBodyBones, Transform> transformsByHumanBoneName)
        {
            Dictionary<HumanBodyBones, List<Quaternion>> localQuaternionsByFrame =
                new Dictionary<HumanBodyBones, List<Quaternion>>(55);

            for (int frame = clip.initFrame; frame <= clip.lastFrame; frame++)
            {
                int   transformIndex = 0;
                float time           = clip.fixedDeltaTime * frame;


                foreach (KeyValuePair<HumanBodyBones, Transform> pair in transformsByHumanBoneName)
                {
                    int indexInCurveOfKey = frame * transformsByHumanBoneName.Count + transformIndex;


                    Quaternion frameRotation = new Quaternion(clip.physicsKeyframesCurve3[indexInCurveOfKey],
                                                              clip.physicsKeyframesCurve4[indexInCurveOfKey],
                                                              clip.physicsKeyframesCurve5[indexInCurveOfKey],
                                                              clip.physicsKeyframesCurve6[indexInCurveOfKey]);

                    if (!localQuaternionsByFrame.ContainsKey(pair.Key))
                    {
                        localQuaternionsByFrame.Add(pair.Key, new List<Quaternion>(clip.lastFrame + 1));
                    }

                    localQuaternionsByFrame[pair.Key].Add(frameRotation);
                    transformIndex++;
                }
            }

            return localQuaternionsByFrame;
        }
    }
}