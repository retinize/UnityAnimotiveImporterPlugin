using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AnimotiveImporterDLL;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Retinize.Editor.AnimotiveImporter
{
    public static class IT_AnimotiveImporterEditorTimeline
    {
        /// <summary>
        ///     Creates the scene objects according to given group info and  creates&assigns them to their respective playable
        ///     asset.
        /// </summary>
        /// <param name="transformGroupDatas">List of groupDatas which includes clusters and take datas inside them</param>
        /// <param name="fbxDatasAndHoldersTuples">Imported FBX datas and their holders in the scene</param>
        /// <param name="sceneInternalData">Binary scene data.</param>
        public static async Task HandleTimeLineOperations(List<IT_GroupData> transformGroupDatas,
            Dictionary<string, IT_FbxDatasAndHoldersTuple> fbxDatasAndHoldersTuples,
            IT_SceneInternalData sceneInternalData)
        {
            if (!Directory.Exists(IT_AnimotiveImporterEditorConstants.UnityFilesPlayablesDirectory))
                Directory.CreateDirectory(IT_AnimotiveImporterEditorConstants.UnityFilesPlayablesDirectory);

            for (var i = 0; i < transformGroupDatas.Count; i++)
            {
                var groupData = transformGroupDatas[i];

                var groupObject = groupData.GroupHeadInScene;

                foreach (var pair in groupData.TakeDatas)
                {
                    var takeData = pair.Value;
                    var takeNumberOnUI = pair.Key + 1;
                    var takeNameInScene = string.Concat("Take_", takeNumberOnUI.ToString());

                    var takeObjectInScene = new GameObject(takeNameInScene);
                    takeObjectInScene.transform.SetParent(groupObject.transform);
                    var playableDirector = takeObjectInScene.AddOrGetComponent<PlayableDirector>();


                    var timelineData = new IT_TimelineData(groupData.TrimmedGroupName, takeData.Clusters,
                        playableDirector,
                        fbxDatasAndHoldersTuples, takeData);

                    playableDirector.playableAsset = await CreatePlayableAssets(timelineData, sceneInternalData);
                }
            }
        }

        /// <summary>
        ///     Creates a playable asset, tracks and clips and binds them to the given gameObject
        /// </summary>
        /// <param name="timelineData"></param>
        /// <param name="sceneInternalData"></param>
        /// <returns></returns>
        public static async Task<PlayableAsset> CreatePlayableAssets(IT_TimelineData timelineData,
            IT_SceneInternalData sceneInternalData)
        {
            var playableDirector = timelineData.PlayableDirector;

            var assetName = string.Concat(sceneInternalData.currentSetName, "_", timelineData.GroupName, "_Take",
                timelineData.TakeData.TakeIndex);

            var assetPath = Path.Combine(IT_AnimotiveImporterEditorConstants.UnityFilesPlayablesDirectory,
                string.Concat(assetName, IT_AnimotiveImporterEditorConstants.PlayableExtension)
            );

            assetPath = IT_AnimotiveImporterEditorUtilities.ConvertSystemPathToAssetDatabasePath(assetPath);

            var asset = ScriptableObject.CreateInstance<TimelineAsset>();

            var fullOsPath = IT_AnimotiveImporterEditorUtilities.ConvertAssetDatabasePathToSystemPath(assetPath);
            var assetPathDir = Path.GetDirectoryName(fullOsPath);

            if (File.Exists(fullOsPath))
            {
                var similarName = await IT_AnimotiveImporterEditorUtilities.GetLatestSimilarFileName(assetPathDir,
                    fullOsPath, Path.GetFileName(fullOsPath), IT_AnimotiveImporterEditorConstants.PlayableExtension);
                similarName = IT_AnimotiveImporterEditorUtilities.ConvertSystemPathToAssetDatabasePath(similarName);
                assetPath = similarName;
            }

            AssetDatabase.CreateAsset(asset, assetPath);

            var groupTrack = asset.CreateTrack<GroupTrack>();
            groupTrack.name = timelineData.GroupName;

            //create tracks
            for (var i = 0; i < timelineData.ClipClustersInTake.Count; i++)
            {
                var cluster = timelineData.ClipClustersInTake[i];


                if (cluster.ClusterType == IT_ClusterType.CharacterCluster)
                {
                    var clipCluster = (IT_CharacterCluster) timelineData.ClipClustersInTake[i];

                    if (clipCluster.IsAnimationProcessInterrupted)
                    {
                        AssetDatabase.DeleteAsset(assetPath);
                        continue;
                    }

                    var objToBind = timelineData.FbxDataWithHolders[clipCluster.EntityName].FbxData.FbxGameObject;

                    var bodyAnimationPath = IT_AnimotiveImporterEditorUtilities.ConvertFullFilePathIntoUnityFilesPath(
                        IT_AnimotiveImporterEditorConstants.UnityFilesBodyAnimationDirectory,
                        clipCluster.ClipDatas[IT_ClipType.TransformAnimationClip].ClipDataPath,
                        IT_AnimotiveImporterEditorConstants.AnimationExtension);

                    if (AssetDatabase.LoadAssetAtPath<AnimationClip>(bodyAnimationPath) == null) continue;


                    #region Facial Animation

                    if (!string.IsNullOrEmpty(clipCluster.FacialAnimationClipData.ClipDataPath))
                    {
                        var facialAnimationClipData = clipCluster.FacialAnimationClipData;
                        var facialAnimationPath =
                            IT_AnimotiveImporterEditorUtilities.ConvertFullFilePathIntoUnityFilesPath(
                                IT_AnimotiveImporterEditorConstants.UnityFilesFacialAnimationDirectory,
                                clipCluster.FacialAnimationClipData.ClipDataPath,
                                IT_AnimotiveImporterEditorConstants.AnimationExtension);
                        var clipName = Path.GetFileNameWithoutExtension(facialAnimationClipData.ClipDataPath);


                        CreateAnimationTrack(clipName, asset, groupTrack,
                            facialAnimationPath, playableDirector,
                            objToBind, IT_AnimotiveImporterEditorConstants.UnityFilesFacialAnimationDirectory);
                    }

                    #endregion

                    var bodyAnimationClipData = clipCluster.ClipDatas[IT_ClipType.TransformAnimationClip];

                    CreateAnimationTrack(bodyAnimationClipData.ClipPlayerData, asset, groupTrack,
                        bodyAnimationPath,
                        playableDirector,
                        objToBind, IT_AnimotiveImporterEditorConstants.UnityFilesBodyAnimationDirectory);

                    CreateAudioTrack(asset, groupTrack, clipCluster.ClipDatas[IT_ClipType.AudioClip].ClipDataPath,
                        playableDirector,
                        objToBind);
                }
                else
                {
                    var cameraCluster = (IT_CameraCluster) cluster;
                    var cameraAnimationClipData = cameraCluster.ClipDatas[IT_ClipType.PropertiesClip];
                    var objToBind = cameraCluster.ReferenceInScene.transform.parent.gameObject;

                    objToBind.AddOrGetComponent<Animator>();


                    CreateAnimationTrack(cameraAnimationClipData.ClipPlayerData, asset, groupTrack,
                        cameraCluster.PropertiesDataAnimationClipAssetDatabasePath,
                        playableDirector,
                        objToBind, IT_AnimotiveImporterEditorConstants.UnityFilesBodyAnimationDirectory);
                }
            }


            AssetDatabase.Refresh();

            var playableAsset =
                AssetDatabase.LoadAssetAtPath(assetPath, typeof(PlayableAsset)) as PlayableAsset;


            return playableAsset;
        }

        /// <summary>
        ///     Creates animation track in the given group track.
        /// </summary>
        /// <param name="clipData"></param>
        /// <param name="asset">Timeline asset to create track in</param>
        /// <param name="groupTrack">group to assign created animation track</param>
        /// <param name="animationPath">Path to animation clip</param>
        /// <param name="playableDirector">Playable director to bind this track to. </param>
        /// <param name="objToBind">Game object in the scene to bind animation clip to</param>
        /// <param name="directory"></param>
        private static async void CreateAnimationTrack(IT_ClipPlayerData clipData, TimelineAsset asset,
            GroupTrack groupTrack, string animationPath,
            PlayableDirector playableDirector, GameObject objToBind, string directory)
        {
            var fileName = await IT_AnimotiveImporterEditorUtilities.FindLatestFileName(clipData.clipName,
                directory, IT_AnimotiveImporterEditorConstants.AnimationExtension);

            if (!string.IsNullOrEmpty(fileName))
                animationPath = IT_AnimotiveImporterEditorUtilities.ConvertSystemPathToAssetDatabasePath(fileName);

            if (string.IsNullOrEmpty(animationPath)) return;
            var animationTrack = asset.CreateTrack<AnimationTrack>();
            animationTrack.SetGroup(groupTrack);

            var bodyAnimationClip =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(animationPath);

            var timelineClip = animationTrack.CreateClip(bodyAnimationClip);
            timelineClip.displayName =
                Path.GetFileNameWithoutExtension(Path.Combine(Directory.GetCurrentDirectory(), animationPath));

            var startSecond = clipData.startFrameInTimeline * IT_PhysicsManager.FixedDeltaTime;

            timelineClip.start = startSecond;

            playableDirector.SetGenericBinding(animationTrack, objToBind);
        }

        private static async void CreateAnimationTrack(string clipName, TimelineAsset asset,
            GroupTrack groupTrack, string animationPath,
            PlayableDirector playableDirector, GameObject objToBind, string directory)
        {
            var fileName = await IT_AnimotiveImporterEditorUtilities.FindLatestFileName(clipName,
                directory, IT_AnimotiveImporterEditorConstants.AnimationExtension);

            if (!string.IsNullOrEmpty(fileName))
                animationPath = IT_AnimotiveImporterEditorUtilities.ConvertSystemPathToAssetDatabasePath(fileName);

            if (string.IsNullOrEmpty(animationPath)) return;
            var animationTrack = asset.CreateTrack<AnimationTrack>();
            animationTrack.SetGroup(groupTrack);

            var bodyAnimationClip =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(animationPath);


            var timelineClip = animationTrack.CreateClip(bodyAnimationClip);
            timelineClip.displayName =
                Path.GetFileNameWithoutExtension(Path.Combine(Directory.GetCurrentDirectory(), animationPath));
            timelineClip.start = 0;

            playableDirector.SetGenericBinding(animationTrack, objToBind);
        }

        /// <summary>
        ///     Creates audio track in the given group track.
        /// </summary>
        /// <param name="asset">Timeline asset to create track in</param>
        /// <param name="groupTrack">group to assign created animation track</param>
        /// <param name="clipDataPath">Path to audio clip data(binary)</param>
        /// <param name="playableDirector">Playable director to bind this track to. </param>
        /// <param name="objToBind">Game object in the scene to bind animation clip to</param>
        private static void CreateAudioTrack(TimelineAsset asset, GroupTrack groupTrack, string clipDataPath,
            PlayableDirector playableDirector, GameObject objToBind)
        {
            var clipFullName = string.Concat(clipDataPath, IT_AnimotiveImporterEditorConstants.AudioExtension);

            var path = Path.Combine(IT_AnimotiveImporterEditorConstants.UnityFilesAudioDirectory,
                Path.GetFileName(clipFullName));
            path = IT_AnimotiveImporterEditorUtilities.ConvertSystemPathToAssetDatabasePath(path);

            var audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);

            objToBind.GetComponent<AudioSource>().clip = audioClip;
            var itSoundTrack = asset.CreateTrack<AudioTrack>();
            itSoundTrack.SetGroup(groupTrack);

            var soundClip = itSoundTrack.CreateClip(audioClip);
            soundClip.displayName = Path.GetFileNameWithoutExtension(clipFullName);
            playableDirector.SetGenericBinding(itSoundTrack, objToBind);
        }
    }
}