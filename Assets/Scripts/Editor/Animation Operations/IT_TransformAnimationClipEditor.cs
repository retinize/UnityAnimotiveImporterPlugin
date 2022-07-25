using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AnimotiveImporterDLL;
using OdinSerializer;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    public static class IT_TransformAnimationClipEditor
    {
        private static string bodyAnimationPath = "";
        public static string bodyAnimatorPath = "";
        public static string bodyAnimationName = "";

        #region Dictionary Operations

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
                var humanBodyBone = (HumanBodyBones)usedHumanoidBones[i];
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
        /// <param name="clipData"></param>
        private static AnimationClip CreateTransformMovementsAnimationClip(
            IT_ClipByDictionaryTuple clipAndDictionariesTuple,
            GameObject characterRoot, IT_ClipData clipData)
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
                GetLocalTransformValuesFromAnimFile(clip, transformsByHumanBodyBones, clipData);


            var globalQuaternionsByFrame =
                GetGlobalRotationsFromAnimFile(transformsByHumanBodyBones, humanBodyBonesBytransforms,
                    localTransformValuesFromAnimFile, clip, clipData);

            //HARDCODE !
            var path = string.Concat(Directory.GetCurrentDirectory(),
                @"\Assets\AnimotivePluginExampleStructure\Example Data\Animation\Poses");


            var editorTPoseText = File.ReadAllText(string.Concat(path, "\\EditorTPoseFrank.json"));


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

                        var editorTPoseList = editorTPoseTransformInfoList.TransformsByStrings
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
        public static async Task HandleBodyAnimationClipOperations(IT_SceneInternalData sceneData, string clipsPath)
        {
            var transformGroupDatas = GetClipsPathByType(sceneData, clipsPath);
            var groupInfos = new Dictionary<int, List<IT_AnimotiveImporterEditorGroupMemberInfo>>();

            var fbxDatasAndHoldersTuples = GetFbxDataAndHolders(transformGroupDatas);


            for (var i = 0; i < transformGroupDatas.Count; i++)
            {
                var groupData = transformGroupDatas[i];

                groupInfos.Add(groupData.serializedId, null);

                bodyAnimatorPath =
                    string.Concat(
                        string.Concat(IT_AnimotiveImporterEditorConstants.BodyAnimationDirectory, groupData.GroupName),
                        ".controller");

                var animatorController = AnimatorController.CreateAnimatorControllerAtPath(bodyAnimatorPath);


                foreach (var pair in groupData.TakeDatas)
                {
                    var takeData = pair.Value;
                    for (var k = 0; k < takeData.ClipDatas.Count; k++)
                    {
                        var clipData = takeData.ClipDatas[k];
                        if (clipData.Type != IT_ClipType.TransformClip) continue;

                        var fbxDataTuple = fbxDatasAndHoldersTuples[clipData.ModelName];
                        var fbxData = fbxDataTuple.FbxData;

                        var holderObject = fbxDataTuple.HolderObject;

                        fbxData.FbxAnimator.runtimeAnimatorController = animatorController;
                        // var boneCount = CalculateBoneCount(fbxData);

                        var animationClipDataPath = clipData.animationClipDataPath;
                        //
                        //
                        // var baseBodyPathWithNameWithoutExtension = string.Concat(
                        //     IT_AnimotiveImporterEditorConstants.BodyAnimationDirectory,
                        //     Path.GetFileNameWithoutExtension(animationClipDataPath));

                        bodyAnimationPath =
                            IT_AnimotiveImporterEditorUtilities
                                .GetBodyAnimationAssetDatabasePath(animationClipDataPath);

                        bodyAnimationName = Path.GetFileName(animationClipDataPath);

                        var clipAndDictionariesTuple = PrepareAndGetAnimationData(fbxData, animationClipDataPath);


                        fbxData.FbxGameObject.transform.localScale =
                            clipAndDictionariesTuple.Clip.lossyScaleRoot *
                            Vector3.one; // since the character has no parent 
                        // we can safely assign lossy scale data to character's root

                        AssignWorldPositionAndRotationToHolder(holderObject, clipAndDictionariesTuple.Clip);

                        if (!IsBoneCountMatchWithTheClipData(clipAndDictionariesTuple,
                                clipAndDictionariesTuple.Clip.humanoidBonesEnumThatAreUsed.Length))
                        {
                            var message =
                                $@"Bone count with the {clipData.ModelName} and {clipData.ClipPlayerData.clipName} doesn't match !
 Make sure that you're using the correct character and clip data";
                            EditorUtility.DisplayDialog(
                                IT_AnimotiveImporterEditorConstants.WarningTitle + " Can't create animation", message,
                                "OK");

                            continue;
                        }


                        IT_AnimotiveImporterEditorUtilities
                            .DeleteAssetIfExists(bodyAnimationPath,
                                typeof(AnimationClip));

                        var animationClip =
                            CreateTransformMovementsAnimationClip(clipAndDictionariesTuple, fbxData.FbxGameObject,
                                clipData);


                        animatorController.AddMotion(animationClip);


                        if (groupInfos[groupData.serializedId] == null)
                        {
                            groupInfos[groupData.serializedId] =
                                new List<IT_AnimotiveImporterEditorGroupMemberInfo>();
                        }

                        var animationGroupMemberInfo =
                            new IT_AnimotiveImporterEditorGroupMemberInfo(groupData.GroupName,
                                groupData.serializedId,
                                bodyAnimationName,
                                fbxData.FbxGameObject, bodyAnimationPath, clipData);
                        groupInfos[groupData.serializedId].Add(animationGroupMemberInfo);
                    }
                }
            }


            foreach (var pair in fbxDatasAndHoldersTuples)
            {
                pair.Value.FbxData.FbxGameObject.transform.SetParent(pair.Value.HolderObject.transform);
                pair.Value.FbxData.FbxAnimator.avatar = null;
            }

            IT_AnimotiveImporterEditorTimeline.HandleGroups(transformGroupDatas, fbxDatasAndHoldersTuples, sceneData);

            AssetDatabase.Refresh();
        }


        private static List<IT_GroupData> GetClipsPathByType(IT_SceneInternalData sceneData,
            string clipsPath)
        {
            var groupDatas = new List<IT_GroupData>();

            foreach (var groupData in sceneData.groupDataById.Values)
            {
                var readerGroupData = new IT_GroupData(groupData.serializedId, groupData.groupName);
                foreach (var entityId in groupData.entitiesIds)
                {
                    var entityData = sceneData.entitiesDataBySerializedId[entityId];
                    for (var i = 0; i < entityData.clipsByTrackByTakeIndex.Count; i++)
                    {
                        var take = entityData.clipsByTrackByTakeIndex[i];

                        if (!readerGroupData.TakeDatas.ContainsKey(i))
                            readerGroupData.TakeDatas.Add(i, new IT_TakeData(i));

                        for (var j = 0; j < take.Count; j++)
                        {
                            var track = take[j];

                            for (var k = 0; k < track.Count; k++)
                            {
                                var clip = track[k];

                                var animationClipDataPath =
                                    IT_AnimotiveImporterEditorUtilities.ReturnClipDataFromPath(clipsPath,
                                        clip.clipName);

                                var type =
                                    IT_AnimotiveImporterEditorUtilities.GetClipTypeFromClipName(clip.clipName);

                                var clipdata = new IT_ClipData(type, clip, animationClipDataPath,
                                    entityData.entityInstantiationTokenData, i);

                                readerGroupData.TakeDatas[i].ClipDatas.Add(clipdata);
                            }
                        }
                    }
                }

                readerGroupData.TakeDatas = readerGroupData.TakeDatas.Where(a => a.Value.ClipDatas.Count != 0)
                    .ToDictionary(p => p.Key, p => p.Value);

                groupDatas.Add(readerGroupData);
            }

            return groupDatas;
        }

        private static int CalculateBoneCount(IT_FbxData fbxData)
        {
            var boneCount = 1;


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
            var formula = totalFrameCount * boneCount;

            return formula <= recorderFramesMultipliedByBone;
        }


        private static Dictionary<string, IT_FbxDatasAndHoldersTuple> GetFbxDataAndHolders(
            List<IT_GroupData> transformGroupDatas)
        {
            var fbxDatasAndHoldersTuples = new Dictionary<string, IT_FbxDatasAndHoldersTuple>();
            for (var i = 0; i < transformGroupDatas.Count; i++)
            {
                var groupdata = transformGroupDatas[i];


                foreach (var pair in groupdata.TakeDatas)
                {
                    var takeData = pair.Value;
                    for (var k = 0; k < takeData.ClipDatas.Count; k++)
                    {
                        var clipData = takeData.ClipDatas[k];

                        if (fbxDatasAndHoldersTuples.ContainsKey(clipData.ModelName)) continue;
                        var files = Directory.GetDirectories(Path.Combine(
                            IT_AnimotiveImporterEditorWindow.UserChosenDirectoryToImportUnityExports, "EntityAssets",
                            "Characters"));

                        files = files.Where(a => a.EndsWith(clipData.ModelName)).ToArray();
                        var modelDirectory = files[0];
                        var fbxes = Directory.GetFiles(modelDirectory)
                            .Where(a => a.Substring(a.Length - 4, 4).ToLower().EndsWith(".fbx")).ToArray();

                        var fullOsPathToFbx = fbxes[0];
                        var modelName = Path.GetFileName(fullOsPathToFbx);

                        var pathToFbx = Path.Combine(
                            IT_AnimotiveImporterEditorWindow.ImportedCharactersAssetdatabaseDirectory
                            , modelName);
                        // var pathToFbx = fbxes[0];

                        pathToFbx =
                            IT_AnimotiveImporterEditorUtilities.GetImportedFbxAssetDatabasePathVariable(pathToFbx);

                        var fbxData = IT_AnimotiveImporterEditorUtilities.LoadFbx(pathToFbx);
                        var holderObject = new GameObject(string.Concat(fbxData.FbxGameObject.name, "_HOLDER"));
                        var temp = new IT_FbxDatasAndHoldersTuple(fbxData, holderObject);
                        fbxDatasAndHoldersTuples.Add(clipData.ModelName, temp);
                    }
                }
            }

            return fbxDatasAndHoldersTuples;
        }


        private static void AssignWorldPositionAndRotationToHolder(GameObject holderObject,
            IT_CharacterTransformAnimationClip clip)
        {
            holderObject.transform.position = clip.worldPositionHolder;
            holderObject.transform.rotation = clip.worldRotationHolder;
        }

        #endregion
    }
}