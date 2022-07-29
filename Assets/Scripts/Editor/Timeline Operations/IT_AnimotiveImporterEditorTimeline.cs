using System.Collections.Generic;
using System.IO;
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
        /// <param name="group">List of group info</param>
        public static void HandleGroups(List<IT_GroupData> transformGroupDatas,
            Dictionary<string, IT_FbxDatasAndHoldersTuple> fbxDatasAndHoldersTuples,
            IT_SceneInternalData sceneInternalData)
        {
            for (var i = 0; i < transformGroupDatas.Count; i++)
            {
                var groupData = transformGroupDatas[i];

                var groupObject = new GameObject(groupData.GroupName);

                foreach (var pair in groupData.TakeDatas)
                {
                    var takeData = pair.Value;

                    var takeNameInScene = string.Concat("Take_", pair.Key.ToString());

                    var takeObjectInScene = new GameObject(takeNameInScene);
                    takeObjectInScene.transform.SetParent(groupObject.transform);
                    var playableDirector = takeObjectInScene.AddOrGetComponent<PlayableDirector>();


                    var timelineData = new IT_TimelineData(groupData.GroupName, takeData.Clusters, playableDirector,
                        fbxDatasAndHoldersTuples, takeData);

                    playableDirector.playableAsset = CreatePlayableAssets(timelineData, sceneInternalData);
                }
            }
        }

        /// <summary>
        ///     Creates a playable asset, tracks and clips and binds them to the given gameObject
        /// </summary>
        /// <param name="timelineData"></param>
        /// <param name="sceneInternalData"></param>
        /// <returns></returns>
        public static PlayableAsset CreatePlayableAssets(IT_TimelineData timelineData,
            IT_SceneInternalData sceneInternalData)
        {
            var playableDirector = timelineData.PlayableDirector;

            var assetName = string.Concat(sceneInternalData.currentSetName, "_", timelineData.GroupName, "_Take",
                timelineData.TakeData.TakeIndex);

            var assetPath = string.Concat(IT_AnimotiveImporterEditorConstants.PlayablesCreationPath, assetName,
                ".playable");


            var asset = ScriptableObject.CreateInstance<TimelineAsset>();

            var fullOsPath = IT_AnimotiveImporterEditorUtilities.ConvertAssetDatabasePathToSystemPath(assetPath);
            var assetPathDir = Path.GetDirectoryName(fullOsPath);

            if (File.Exists(fullOsPath))
            {
                var similarName = IT_AnimotiveImporterEditorUtilities.GetLatestSimilarFileName(assetPathDir,
                    fullOsPath, Path.GetFileName(fullOsPath), "playable");
                similarName = IT_AnimotiveImporterEditorUtilities.ConvertSystemPathToAssetDatabasePath(similarName);
                assetPath = similarName;
            }

            AssetDatabase.CreateAsset(asset, assetPath);


            for (var i = 0; i < timelineData.ClipClustersInTake.Count; i++)
            {
                var clipCluster = timelineData.ClipClustersInTake[i];
                if (clipCluster.IsAnimationProcessInterrupted) continue;

                //TODO: Will be updated to add the audio clip and facial animations as well

                var groupTrack = asset.CreateTrack<GroupTrack>();
                groupTrack.name = timelineData.GroupName;

                var objToBind = timelineData.FbxDataWithHolders[clipCluster.ModelName].FbxData.FbxGameObject;
                var bodyAnimationPath = IT_AnimotiveImporterEditorUtilities.GetBodyAssetDatabasePath(
                    clipCluster.TransformClip.clipDataPath, "anim");

                if (AssetDatabase.LoadAssetAtPath<AnimationClip>(bodyAnimationPath) == null) continue;

                // TODO: REMOVE FACIAL ANIMATION PART LATER.
                // FACIAL ANIMATION 
                var facialPerformanceAnimationTrack = asset.CreateTrack<AnimationTrack>();
                facialPerformanceAnimationTrack.SetGroup(groupTrack);
                var blendshapeAnimationClip =
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(IT_AnimotiveImporterEditorConstants
                        .FacialAnimationCreatedPath);

                if (blendshapeAnimationClip)
                {
                    var facialPerformanceClip = facialPerformanceAnimationTrack.CreateDefaultClip();
                    facialPerformanceClip.start = 0;
                    playableDirector.SetGenericBinding(facialPerformanceAnimationTrack, objToBind);
                }

                // END OF FACIAL ANIMATION


                CreateAnimationTrack(asset, groupTrack, bodyAnimationPath, playableDirector, objToBind);
                // CreateAnimationTrack(); //facial animation
                CreateAudioTrack(asset, groupTrack, clipCluster.AudioClip.clipDataPath, playableDirector, objToBind);
            }


            AssetDatabase.Refresh();

            var playableAsset =
                AssetDatabase.LoadAssetAtPath(assetPath, typeof(PlayableAsset)) as PlayableAsset;


            return playableAsset;
        }

        private static void CreateAnimationTrack(TimelineAsset asset, GroupTrack groupTrack, string bodyAnimationPath,
            PlayableDirector playableDirector, GameObject objToBind)
        {
            if (string.IsNullOrEmpty(bodyAnimationPath)) return;
            var animationTrack = asset.CreateTrack<AnimationTrack>();
            animationTrack.SetGroup(groupTrack);

            var bodyAnimationClip =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(bodyAnimationPath);


            var timelineClip = animationTrack.CreateClip(bodyAnimationClip);
            timelineClip.displayName =
                Path.GetFileNameWithoutExtension(Path.Combine(Directory.GetCurrentDirectory(), bodyAnimationPath));
            timelineClip.start = 0;

            playableDirector.SetGenericBinding(animationTrack, objToBind);
        }


        private static void CreateAudioTrack(TimelineAsset asset, GroupTrack groupTrack, string clipDataPath,
            PlayableDirector playableDirector, GameObject objToBind)
        {
            var clipFullName = string.Concat(clipDataPath, ".wav");

            var path = Path.Combine(IT_AnimotiveImporterEditorWindow.ImportedAudiosAssetdatabaseDirectory,
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