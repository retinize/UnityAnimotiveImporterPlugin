namespace Retinize.Editor.AnimotiveImporter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AnimotiveImporterDLL;
    using OdinSerializer;
    using UnityEditor;
    using UnityEditor.Animations;
    using UnityEngine;

    
    public struct DictionaryTuple
    {
        public Dictionary<HumanBodyBones, Transform> TransformsByHumanBodyBones{ get;  }
        public Dictionary<Transform, HumanBodyBones> HumanBodyBonesByTransform { get;  }

        public DictionaryTuple(Dictionary<HumanBodyBones, Transform> transformsByHumanBodyBones, Dictionary<Transform, HumanBodyBones> humanBodyBonesByTransform)
        {
            TransformsByHumanBodyBones = transformsByHumanBodyBones;
            HumanBodyBonesByTransform = humanBodyBonesByTransform;
        }

        public DictionaryTuple(int a=0)
        {
            TransformsByHumanBodyBones = new Dictionary<HumanBodyBones, Transform>();
            HumanBodyBonesByTransform = new Dictionary<Transform, HumanBodyBones>();
        }
    }

    public struct ClipByDictionaryTuple
    {
        public IT_CharacterTransformAnimationClip Clip { get; private set; }
        public DictionaryTuple DictTuple { get; private set; }

        public ClipByDictionaryTuple(IT_CharacterTransformAnimationClip clip, DictionaryTuple dictTuple)
        {
            Clip = clip;
            DictTuple = dictTuple;
        }
    }
    public static class IT_TransformAnimationClipEditor
    {
        
      
        private static string _AnimationClipDataPath = "";

        public static string bodyAnimationPath = "";
        public static string bodyAnimatorPath = "";
        public static string bodyAnimationName = "";

        #region Dictionary Operations

        /// <summary>
        ///     Creates and returns two dictionary where 'HumanBodyBones' and 'Transform' types are key/value and vice versa.
        /// </summary>
        /// <param name="animator">Animator of the character</param>
        /// <param name="characterRoot">Root GameObject of the character</param>
        /// <returns></returns>
        private static DictionaryTuple GetBoneTransformDictionaries(Animator animator, GameObject characterRoot)
        {
            var temp = Enum.GetValues(typeof(HumanBodyBones));
            var humanBodyBonesByTransforms =
                new Dictionary<HumanBodyBones, Transform>(55);

            var transformsByHumanBodyBones =
                new Dictionary<Transform, HumanBodyBones>(55);

            foreach (HumanBodyBones humanBodyBone in temp)
            {
                if (humanBodyBone == HumanBodyBones.LastBone) continue;

                var tr = animator.GetBoneTransform(humanBodyBone);
                if (tr != null)
                {
                    humanBodyBonesByTransforms.Add(humanBodyBone, tr);
                    transformsByHumanBodyBones.Add(tr, humanBodyBone);
                }
            }

            humanBodyBonesByTransforms.Add(HumanBodyBones.LastBone, characterRoot.transform);
            transformsByHumanBodyBones.Add(characterRoot.transform, HumanBodyBones.LastBone);
            return new DictionaryTuple(humanBodyBonesByTransforms, transformsByHumanBodyBones);
        }

        /// <summary>
        ///     Calculates the global rotations of the animation data read from binary file.
        /// </summary>
        /// <param name="transformByHumanoidBone">Dictionary that contains 'Transform' by 'HumanBodyBones'</param>
        /// <param name="humanoidBoneByTransform">Dictionary that contains 'HumanBodyBones' by 'Transform'</param>
        /// <param name="localQuaternionsByFrame">
        ///     Dictionary that contains all the localRotation values read from binary file for
        ///     every frame by 'HumanBodyBones'
        /// </param>
        /// <param name="clip">Animation data that read from binary file and casted into 'IT_CharacterTransformAnimationClip' type.</param>
        /// <returns></returns>
        private static Dictionary<HumanBodyBones, List<Quaternion>> GetGlobalRotationsFromAnimFile(
            Dictionary<HumanBodyBones, Transform> transformByHumanoidBone,
            Dictionary<Transform, HumanBodyBones> humanoidBoneByTransform,
            Dictionary<HumanBodyBones, List<Tuple<Quaternion, Vector3>>>
                localQuaternionsByFrame,
            IT_CharacterTransformAnimationClip clip)
        {
            var globalQuaternionsByFrame =
                new Dictionary<HumanBodyBones, List<Quaternion>>(55);

            for (var frame = clip.startFrameInTimelineWhenItWasCaptured; frame <= clip.lastFrameInTimelineWhenItWasCaptured; frame++)
            {
                foreach (HumanBodyBones humanBone in Enum.GetValues(typeof(HumanBodyBones)))
                {
                    if (!transformByHumanoidBone.ContainsKey(humanBone)) continue;

                    var boneTransform = transformByHumanoidBone[humanBone];
                    var globalRotationOfThisBone = Quaternion.identity;

                    while (boneTransform != null)
                    {
                        if (humanoidBoneByTransform.ContainsKey(boneTransform))
                        {
                            var bone = humanoidBoneByTransform[boneTransform];

                            var localRotationInAnimFile =
                                localQuaternionsByFrame[bone][frame].Item1;
                            globalRotationOfThisBone = localRotationInAnimFile * globalRotationOfThisBone;
                        }

                        boneTransform = boneTransform.parent;
                    }

                    if (!globalQuaternionsByFrame.ContainsKey(humanBone))
                        globalQuaternionsByFrame.Add(humanBone, new List<Quaternion>(clip.lastFrameInTimelineWhenItWasCaptured + 1));

                    globalQuaternionsByFrame[humanBone].Add(globalRotationOfThisBone);
                }
            }

            return globalQuaternionsByFrame;
        }

        /// <summary>
        ///     Stores all the localRotation data from the binary animation data into a dictionary.
        /// </summary>
        /// <param name="clip">Animation data that read from binary file and casted into 'IT_CharacterTransformAnimationClip' type.</param>
        /// <param name="transformsByHumanBoneName">Dictionary that contains 'Transform' by 'HumanBodyBones'</param>
        /// <returns></returns>
        private static Dictionary<HumanBodyBones, List<Tuple<Quaternion, Vector3>>>
            GetLocalTransformValuesFromAnimFile(
                IT_CharacterTransformAnimationClip clip,
                Dictionary<HumanBodyBones, Transform> transformsByHumanBoneName)
        {
            var localQuaternionsByFrame =
                new Dictionary<HumanBodyBones, List<Tuple<Quaternion, Vector3>>>(55);

            for (var frame = clip.startFrameInTimelineWhenItWasCaptured; frame <= clip.lastFrameInTimelineWhenItWasCaptured; frame++)
            {
                var transformIndex = 0;
                var time = clip.fixedDeltaTime * frame;


                foreach (var pair in transformsByHumanBoneName)
                {
                    var indexInCurveOfKey = frame * transformsByHumanBoneName.Count + transformIndex;


                    var frameRotation = new Quaternion(clip.physicsKeyframesCurve3[indexInCurveOfKey],
                        clip.physicsKeyframesCurve4[indexInCurveOfKey],
                        clip.physicsKeyframesCurve5[indexInCurveOfKey],
                        clip.physicsKeyframesCurve6[indexInCurveOfKey]);

                    var framePosition = new Vector3(clip.physicsKeyframesCurve0[indexInCurveOfKey],
                        clip.physicsKeyframesCurve1[indexInCurveOfKey],
                        clip.physicsKeyframesCurve2[indexInCurveOfKey]);

                    if (!localQuaternionsByFrame.ContainsKey(pair.Key))
                    {
                        localQuaternionsByFrame.Add(pair.Key,
                            new List<Tuple<Quaternion, Vector3>>(clip.lastFrameInTimelineWhenItWasCaptured + 1));
                    }

                    localQuaternionsByFrame[pair.Key]
                        .Add(new Tuple<Quaternion, Vector3>(frameRotation, framePosition));
                    transformIndex++;
                }
            }

            return localQuaternionsByFrame;
        }

        #endregion


        #region Animation Operations

        /// <summary>
        ///     Reads the binary file that contains animation data from it's designated path. Creates a dictionary with the key of
        ///     'HumanBodyBones' and Transform that corresponds to the enum value. Note that this function assumes that the
        ///     character is 'Humanoid' .
        /// </summary>
        /// <param name="loadedFbXofCharacter">Tuple of loaded character.</param>
        /// <returns>Tuple with the read and casted animation data from binary file and the dictionary of the humanoid bones.</returns>
        private static ClipByDictionaryTuple
            PrepareAndGetAnimationData(FbxData loadedFbXofCharacter)
        {
            var clip =
                SerializationUtility.DeserializeValue<IT_CharacterTransformAnimationClip>(
                    File.ReadAllBytes(_AnimationClipDataPath),
                    DataFormat.Binary);


            var animator = loadedFbXofCharacter.FbxAnimator;
            var
                boneTransformDictionaries = GetBoneTransformDictionaries(animator, loadedFbXofCharacter.FbxGameObject);

            animator.avatar = null;

            AssetDatabase.Refresh();
            return new ClipByDictionaryTuple(clip, boneTransformDictionaries);
        }

        /// <summary>
        ///     Creates the Animation clip according to the given data contains clip info read from json and
        ///     BoneTransformDictionaries
        /// </summary>
        /// <param name="clipAndDictionariesTuple">Contains clip info read from json and BoneTransformDictionaries</param>
        /// <param name="characterRoot">Root gameObject of the character to apply animation to.</param>
        private static AnimationClip CreateTransformMovementsAnimationClip(ClipByDictionaryTuple clipAndDictionariesTuple,
            GameObject characterRoot)
        {
            var clip = clipAndDictionariesTuple.Clip;
            var transformsByHumanBodyBones = clipAndDictionariesTuple.DictTuple.TransformsByHumanBodyBones;
            var humanBodyBonesBytransforms = clipAndDictionariesTuple.DictTuple.HumanBodyBonesByTransform;

            var animationClip = new AnimationClip();

            // this dictionary is used to store the values of bones at every frame to be applied to animation clip later.
            var pathAndKeyframesDictionary =
                new Dictionary<string, List<List<Keyframe>>>(55);


            var localTransformValuesFromAnimFile =
                GetLocalTransformValuesFromAnimFile(clip, transformsByHumanBodyBones);


            var globalQuaternionsByFrame =
                GetGlobalRotationsFromAnimFile(transformsByHumanBodyBones, humanBodyBonesBytransforms, localTransformValuesFromAnimFile, clip);

            //HARDCODE !
            var path = string.Concat(Directory.GetCurrentDirectory(),
                @"\Assets\AnimotivePluginExampleStructure\Example Data\Animation\Poses");

            var animotiveTPoseText = File.ReadAllText(string.Concat(path, "\\AnimotiveTPoseFrank.json"));
            var editorTPoseText = File.ReadAllText(string.Concat(path, "\\EditorTPoseFrank.json"));


            var animotiveTPoseTransformInfoList =
                JsonUtility.FromJson<IT_TransformInfoList>(animotiveTPoseText);
            var editorTPoseTransformInfoList =
                JsonUtility.FromJson<IT_TransformInfoList>(editorTPoseText);

            //loop as long as the frame count from the binary file (exported from Animotive)
            for (var frame = clip.startFrameInTimelineWhenItWasCaptured; frame <= clip.lastFrameInTimelineWhenItWasCaptured; frame++)
            {
                var transformIndex = 0;
                var time = clip.fixedDeltaTime * frame;

                //loop through every bone at every frame
                foreach (var pair in transformsByHumanBodyBones)
                {
                    var relativePath =
                        AnimationUtility.CalculateTransformPath(pair.Value, characterRoot.transform);
                    if (!pathAndKeyframesDictionary.ContainsKey(relativePath))
                    {
                        //initialize a new list of keyframes for the
                        pathAndKeyframesDictionary.Add(relativePath, new List<List<Keyframe>>(55));

                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>(clip.lastFrameInTimelineWhenItWasCaptured + 1)); //0
                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>(clip.lastFrameInTimelineWhenItWasCaptured + 1)); //1
                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>(clip.lastFrameInTimelineWhenItWasCaptured + 1)); //2
                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>(clip.lastFrameInTimelineWhenItWasCaptured + 1)); //3
                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>(clip.lastFrameInTimelineWhenItWasCaptured + 1)); //4
                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>(clip.lastFrameInTimelineWhenItWasCaptured + 1)); //5
                        pathAndKeyframesDictionary[relativePath].Add(new List<Keyframe>(clip.lastFrameInTimelineWhenItWasCaptured + 1)); //6
                    }

                    // do not add the rotations of the root transform to animation clip
                    if (pair.Key != HumanBodyBones.LastBone)
                    {
                        var boneGlobalRotationThisFrameFromAnimFile = globalQuaternionsByFrame[pair.Key][frame];

                        var editorTPoseList = editorTPoseTransformInfoList.TransformsByStrings
                            .Where(a => a.Name == pair.Key).ToList();
                        var animotiveTPoseList = animotiveTPoseTransformInfoList
                            .TransformsByStrings
                            .Where(a => a.Name == pair.Key).ToList();

                        var editorTPoseRotationForThisBone = editorTPoseList[0].GlobalRotation;
                        var animotiveTPoseRotationThisBone = animotiveTPoseList[0].GlobalRotation;
                        var invAnimotiveTPoseRotationThisBone =
                            Quaternion.Inverse(animotiveTPoseRotationThisBone);

                        var boneRotation = invAnimotiveTPoseRotationThisBone *
                                           boneGlobalRotationThisFrameFromAnimFile *
                                           editorTPoseRotationForThisBone;

                        pair.Value.rotation = boneRotation;

                        var inversedParentBoneRotation = Quaternion.Inverse(pair.Value.parent == null
                            ? Quaternion.identity
                            : pair.Value.parent.rotation);

                        var finalLocalRotation = inversedParentBoneRotation * boneRotation;


                        var localRotationX = new Keyframe(time, finalLocalRotation.x);
                        var localRotationY = new Keyframe(time, finalLocalRotation.y);
                        var localRotationZ = new Keyframe(time, finalLocalRotation.z);
                        var localRotationW = new Keyframe(time, finalLocalRotation.w);

                        pathAndKeyframesDictionary[relativePath][3].Add(localRotationX);
                        pathAndKeyframesDictionary[relativePath][4].Add(localRotationY);
                        pathAndKeyframesDictionary[relativePath][5].Add(localRotationZ);
                        pathAndKeyframesDictionary[relativePath][6].Add(localRotationW);
                    }

                    //add the position of selected bones to animationclip
                    if (pair.Key == HumanBodyBones.Hips || pair.Key == HumanBodyBones.LastBone)
                    {
                        var position = localTransformValuesFromAnimFile[pair.Key][frame].Item2;
                        var inversedPosition = pair.Value.InverseTransformPoint(position);

                        var localPositionX = new Keyframe(time, position.x);
                        var localPositionY = new Keyframe(time, inversedPosition.y);
                        var localPositionZ = new Keyframe(time, position.z);

                        pathAndKeyframesDictionary[relativePath][0].Add(localPositionX);
                        pathAndKeyframesDictionary[relativePath][1].Add(localPositionY);
                        pathAndKeyframesDictionary[relativePath][2].Add(localPositionZ);
                    }

                    transformIndex++;
                }
            }


            foreach (var pair in pathAndKeyframesDictionary)
            {
                var relativePath = pair.Key;

                var rotationCurveX = new AnimationCurve(pair.Value[3].ToArray());
                var rotationCurveY = new AnimationCurve(pair.Value[4].ToArray());
                var rotationCurveZ = new AnimationCurve(pair.Value[5].ToArray());
                var rotationCurveW = new AnimationCurve(pair.Value[6].ToArray());

                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.x", rotationCurveX);
                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.y", rotationCurveY);
                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.z", rotationCurveZ);
                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.w", rotationCurveW);
                if (pair.Value[0].Count != 0)
                {
                    var positionCurveX = new AnimationCurve(pair.Value[0].ToArray());
                    var positionCurveY = new AnimationCurve(pair.Value[1].ToArray());
                    var positionCurveZ = new AnimationCurve(pair.Value[2].ToArray());

                    animationClip.SetCurve(relativePath, typeof(Transform), "localPosition.x", positionCurveX);
                    animationClip.SetCurve(relativePath, typeof(Transform), "localPosition.y", positionCurveY);
                    animationClip.SetCurve(relativePath, typeof(Transform), "localPosition.z", positionCurveZ);
                }

                animationClip.EnsureQuaternionContinuity();
            }


            AssetDatabase.CreateAsset(animationClip, bodyAnimationPath);
            AssetDatabase.Refresh();

            return animationClip;
        }

        /// <summary>
        ///     Triggers all the necessary methods for the related animation clip creation PoC
        /// </summary>
        public static GameObject HandleBodyAnimationClipOperations(string animationClipDataPath)
        {
            _AnimationClipDataPath = animationClipDataPath;
            var baseBodyPathWithNameWithoutExtension = string.Concat(
                IT_AnimotiveImporterEditorConstants.BodyAnimationDirectory,
                Path.GetFileNameWithoutExtension(animationClipDataPath));

            bodyAnimationPath = string.Concat(baseBodyPathWithNameWithoutExtension, ".anim");

            bodyAnimatorPath = string.Concat(baseBodyPathWithNameWithoutExtension, ".controller");

            bodyAnimationName = Path.GetFileName(animationClipDataPath);

            var fbxData = IT_AnimotiveImporterEditorUtilities.LoadFbx();

            var clipAndDictionariesTuple = PrepareAndGetAnimationData(fbxData);

            IT_AnimotiveImporterEditorUtilities
                .DeleteAssetIfExists(bodyAnimationPath,
                    typeof(AnimationClip));

            var animationClip =
                CreateTransformMovementsAnimationClip(clipAndDictionariesTuple, fbxData.FbxGameObject);

            var animatorController =
                AnimatorController.CreateAnimatorControllerAtPathWithClip(bodyAnimatorPath, animationClip);

            AssetDatabase.Refresh();

            fbxData.FbxAnimator.runtimeAnimatorController = animatorController;

            return fbxData.FbxGameObject;
        }

        #endregion
    }
}