using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnimotiveImporterDLL;
using OdinSerializer;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    public static class IT_TransformAnimationClipEditor
    {

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
        private static IT_DictionaryTuple GetBoneTransformDictionaries(Animator animator, GameObject characterRoot)
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
            transformsByHumanBodyBones.Add(characterRoot.transform, HumanBodyBones.LastBone);
            return new IT_DictionaryTuple(humanBodyBonesByTransforms, transformsByHumanBodyBones);
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
            Dictionary<HumanBodyBones, List<Quaternion>> globalQuaternionsByFrame =
                new Dictionary<HumanBodyBones, List<Quaternion>>(55);

            for (int frame = clip.startFrameInTimelineWhenItWasCaptured;
                 frame <= clip.lastFrameInTimelineWhenItWasCaptured;
                 frame++)
            {
                foreach (HumanBodyBones humanBone in Enum.GetValues(typeof(HumanBodyBones)))
                {
                    if (!transformByHumanoidBone.ContainsKey(humanBone))
                    {
                        continue;
                    }

                    Transform boneTransform = transformByHumanoidBone[humanBone];
                    Quaternion globalRotationOfThisBone = Quaternion.identity;

                    while (boneTransform != null)
                    {
                        if (humanoidBoneByTransform.ContainsKey(boneTransform))
                        {
                            HumanBodyBones bone = humanoidBoneByTransform[boneTransform];

                            Quaternion localRotationInAnimFile =
                                localQuaternionsByFrame[bone][frame].Item1;
                            globalRotationOfThisBone = localRotationInAnimFile * globalRotationOfThisBone;
                        }

                        boneTransform = boneTransform.parent;
                    }

                    if (!globalQuaternionsByFrame.ContainsKey(humanBone))
                    {
                        globalQuaternionsByFrame.Add(humanBone,
                            new List<Quaternion>(clip.lastFrameInTimelineWhenItWasCaptured + 1));
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
        private static Dictionary<HumanBodyBones, List<Tuple<Quaternion, Vector3>>>
            GetLocalTransformValuesFromAnimFile(
                IT_CharacterTransformAnimationClip clip,
                Dictionary<HumanBodyBones, Transform> transformsByHumanBoneName)
        {
            Dictionary<HumanBodyBones, List<Tuple<Quaternion, Vector3>>> localQuaternionsByFrame =
                new Dictionary<HumanBodyBones, List<Tuple<Quaternion, Vector3>>>(55);

            for (int frame = clip.startFrameInTimelineWhenItWasCaptured;
                 frame <= clip.lastFrameInTimelineWhenItWasCaptured;
                 frame++)
            {
                int transformIndex = 0;
                float time = clip.fixedDeltaTime * frame;


                foreach (KeyValuePair<HumanBodyBones, Transform> pair in transformsByHumanBoneName)
                {
                    int indexInCurveOfKey = frame * transformsByHumanBoneName.Count + transformIndex;


                    Quaternion frameRotation = new Quaternion(clip.physicsKeyframesCurve3[indexInCurveOfKey],
                        clip.physicsKeyframesCurve4[indexInCurveOfKey],
                        clip.physicsKeyframesCurve5[indexInCurveOfKey],
                        clip.physicsKeyframesCurve6[indexInCurveOfKey]);

                    Vector3 framePosition = new Vector3(clip.physicsKeyframesCurve0[indexInCurveOfKey],
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
        private static IT_ClipByDictionaryTuple PrepareAndGetAnimationData(IT_FbxData loadedFbXofCharacter, string animationClipDataPath)
        {
            IT_CharacterTransformAnimationClip clip =
                SerializationUtility.DeserializeValue<IT_CharacterTransformAnimationClip>(
                    File.ReadAllBytes(animationClipDataPath),
                    DataFormat.Binary);


            Animator animator = loadedFbXofCharacter.FbxAnimator;

            IT_DictionaryTuple boneTransformDictionaries =
                GetBoneTransformDictionaries(animator, loadedFbXofCharacter.FbxGameObject);

            animator.avatar = null;

            AssetDatabase.Refresh();
            return new IT_ClipByDictionaryTuple(clip, boneTransformDictionaries);
        }

        /// <summary>
        ///     Creates the Animation clip according to the given data contains clip info read from json and
        ///     BoneTransformDictionaries
        /// </summary>
        /// <param name="itClipAndDictionariesTuple">Contains clip info read from json and BoneTransformDictionaries</param>
        /// <param name="characterRoot">Root gameObject of the character to apply animation to.</param>
        private static AnimationClip CreateTransformMovementsAnimationClip(
            IT_ClipByDictionaryTuple itClipAndDictionariesTuple,
            GameObject characterRoot)
        {
            IT_CharacterTransformAnimationClip clip = itClipAndDictionariesTuple.Clip;
            Dictionary<HumanBodyBones, Transform> transformsByHumanBodyBones =
                itClipAndDictionariesTuple.DictTuple.TransformsByHumanBodyBones;
            Dictionary<Transform, HumanBodyBones> humanBodyBonesBytransforms =
                itClipAndDictionariesTuple.DictTuple.HumanBodyBonesByTransform;

            AnimationClip animationClip = new AnimationClip();

            // this dictionary is used to store the values of bones at every frame to be applied to animation clip later.
            Dictionary<string, List<List<Keyframe>>> pathAndKeyframesDictionary =
                new Dictionary<string, List<List<Keyframe>>>(55);


            Dictionary<HumanBodyBones, List<Tuple<Quaternion, Vector3>>> localTransformValuesFromAnimFile =
                GetLocalTransformValuesFromAnimFile(clip, transformsByHumanBodyBones);


            Dictionary<HumanBodyBones, List<Quaternion>> globalQuaternionsByFrame =
                GetGlobalRotationsFromAnimFile(transformsByHumanBodyBones, humanBodyBonesBytransforms,
                    localTransformValuesFromAnimFile, clip);

            //HARDCODE !
            string path = string.Concat(Directory.GetCurrentDirectory(),
                @"\Assets\AnimotivePluginExampleStructure\Example Data\Animation\Poses");

            string animotiveTPoseText = File.ReadAllText(string.Concat(path, "\\AnimotiveTPoseFrank.json"));
            string editorTPoseText = File.ReadAllText(string.Concat(path, "\\EditorTPoseFrank.json"));


            IT_TransformInfoList animotiveTPoseTransformInfoList =
                JsonUtility.FromJson<IT_TransformInfoList>(animotiveTPoseText);
            IT_TransformInfoList editorTPoseTransformInfoList =
                JsonUtility.FromJson<IT_TransformInfoList>(editorTPoseText);

            //loop as long as the frame count from the binary file (exported from Animotive)
            for (int frame = clip.startFrameInTimelineWhenItWasCaptured;
                 frame <= clip.lastFrameInTimelineWhenItWasCaptured;
                 frame++)
            {
                int transformIndex = 0;
                float time = clip.fixedDeltaTime * frame;

                //loop through every bone at every frame
                foreach (KeyValuePair<HumanBodyBones, Transform> pair in transformsByHumanBodyBones)
                {
                    string relativePath =
                        AnimationUtility.CalculateTransformPath(pair.Value, characterRoot.transform);
                    if (!pathAndKeyframesDictionary.ContainsKey(relativePath))
                    {
                        //initialize a new list of keyframes for the
                        pathAndKeyframesDictionary.Add(relativePath, new List<List<Keyframe>>(55));

                        pathAndKeyframesDictionary[relativePath]
                            .Add(new List<Keyframe>(clip.lastFrameInTimelineWhenItWasCaptured + 1)); //0
                        pathAndKeyframesDictionary[relativePath]
                            .Add(new List<Keyframe>(clip.lastFrameInTimelineWhenItWasCaptured + 1)); //1
                        pathAndKeyframesDictionary[relativePath]
                            .Add(new List<Keyframe>(clip.lastFrameInTimelineWhenItWasCaptured + 1)); //2
                        pathAndKeyframesDictionary[relativePath]
                            .Add(new List<Keyframe>(clip.lastFrameInTimelineWhenItWasCaptured + 1)); //3
                        pathAndKeyframesDictionary[relativePath]
                            .Add(new List<Keyframe>(clip.lastFrameInTimelineWhenItWasCaptured + 1)); //4
                        pathAndKeyframesDictionary[relativePath]
                            .Add(new List<Keyframe>(clip.lastFrameInTimelineWhenItWasCaptured + 1)); //5
                        pathAndKeyframesDictionary[relativePath]
                            .Add(new List<Keyframe>(clip.lastFrameInTimelineWhenItWasCaptured + 1)); //6
                    }

                    // do not add the rotations of the root transform to animation clip
                    if (pair.Key != HumanBodyBones.LastBone)
                    {
                        Quaternion boneGlobalRotationThisFrameFromAnimFile = globalQuaternionsByFrame[pair.Key][frame];

                        List<IT_TransformsByString> editorTPoseList = editorTPoseTransformInfoList.TransformsByStrings
                            .Where(a => a.Name == pair.Key).ToList();
                        List<IT_TransformsByString> animotiveTPoseList = animotiveTPoseTransformInfoList
                            .TransformsByStrings
                            .Where(a => a.Name == pair.Key).ToList();

                        Quaternion editorTPoseRotationForThisBone = editorTPoseList[0].GlobalRotation;
                        Quaternion animotiveTPoseRotationThisBone = animotiveTPoseList[0].GlobalRotation;
                        Quaternion invAnimotiveTPoseRotationThisBone =
                            Quaternion.Inverse(animotiveTPoseRotationThisBone);

                        Quaternion boneRotation = invAnimotiveTPoseRotationThisBone *
                                                  boneGlobalRotationThisFrameFromAnimFile *
                                                  editorTPoseRotationForThisBone;

                        pair.Value.rotation = boneRotation;

                        Quaternion inversedParentBoneRotation = Quaternion.Inverse(pair.Value.parent == null
                            ? Quaternion.identity
                            : pair.Value.parent.rotation);

                        Quaternion finalLocalRotation = inversedParentBoneRotation * boneRotation;


                        Keyframe localRotationX = new Keyframe(time, finalLocalRotation.x);
                        Keyframe localRotationY = new Keyframe(time, finalLocalRotation.y);
                        Keyframe localRotationZ = new Keyframe(time, finalLocalRotation.z);
                        Keyframe localRotationW = new Keyframe(time, finalLocalRotation.w);

                        pathAndKeyframesDictionary[relativePath][3].Add(localRotationX);
                        pathAndKeyframesDictionary[relativePath][4].Add(localRotationY);
                        pathAndKeyframesDictionary[relativePath][5].Add(localRotationZ);
                        pathAndKeyframesDictionary[relativePath][6].Add(localRotationW);
                    }

                    //add the position of selected bones to animationclip
                    if (pair.Key == HumanBodyBones.Hips || pair.Key == HumanBodyBones.LastBone)
                    {
                        Vector3 position = localTransformValuesFromAnimFile[pair.Key][frame].Item2;
                        Vector3 inversedPosition = pair.Value.InverseTransformPoint(position);

                        Keyframe localPositionX = new Keyframe(time, position.x);
                        Keyframe localPositionY = new Keyframe(time, inversedPosition.y);
                        Keyframe localPositionZ = new Keyframe(time, position.z);

                        pathAndKeyframesDictionary[relativePath][0].Add(localPositionX);
                        pathAndKeyframesDictionary[relativePath][1].Add(localPositionY);
                        pathAndKeyframesDictionary[relativePath][2].Add(localPositionZ);
                    }

                    transformIndex++;
                }
            }


            foreach (KeyValuePair<string, List<List<Keyframe>>> pair in pathAndKeyframesDictionary)
            {
                string relativePath = pair.Key;

                AnimationCurve rotationCurveX = new AnimationCurve(pair.Value[3].ToArray());
                AnimationCurve rotationCurveY = new AnimationCurve(pair.Value[4].ToArray());
                AnimationCurve rotationCurveZ = new AnimationCurve(pair.Value[5].ToArray());
                AnimationCurve rotationCurveW = new AnimationCurve(pair.Value[6].ToArray());

                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.x", rotationCurveX);
                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.y", rotationCurveY);
                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.z", rotationCurveZ);
                animationClip.SetCurve(relativePath, typeof(Transform), "localRotation.w", rotationCurveW);
                if (pair.Value[0].Count != 0)
                {
                    AnimationCurve positionCurveX = new AnimationCurve(pair.Value[0].ToArray());
                    AnimationCurve positionCurveY = new AnimationCurve(pair.Value[1].ToArray());
                    AnimationCurve positionCurveZ = new AnimationCurve(pair.Value[2].ToArray());

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
        public static void HandleBodyAnimationClipOperations(IT_SceneInternalData sceneData, string clipsPath,
            IT_FbxData fbxData)
        {
            Dictionary<IT_ClipType, string> clipsTypeByPath = GetClipsPathByType(sceneData, clipsPath);

            foreach (KeyValuePair<IT_ClipType, string> pair in clipsTypeByPath)
            {
                string animationClipDataPath = pair.Value;
                
                string baseBodyPathWithNameWithoutExtension = string.Concat(
                    IT_AnimotiveImporterEditorConstants.BodyAnimationDirectory,
                    Path.GetFileNameWithoutExtension(animationClipDataPath));

                bodyAnimationPath = string.Concat(baseBodyPathWithNameWithoutExtension, ".anim");

                bodyAnimatorPath = string.Concat(baseBodyPathWithNameWithoutExtension, ".controller");

                bodyAnimationName = Path.GetFileName(animationClipDataPath);


                IT_ClipByDictionaryTuple clipAndDictionariesTuple = PrepareAndGetAnimationData(fbxData,animationClipDataPath);

                IT_AnimotiveImporterEditorUtilities
                    .DeleteAssetIfExists(bodyAnimationPath,
                        typeof(AnimationClip));

                AnimationClip animationClip =
                    CreateTransformMovementsAnimationClip(clipAndDictionariesTuple, fbxData.FbxGameObject);

                AnimatorController animatorController =
                    AnimatorController.CreateAnimatorControllerAtPathWithClip(bodyAnimatorPath, animationClip);


                fbxData.FbxAnimator.runtimeAnimatorController = animatorController;
            }

            AssetDatabase.Refresh();
        }

        private static Dictionary<IT_ClipType, string> GetClipsPathByType(IT_SceneInternalData sceneData,
            string clipsPath)
        {
            Dictionary<IT_ClipType, string> clipPathsByType = new Dictionary<IT_ClipType, string>();

            foreach (GroupData groupData in sceneData.groupDataById.Values)
            {
                foreach (int entityId in groupData.entitiesIds)
                {
                    EntityData entityData = sceneData.entitiesDataBySerializedId[entityId];
                    //Here you can read the entity data that belongs to this group, such as its animation clips per take
                    for (int i = 0; i < entityData.clipsByTrackByTakeIndex.Count; i++)
                    {
                        List<List<IT_ClipPlayerData>> takes = entityData.clipsByTrackByTakeIndex[i];

                        for (int j = 0; j < takes.Count; j++)
                        {
                            List<IT_ClipPlayerData> tracks = takes[j];

                            for (int k = 0; k < tracks.Count; k++)
                            {
                                IT_ClipPlayerData clip = tracks[k];

                                string animationClipDataPath =
                                    IT_AnimotiveImporterEditorUtilities.ReturnClipDataFromPath(clipsPath,
                                        clip.clipName);

                                IT_ClipType type =
                                    IT_AnimotiveImporterEditorUtilities.GetClipTypeFromClipName(clip.clipName);

                                clipPathsByType.Add(type, animationClipDataPath);
                            }
                        }
                    }
                }
            }

            return clipPathsByType;
        }

        #endregion
    }
}