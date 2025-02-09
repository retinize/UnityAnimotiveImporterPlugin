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
        public static async void HandleGroups(List<IT_GroupData> transformGroupDatas,
            Dictionary<string, IT_FbxDatasAndHoldersTuple> fbxDatasAndHoldersTuples)
        {
            if (!Directory.Exists(IT_AnimotiveImporterEditorConstants.UnityFilesPlayablesDirectory))
                Directory.CreateDirectory(IT_AnimotiveImporterEditorConstants.UnityFilesPlayablesDirectory);

            for (var i = 0; i < transformGroupDatas.Count; i++)
            {
                var groupData = transformGroupDatas[i];

                var groupObject = new GameObject(groupData.TrimmedGroupName);

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

                    playableDirector.playableAsset = await CreatePlayableAssets(timelineData);
                }
            }
        }

        /// <summary>
        ///     Creates a playable asset, tracks and clips and binds them to the given gameObject
        /// </summary>
        /// <param name="timelineData"></param>
        /// <returns></returns>
        public static async Task<PlayableAsset> CreatePlayableAssets(IT_TimelineData timelineData)
        {
            var playableDirector = timelineData.PlayableDirector;

            var assetName = string.Concat(timelineData.GroupName, "_Take",
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

            //create tracks
            for (var i = 0; i < timelineData.ClipClustersInTake.Count; i++)
            {
                var clipCluster = timelineData.ClipClustersInTake[i];
                if (clipCluster.IsAnimationProcessInterrupted)
                {
                    AssetDatabase.DeleteAsset(assetPath);
                    continue;
                }

                //TODO: Will be updated to add facial animations as well

                var groupTrack = asset.CreateTrack<GroupTrack>();
                groupTrack.name = timelineData.GroupName;

                var objToBind = timelineData.FbxDataWithHolders[clipCluster.ModelName].FbxData.FbxGameObject;
                var bodyAnimationPath = IT_AnimotiveImporterEditorUtilities.ConvertFullFilePathIntoUnityFilesPath(
                    IT_AnimotiveImporterEditorConstants.UnityFilesBodyAnimationDirectory,
                    clipCluster.BodyAnimationClipData.ClipDataPath,
                    IT_AnimotiveImporterEditorConstants.AnimationExtension);

                if (AssetDatabase.LoadAssetAtPath<AnimationClip>(bodyAnimationPath) == null) continue;

                // FACIAL ANIMATION 

                if (!string.IsNullOrEmpty(clipCluster.FacialAnimationClipData.ClipDataPath))
                {
                    var facialAnimationClipData = clipCluster.FacialAnimationClipData;
                    var facialAnimationPath = IT_AnimotiveImporterEditorUtilities.ConvertFullFilePathIntoUnityFilesPath(
                        IT_AnimotiveImporterEditorConstants.UnityFilesFacialAnimationDirectory,
                        clipCluster.FacialAnimationClipData.ClipDataPath,
                        IT_AnimotiveImporterEditorConstants.AnimationExtension);
                    var clipName = Path.GetFileNameWithoutExtension(facialAnimationClipData.ClipDataPath);
                    CreateAnimationTrack(clipName, asset, groupTrack, facialAnimationPath, playableDirector,
                        objToBind, IT_AnimotiveImporterEditorConstants.UnityFilesFacialAnimationDirectory);
                }

                // END OF FACIAL ANIMATION

                var bodyAnimationClipData = clipCluster.BodyAnimationClipData;
                CreateAnimationTrack(bodyAnimationClipData.ClipPlayerData.clipName, asset, groupTrack,
                    bodyAnimationPath,
                    playableDirector,
                    objToBind, IT_AnimotiveImporterEditorConstants.UnityFilesBodyAnimationDirectory);
                // CreateAnimationTrack(); //facial animation
                CreateAudioTrack(asset, groupTrack, clipCluster.AudioClipData, playableDirector,
                    objToBind);
            }


            AssetDatabase.Refresh();

            var playableAsset =
                AssetDatabase.LoadAssetAtPath(assetPath, typeof(PlayableAsset)) as PlayableAsset;


            return playableAsset;
        }

        /// <summary>
        ///     Creates animation track in the given group track.
        /// </summary>
        /// <param name="clipName"></param>
        /// <param name="asset">Timeline asset to create track in</param>
        /// <param name="groupTrack">group to assign created animation track</param>
        /// <param name="animationPath">Path to animation clip</param>
        /// <param name="playableDirector">Playable director to bind this track to. </param>
        /// <param name="objToBind">Game object in the scene to bind animation clip to</param>
        /// <param name="directory"></param>
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
        private static void CreateAudioTrack(TimelineAsset asset, GroupTrack groupTrack,
            IT_ClipData<IT_ClipPlayerData> audioClipData,
            PlayableDirector playableDirector, GameObject objToBind)
        {
            var clipFullName = string.Concat(audioClipData.ClipDataPath, audioClipData.FileExtension);

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