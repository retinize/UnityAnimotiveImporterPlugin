using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AnimotiveImporterDLL;
using OdinSerializer;
using UnityEditor;
using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    /// <summary>
    ///     Class which is responsible for creating and handling animation clips for body.
    /// </summary>
    public static class IT_BodyAnimationClipEditor
    {
        #region Dictionary Operations for Animation

        /// <summary>
        ///     Creates and returns two dictionary where 'HumanBodyBones' and 'Transform' types are key/value and vice versa.
        /// </summary>
        /// <param name="animator">Animator of the character</param>
        /// <param name="characterRoot">Root GameObject of the character</param>
        /// <param name="usedHumanoidBones">Array that has the bone indexes used in the clip</param>
        /// <returns></returns>
        private static IT_DictionaryTuple GetBoneTransformDictionaries(Animator animator, GameObject characterRoot,
            int[] usedHumanoidBones)
        {
            var humanBodyBonesByTransforms =
                new Dictionary<HumanBodyBones, Transform>(55);

            var transformsByHumanBodyBones =
                new Dictionary<Transform, HumanBodyBones>(55);

            for (var i = 0; i < usedHumanoidBones.Length; i++)
            {
                var humanBodyBone = (HumanBodyBones) usedHumanoidBones[i];
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
            IT_CharacterTransformAnimationClip clip)
        {
            var globalQuaternionsByFrame =
                new Dictionary<HumanBodyBones, List<Quaternion>>(55);

            var startFrame = 0;
            var lastFrame = clip.lastFrameInTimelineWhenItWasCaptured - clip.startFrameInTimelineWhenItWasCaptured;

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
                Dictionary<HumanBodyBones, Transform> transformsByHumanBoneName)
        {
            var localQuaternionsByFrame = new Dictionary<HumanBodyBones, List<IT_TransformValues>>(55);

            var startFrame = 0;
            var lastFrame = clip.lastFrameInTimelineWhenItWasCaptured - clip.startFrameInTimelineWhenItWasCaptured;


            for (var frame = startFrame;
                 frame <= lastFrame;
                 frame++)
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
                GetBoneTransformDictionaries(animator, loadedFbXofCharacter.FbxGameObject,
                    clip.humanoidBonesEnumThatAreUsed);

            // animator.avatar = null;
            AssetDatabase.Refresh();
            return new IT_ClipByDictionaryTuple(clip, boneTransformDictionaries);
        }

        /// <summary>
        ///     Creates the Animation clip according to the given data contains clip info read from json and
        ///     BoneTransformDictionaries
        /// </summary>
        /// <param name="clipAndDictionariesTuple">Contains clip info read from json and BoneTransformDictionaries</param>
        /// <param name="characterRoot">Root gameObject of the character to apply animation to.</param>
        /// <param name="editorTPose">TPose data generated by the PoseManager</param>
        /// <param name="bodyAnimationPath">Full path to create animation clip at</param>
        private static void CreateTransformMovementsAnimationClip(
            IT_ClipByDictionaryTuple clipAndDictionariesTuple,
            GameObject characterRoot, IT_TransformInfoList editorTPose, string bodyAnimationPath)
        {
            var clip = clipAndDictionariesTuple.Clip;
            var transformsByHumanBodyBones =
                clipAndDictionariesTuple.DictTuple.TransformsByHumanBodyBones;

            var humanBodyBonesBytransforms =
                clipAndDictionariesTuple.DictTuple.HumanBodyBonesByTransform;

            var animationClip = new AnimationClip();

            // this dictionary is used to store the values of bones at every frame to be applied to animation clip later.
            var pathAndKeyframesDictionary =
                new Dictionary<string, List<List<Keyframe>>>();


            var localTransformValuesFromAnimFile =
                GetLocalTransformValuesFromAnimFile(clip, transformsByHumanBodyBones);


            var globalQuaternionsByFrame =
                GetGlobalRotationsFromAnimFile(transformsByHumanBodyBones, humanBodyBonesBytransforms,
                    localTransformValuesFromAnimFile, clip);

            var editorTPoseTransformInfoList = editorTPose;


            var startFrame = 0;
            var lastFrame = clip.lastFrameInTimelineWhenItWasCaptured - clip.startFrameInTimelineWhenItWasCaptured;

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
                        pathAndKeyframesDictionary.Add(relativePath, new List<List<Keyframe>>());

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

                        var editorTPoseList = editorTPoseTransformInfoList.transformsByStrings
                            .Where(a => a.Name == pair.Key).ToList();

                        var editorTPoseGlobalRotationForThisBone = Quaternion.identity;

                        if (editorTPoseList.Count != 0)
                            editorTPoseGlobalRotationForThisBone = editorTPoseList[0].GlobalRotation;


                        var animotiveTPoseGlobalRotationForThisBone = Quaternion.identity;

                        if (clip.sourceCharacterTPoseRotationInRootLocalSpaceByHumanoidBone.ContainsKey(pair.Key))
                        {
                            animotiveTPoseGlobalRotationForThisBone =
                                clip.sourceCharacterTPoseRotationInRootLocalSpaceByHumanoidBone[pair.Key];
                        }

                        var invAnimotiveTPoseRotationThisBone =
                            Quaternion.Inverse(animotiveTPoseGlobalRotationForThisBone);

                        var boneRotation = invAnimotiveTPoseRotationThisBone *
                                           boneGlobalRotationThisFrameFromAnimFile *
                                           editorTPoseGlobalRotationForThisBone;

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

                    HumanBodyBones[] positionAllowedBones =
                        {HumanBodyBones.Hips, HumanBodyBones.LastBone};

                    //add the position of selected bones to animationclip
                    if (positionAllowedBones.Any(a => a == pair.Key))
                    {
                        var position = localTransformValuesFromAnimFile[pair.Key][frame].Position;
                        // var inversedPosition = pair.Value.InverseTransformPoint(position);

                        var localPositionX = new Keyframe(time, position.x);
                        var localPositionY = new Keyframe(time, position.y);
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

            var fullOsPathToBodyAnim =
                IT_AnimotiveImporterEditorUtilities.ConvertAssetDatabasePathToSystemPath(bodyAnimationPath);

            if (File.Exists(fullOsPathToBodyAnim))
            {
                var assetDatabaseDir = Path.GetDirectoryName(
                    IT_AnimotiveImporterEditorUtilities.ConvertAssetDatabasePathToSystemPath(bodyAnimationPath));
                var similarFileName = IT_AnimotiveImporterEditorUtilities.GetLatestSimilarFileName(assetDatabaseDir,
                    fullOsPathToBodyAnim,
                    Path.GetFileName(fullOsPathToBodyAnim), IT_AnimotiveImporterEditorConstants.AnimationExtension);

                bodyAnimationPath =
                    IT_AnimotiveImporterEditorUtilities.ConvertSystemPathToAssetDatabasePath(similarFileName);
            }

            AssetDatabase.CreateAsset(animationClip, bodyAnimationPath);


            AssetDatabase.Refresh();
        }

        /// <summary>
        ///     Checks directories of animation clips, gets group datas, checks if animation clip is apliable to the character
        ///     given (shows a display dialog if not), gets fbx data and holders and assigns world position and rotations to
        ///     holders.
        /// </summary>
        /// <param name="groupDatas"></param>
        /// <param name="fbxDatasAndHoldersTuples"></param>
        /// <returns>Tuple of list of group data and dictionary with fbx datas tuple</returns>
        public static async Task HandleBodyAnimationClipOperations(
            List<IT_GroupData> groupDatas,
            Dictionary<string, IT_FbxDatasAndHoldersTuple> fbxDatasAndHoldersTuples)
        {
            string[] animationDirectories =
            {
                IT_AnimotiveImporterEditorConstants.UnityFilesAnimationDirectory,
                IT_AnimotiveImporterEditorConstants.UnityFilesBodyAnimationDirectory,
                IT_AnimotiveImporterEditorConstants.UnityFilesFacialAnimationDirectory
            };

            for (var i = 0; i < animationDirectories.Length; i++)
            {
                if (!Directory.Exists(animationDirectories[i])) Directory.CreateDirectory(animationDirectories[i]);
            }

            for (var i = 0; i < groupDatas.Count; i++)
            {
                var groupData = groupDatas[i];


                foreach (var pair in groupData.TakeDatas)
                {
                    var takeData = pair.Value;

                    for (var k = 0; k < takeData.Clusters.Count; k++)
                    {
                        var clipCluster = takeData.Clusters[k];

                        var fbxDataTuple = fbxDatasAndHoldersTuples[clipCluster.ModelName];
                        var fbxData = fbxDataTuple.FbxData;

                        var holderObject = fbxDataTuple.HolderObject;

                        var animationClipDataPath = clipCluster.BodyAnimationClipData.ClipDataPath;


                        var bodyAnimationPath =
                            IT_AnimotiveImporterEditorUtilities
                                .ConvertFullFilePathIntoUnityFilesPath(
                                    IT_AnimotiveImporterEditorConstants.UnityFilesBodyAnimationDirectory,
                                    animationClipDataPath, IT_AnimotiveImporterEditorConstants.AnimationExtension);

                        var clipAndDictionariesTuple =
                            PrepareAndGetAnimationData(fbxData, animationClipDataPath);


                        var isAvatarHasAllRequiredBones =
                            clipAndDictionariesTuple.DictTuple.HumanBodyBonesByTransform.Count !=
                            clipAndDictionariesTuple.Clip.humanoidBonesEnumThatAreUsed.Length;

                        if (isAvatarHasAllRequiredBones)
                        {
                            var message =
                                $@" Bone count in the '{fbxData.FbxAnimator.avatar.name}' avatar and the used bones count in '{clipCluster.BodyAnimationClipData.ClipPlayerData.clipName}' clip don't match !
  Make sure that the avatar has all the required bones and you're using correct FBX for this clip";

                            EditorUtility.DisplayDialog(
                                IT_AnimotiveImporterEditorConstants.WarningTitle + " Can't create animation",
                                message,
                                "OK");

                            clipCluster.SetInterruptionValue(true);
                            continue;
                        }

                        clipCluster.NumberOfCaptureInWhichItWasCaptured =
                            clipAndDictionariesTuple.Clip.numberOfCaptureInWhichItWasCaptured;

                        fbxData.FbxGameObject.transform.localScale =
                            clipAndDictionariesTuple.Clip.lossyScaleRoot *
                            Vector3.one; // since the character has no parent at this point
                        // we can safely assign lossy scale data to character's root


                        holderObject.transform.position = clipAndDictionariesTuple.Clip.worldPositionHolder;
                        holderObject.transform.rotation = clipAndDictionariesTuple.Clip.worldRotationHolder;

                        if (!IsBoneCountMatchWithTheClipData(clipAndDictionariesTuple,
                                clipAndDictionariesTuple.Clip.humanoidBonesEnumThatAreUsed.Length))
                        {
                            var message =
                                $@"Bone count with the {clipCluster.ModelName} and {clipCluster.BodyAnimationClipData.ClipPlayerData.clipName} doesn't match !
 Make sure that you're using the correct character and clip data";
                            EditorUtility.DisplayDialog(
                                IT_AnimotiveImporterEditorConstants.WarningTitle + " Can't create animation",
                                message,
                                "OK");
                            clipCluster.SetInterruptionValue(true);

                            continue;
                        }


                        CreateTransformMovementsAnimationClip(clipAndDictionariesTuple,
                            fbxData.FbxGameObject, fbxDataTuple.EditorTPose, bodyAnimationPath);
                    }
                }
            }


            foreach (var pair in fbxDatasAndHoldersTuples)
            {
                pair.Value.FbxData.FbxGameObject.transform.SetParent(pair.Value.HolderObject.transform);
                pair.Value.FbxData.FbxAnimator.avatar = null;
            }


            AssetDatabase.Refresh();
        }

        /// <summary>
        ///     Checks if the bone count used in the animation clip matches with the character
        /// </summary>
        /// <param name="clipAndDictionariesTuple"></param>
        /// <param name="boneCount"></param>
        /// <returns></returns>
        private static bool IsBoneCountMatchWithTheClipData(IT_ClipByDictionaryTuple clipAndDictionariesTuple,
            int boneCount)
        {
            var clip = clipAndDictionariesTuple.Clip;
            var totalFrameCount = clip.lengthInFrames;


            var recorderFramesMultipliedByBone = clip.physicsKeyframesCurve0.Length;
            var formula = totalFrameCount * boneCount;

            return formula <= recorderFramesMultipliedByBone;
        }

        #endregion
    }
}