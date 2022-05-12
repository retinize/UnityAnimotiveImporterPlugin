namespace AnimotiveImporterEditor
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


    public static class IT_TransformAnimationClipEditor
    {
    #region Dictionary Operations

        /// <summary>
        ///     Creates and returns two dictionary where 'HumanBodyBones' and 'Transform' types are key/value and vice versa.
        /// </summary>
        /// <param name="animator">Animator of the character</param>
        /// <param name="characterRoot">Root GameObject of the character</param>
        /// <returns></returns>
        private static Tuple<Dictionary<HumanBodyBones, Transform>,
                Dictionary<Transform, HumanBodyBones>>
            GetBoneTransformDictionaries(
                Animator animator, GameObject characterRoot)
        {
            Array temp = Enum.GetValues(typeof(HumanBodyBones));
            Dictionary<HumanBodyBones, Transform> humanBodyBonesByTransforms =
                new Dictionary<HumanBodyBones, Transform>(55);

            Dictionary<Transform, HumanBodyBones> transformsByHumanBodyBones =
                new Dictionary<Transform, HumanBodyBones>(55);

            foreach (HumanBodyBones humanBodyBone in temp)
            {
                if (humanBodyBone == HumanBodyBones.LastBone)
                {
                    continue;
                }

                Transform tr = animator.GetBoneTransform(humanBodyBone);
                if (tr != null)
                {
                    humanBodyBonesByTransforms.Add(humanBodyBone, tr);
                    transformsByHumanBodyBones.Add(tr, humanBodyBone);
                }
            }

            humanBodyBonesByTransforms.Add(HumanBodyBones.LastBone, characterRoot.transform);

            return new
                Tuple<Dictionary<HumanBodyBones, Transform>,
                    Dictionary<Transform, HumanBodyBones>>(humanBodyBonesByTransforms, transformsByHumanBodyBones);
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

        /// <summary>
        ///     Stores all the localRotation data from the binary animation data into a dictionary.
        /// </summary>
        /// <param name="clip">Animation data that read from binary file and casted into 'IT_CharacterTransformAnimationClip' type.</param>
        /// <param name="transformsByHumanBoneName">Dictionary that contains 'Transform' by 'HumanBodyBones'</param>
        /// <returns></returns>
        private static Dictionary<HumanBodyBones, List<Quaternion>> GetLocalQuaternionsFromAnimFile(
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

    #endregion


    #region Animation Operations

        /// <summary>
        ///     Reads the binary file that contains animation data from it's designated path. Creates a dictionary with the key of
        ///     'HumanBodyBones' and Transform that corresponds to the enum value. Note that this function assumes that the
        ///     character is 'Humanoid' .
        /// </summary>
        /// <param name="tuple">Tuple of loaded character.</param>
        /// <returns>Tuple with the read and casted animation data from binary file and the dictionary of the humanoid bones.</returns>
        private static Tuple<IT_CharacterTransformAnimationClip,
                Tuple<Dictionary<HumanBodyBones, Transform>, Dictionary<Transform, HumanBodyBones>>>
            PrepareAndGetAnimationData(Tuple<GameObject, Animator> tuple)
        {
            string hardcodedAnimationDataPath =
                string.Concat(Directory.GetCurrentDirectory(), IT_AnimotiveImporterEditorConstants.BinaryAnimPath);

            IT_CharacterTransformAnimationClip clip =
                SerializationUtility.DeserializeValue<IT_CharacterTransformAnimationClip>(
                 File.ReadAllBytes(hardcodedAnimationDataPath),
                 DataFormat.Binary);


            Animator animator = tuple.Item2;
            Tuple<Dictionary<HumanBodyBones, Transform>, Dictionary<Transform, HumanBodyBones>>
                boneTransformDictionaries = GetBoneTransformDictionaries(animator, tuple.Item1);

            animator.avatar = null;


            AssetDatabase.Refresh();
            return new
                Tuple<IT_CharacterTransformAnimationClip, Tuple<Dictionary<HumanBodyBones, Transform>,
                    Dictionary<Transform, HumanBodyBones>>>(clip,
                                                            boneTransformDictionaries);
        }

        /// <summary>
        ///     Creates the Animation clip according to the given data contains clip info read from json and
        ///     BoneTransformDictionaries
        /// </summary>
        /// <param name="clipAndDictionariesTuple">Contains clip info read from json and BoneTransformDictionaries</param>
        /// <param name="characterRoot">Root gameObject of the character to apply animation to.</param>
        private static AnimationClip CreateTransformMovementsAnimationClip(
            Tuple<IT_CharacterTransformAnimationClip, Tuple<Dictionary<HumanBodyBones, Transform>,
                Dictionary<Transform, HumanBodyBones>>> clipAndDictionariesTuple,
            GameObject characterRoot)
        {
            IT_CharacterTransformAnimationClip    clip                       = clipAndDictionariesTuple.Item1;
            Dictionary<HumanBodyBones, Transform> transformsByHumanBodyBones = clipAndDictionariesTuple.Item2.Item1;
            Dictionary<Transform, HumanBodyBones> HumanBodyBonesBytransforms = clipAndDictionariesTuple.Item2.Item2;


            AnimationClip animationClip = new AnimationClip();


            Dictionary<string, List<List<Keyframe>>> pathAndKeyframesDictionary =
                new Dictionary<string, List<List<Keyframe>>>(55);


            Dictionary<HumanBodyBones, List<Quaternion>> localQuaternionsByFrame =
                GetLocalQuaternionsFromAnimFile(clip, transformsByHumanBodyBones);

            Dictionary<HumanBodyBones, List<Quaternion>> globalQuaternionsByFrame =
                GetGlobalRotationsFromAnimFile(transformsByHumanBodyBones,
                                               HumanBodyBonesBytransforms,
                                               localQuaternionsByFrame, clip);

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


                foreach (KeyValuePair<HumanBodyBones, Transform> pair in transformsByHumanBodyBones)
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

                AnimationCurve rotationCurveX = new AnimationCurve(keyValuePair.Value[3].ToArray());
                AnimationCurve rotationCurveY = new AnimationCurve(keyValuePair.Value[4].ToArray());
                AnimationCurve rotationCurveZ = new AnimationCurve(keyValuePair.Value[5].ToArray());
                AnimationCurve rotationCurveW = new AnimationCurve(keyValuePair.Value[6].ToArray());

                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.x", rotationCurveX);
                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.y", rotationCurveY);
                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.z", rotationCurveZ);
                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.w", rotationCurveW);
                animationClip.EnsureQuaternionContinuity();
            }


            AssetDatabase.CreateAsset(animationClip, IT_AnimotiveImporterEditorConstants.TransformAnimPath);
            AssetDatabase.Refresh();

            return animationClip;
        }


        public static void HandleTransformAnimationClipOperations()
        {
            Tuple<GameObject, Animator> fbxTuple = IT_AnimotiveImporterEditorUtilities.LoadFbx();

            Tuple<IT_CharacterTransformAnimationClip, Tuple<Dictionary<HumanBodyBones, Transform>,
                Dictionary<Transform, HumanBodyBones>>> clipAndDictionariesTuple =
                PrepareAndGetAnimationData(fbxTuple);

            IT_AnimotiveImporterEditorUtilities
                .DeleteAssetIfExists(IT_AnimotiveImporterEditorConstants.TransformAnimPath,
                                     typeof(AnimationClip));
            AnimationClip animationClip =
                CreateTransformMovementsAnimationClip(clipAndDictionariesTuple,
                                                      fbxTuple.Item1);


            AnimatorController animatorController =
                AnimatorController.CreateAnimatorControllerAtPathWithClip(IT_AnimotiveImporterEditorConstants
                                                                              .TransformsAnimController, animationClip);
            AssetDatabase.Refresh();


            fbxTuple.Item2.runtimeAnimatorController = animatorController;
        }

    #endregion
    }
}