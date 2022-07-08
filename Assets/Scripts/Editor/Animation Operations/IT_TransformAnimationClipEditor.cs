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
            Dictionary<HumanBodyBones, List<IT_TransformValues>>
                localQuaternionsByFrame,
            IT_CharacterTransformAnimationClip clip, IT_ClipData clipData)
        {
            var globalQuaternionsByFrame =
                new Dictionary<HumanBodyBones, List<Quaternion>>(55);

            var startFrame = clip.startFrameInTimelineWhenItWasCaptured;
            var lastFrame = clip.lastFrameInTimelineWhenItWasCaptured;

            for (var frame = startFrame;
                 frame <= lastFrame;
                 frame++)
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
                                localQuaternionsByFrame[bone][frame].Rotation;
                            globalRotationOfThisBone = localRotationInAnimFile * globalRotationOfThisBone;
                        }

                        boneTransform = boneTransform.parent;
                    }

                    if (!globalQuaternionsByFrame.ContainsKey(humanBone))
                    {
                        globalQuaternionsByFrame.Add(humanBone,
                            new List<Quaternion>(lastFrame + 1));
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
        private static Dictionary<HumanBodyBones, List<IT_TransformValues>>
            GetLocalTransformValuesFromAnimFile(IT_CharacterTransformAnimationClip clip,
                Dictionary<HumanBodyBones, Transform> transformsByHumanBoneName, IT_ClipData clipData)
        {
            var localQuaternionsByFrame = new Dictionary<HumanBodyBones, List<IT_TransformValues>>(55);

            var startFrame = clip.startFrameInTimelineWhenItWasCaptured;
            var lastFrame = clip.lastFrameInTimelineWhenItWasCaptured;


            for (var frame = startFrame;
                 frame <= lastFrame;
                 frame++)
            {
                var transformIndex = 0;
                var time = clip.fixedDeltaTime * frame;


                foreach (var pair in transformsByHumanBoneName)
                {
                    var indexInCurveOfKey = frame * transformsByHumanBoneName.Count + transformIndex;

                    //462 frame ... 8 more frame to go 470
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
                            new List<IT_TransformValues>(lastFrame + 1));
                    }

                    localQuaternionsByFrame[pair.Key]
                        .Add(new IT_TransformValues(frameRotation, framePosition));
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
        /// <param name="animationClipDataPath">Path to binary animation clip data</param>
        /// <returns>Tuple with the read and casted animation data from binary file and the dictionary of the humanoid bones.</returns>
        private static IT_ClipByDictionaryTuple PrepareAndGetAnimationData(IT_FbxData loadedFbXofCharacter,
            string animationClipDataPath)
        {
            var clip =
                SerializationUtility.DeserializeValue<IT_CharacterTransformAnimationClip>(
                    File.ReadAllBytes(animationClipDataPath),
                    DataFormat.Binary);


            var animator = loadedFbXofCharacter.FbxAnimator;

            var boneTransformDictionaries =
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
            GameObject characterRoot, IT_ClipData clipData)
        {
            var clip = itClipAndDictionariesTuple.Clip;
            var transformsByHumanBodyBones =
                itClipAndDictionariesTuple.DictTuple.TransformsByHumanBodyBones;

            var humanBodyBonesBytransforms =
                itClipAndDictionariesTuple.DictTuple.HumanBodyBonesByTransform;

            var animationClip = new AnimationClip();

            // this dictionary is used to store the values of bones at every frame to be applied to animation clip later.
            var pathAndKeyframesDictionary =
                new Dictionary<string, List<List<Keyframe>>>(55);


            var localTransformValuesFromAnimFile =
                GetLocalTransformValuesFromAnimFile(clip, transformsByHumanBodyBones, clipData);


            var globalQuaternionsByFrame =
                GetGlobalRotationsFromAnimFile(transformsByHumanBodyBones, humanBodyBonesBytransforms,
                    localTransformValuesFromAnimFile, clip, clipData);

            //HARDCODE !
            var path = string.Concat(Directory.GetCurrentDirectory(),
                @"\Assets\AnimotivePluginExampleStructure\Example Data\Animation\Poses");


            var animotiveTPoseText = File.ReadAllText(string.Concat(path, "\\AnimotiveTPoseFrank.json"));
            var editorTPoseText = File.ReadAllText(string.Concat(path, "\\EditorTPoseFrank.json"));

            var animotiveTPoseTransformInfoList =
                JsonUtility.FromJson<IT_TransformInfoList>(animotiveTPoseText);
            var editorTPoseTransformInfoList =
                JsonUtility.FromJson<IT_TransformInfoList>(editorTPoseText);


            var startFrame = clip.startFrameInTimelineWhenItWasCaptured;
            var lastFrame = clip.lastFrameInTimelineWhenItWasCaptured;

            //loop as long as the frame count from the binary file (exported from Animotive)
            for (var frame = startFrame;
                 frame <= lastFrame;
                 frame++)
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

                        pathAndKeyframesDictionary[relativePath]
                            .Add(new List<Keyframe>(lastFrame)); //0
                        pathAndKeyframesDictionary[relativePath]
                            .Add(new List<Keyframe>(lastFrame)); //1
                        pathAndKeyframesDictionary[relativePath]
                            .Add(new List<Keyframe>(lastFrame)); //2
                        pathAndKeyframesDictionary[relativePath]
                            .Add(new List<Keyframe>(lastFrame)); //3
                        pathAndKeyframesDictionary[relativePath]
                            .Add(new List<Keyframe>(lastFrame)); //4
                        pathAndKeyframesDictionary[relativePath]
                            .Add(new List<Keyframe>(lastFrame)); //5
                        pathAndKeyframesDictionary[relativePath]
                            .Add(new List<Keyframe>(lastFrame)); //6
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
                        var position = localTransformValuesFromAnimFile[pair.Key][frame].Position;
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
        public static void HandleBodyAnimationClipOperations(IT_SceneInternalData sceneData, string clipsPath,
            IT_FbxData fbxData)
        {
            var clipsTypeByPath = GetClipsPathByType(sceneData, clipsPath);

            var transformClipDatas = clipsTypeByPath.Where(a => a.Key == IT_ClipType.TransformClip).Select(a => a.Value)
                .ToList();

            for (var i = 0; i < transformClipDatas.Count; i++)
            {
                var boneCount = CalculateBoneCount(fbxData);

                var clipData = transformClipDatas[i];
                var animationClipDataPath = clipData.animationClipDataPath;


                var baseBodyPathWithNameWithoutExtension = string.Concat(
                    IT_AnimotiveImporterEditorConstants.BodyAnimationDirectory,
                    Path.GetFileNameWithoutExtension(animationClipDataPath));

                bodyAnimationPath = string.Concat(baseBodyPathWithNameWithoutExtension, ".anim");

                bodyAnimatorPath = string.Concat(baseBodyPathWithNameWithoutExtension, ".controller");

                bodyAnimationName = Path.GetFileName(animationClipDataPath);

                var clipAndDictionariesTuple = PrepareAndGetAnimationData(fbxData, animationClipDataPath);


                if (!IsBoneCountMatchWithTheClipData(clipAndDictionariesTuple, boneCount))
                {
                    EditorUtility.DisplayDialog(
                        IT_AnimotiveImporterEditorConstants.WarningTitle + " Can't create animation",
                        "Bone count with the character and animation clip doesn't match ! Make sure that you're using the correct character and clip data",
                        "OK");
                    continue;
                }

                IT_AnimotiveImporterEditorUtilities
                    .DeleteAssetIfExists(bodyAnimationPath,
                        typeof(AnimationClip));

                var animationClip =
                    CreateTransformMovementsAnimationClip(clipAndDictionariesTuple, fbxData.FbxGameObject,
                        clipData);

                var animatorController =
                    AnimatorController.CreateAnimatorControllerAtPathWithClip(bodyAnimatorPath, animationClip);


                fbxData.FbxAnimator.runtimeAnimatorController = animatorController;

                var animationGroup = new IT_AnimotiveImporterEditorGroupInfo(
                    bodyAnimationName, fbxData.FbxGameObject
                );
                var groupInfos = new List<IT_AnimotiveImporterEditorGroupInfo>(1);

                groupInfos.Add(animationGroup);


                IT_AnimotiveImporterEditorTimeline.HandleGroups(groupInfos);
            }


            AssetDatabase.Refresh();
        }


        private static Dictionary<IT_ClipType, IT_ClipData> GetClipsPathByType(IT_SceneInternalData sceneData,
            string clipsPath)
        {
            var clipPathsByType = new Dictionary<IT_ClipType, IT_ClipData>();

            foreach (var groupData in sceneData.groupDataById.Values)
            {
                foreach (var entityId in groupData.entitiesIds)
                {
                    var entityData = sceneData.entitiesDataBySerializedId[entityId];
                    //Here you can read the entity data that belongs to this group, such as its animation clips per take
                    for (var i = 0; i < entityData.clipsByTrackByTakeIndex.Count; i++)
                    {
                        var takes = entityData.clipsByTrackByTakeIndex[i];

                        for (var j = 0; j < takes.Count; j++)
                        {
                            var tracks = takes[j];

                            for (var k = 0; k < tracks.Count; k++)
                            {
                                var clip = tracks[k];

                                var animationClipDataPath =
                                    IT_AnimotiveImporterEditorUtilities.ReturnClipDataFromPath(clipsPath,
                                        clip.clipName);

                                var type =
                                    IT_AnimotiveImporterEditorUtilities.GetClipTypeFromClipName(clip.clipName);

                                var clipdata = new IT_ClipData(clip, animationClipDataPath);

                                clipPathsByType.Add(type, clipdata);
                            }
                        }
                    }
                }
            }

            return clipPathsByType;
        }

        private static int CalculateBoneCount(IT_FbxData fbxData)
        {
            var boneCount = 1; //lastbone count


            var temp = Enum.GetValues(typeof(HumanBodyBones));
            foreach (HumanBodyBones humanBodyBone in temp)
            {
                if (humanBodyBone == HumanBodyBones.LastBone) continue;

                var tr = fbxData.FbxAnimator.GetBoneTransform(humanBodyBone);
                if (tr != null) boneCount++;
            }

            return boneCount;
        }

        private static bool IsBoneCountMatchWithTheClipData(IT_ClipByDictionaryTuple clipAndDictionariesTuple,
            int boneCount)
        {
            var clip = clipAndDictionariesTuple.Clip;
            var totalFrameCount = clip.lengthInFrames;


            var recorderFramesMultipliedByBone = clip.physicsKeyframesCurve0.Length;
            var formula = totalFrameCount * boneCount + boneCount;

            return formula - boneCount == recorderFramesMultipliedByBone;
        }

        #endregion
    }
}