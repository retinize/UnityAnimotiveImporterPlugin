using System.Collections.Generic;
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

                for (var j = 0; j < groupData.TakeDatas.Count; j++)
                {
                    var takeData = groupData.TakeDatas[j];

                    var takeNameInScene = string.Concat("Take_", j.ToString());

                    var takeObjectInScene = new GameObject(takeNameInScene);
                    takeObjectInScene.transform.SetParent(groupObject.transform);
                    var playableDirector = takeObjectInScene.AddOrGetComponent<PlayableDirector>();


                    var timelineData = new IT_TimelineData(groupData.GroupName, takeData.ClipDatas, playableDirector,
                        fbxDatasAndHoldersTuples, takeData);

                    playableDirector.playableAsset = CreatePlayableAssets(timelineData, sceneInternalData);


                    // for (var k = 0; k < takeData.ClipDatas.Count; k++)
                    // {
                    //     var clipData = takeData.ClipDatas[k];
                    //     if (clipData.Type != IT_ClipType.TransformClip) continue;
                    //
                    //     var characterInScene = fbxDatasAndHoldersTuples[clipData.ModelName].FbxData.FbxGameObject;
                    //
                    //     var audioSource = characterInScene.AddOrGetComponent<AudioSource>();
                    //     audioSource.playOnAwake = false;
                    //
                    //
                    //     var temp =
                    //         IT_AnimotiveImporterEditorUtilities.GetBodyAnimationAssetDatabasePath(
                    //             clipData.animationClipDataPath);
                    //     
                    //         
                    // }
                }
            }


            // foreach (var gr in group)
            // {
            //     var groupObject = new GameObject(gr.Value[0].BindedGroupName);
            //     for (var i = 0; i < gr.Value.Count; i++)
            //     {
            //         var groupMemberInfo = gr.Value[i];
            //
            //         var characterInScene = groupMemberInfo.ObjectInScene;
            //         var audioSource = characterInScene.AddOrGetComponent<AudioSource>();
            //         audioSource.playOnAwake = false;
            //
            //         var takeNameInScene = string.Concat("Take_", groupMemberInfo.ClipData.TakeIndex.ToString());
            //         var takeObjectInScene = new GameObject(takeNameInScene);
            //
            //
            //         var playableDirector = takeObjectInScene.AddOrGetComponent<PlayableDirector>();
            //         playableDirector.playableAsset =
            //             CreatePlayableAsset(characterInScene, playableDirector, groupMemberInfo);
            //     }
            // }
        }

        /// <summary>
        ///     Creates a playable asset, tracks and clips and binds them to the given gameObject
        /// </summary>
        /// <param name="objToBind">gameObject to bind the playable director to.</param>
        /// <param name="playableDirector">Playable object to bind playable asset and gameobject to. </param>
        /// <param name="groupMemberInfo"></param>
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
            AssetDatabase.CreateAsset(asset, assetPath);


            for (var i = 0; i < timelineData.ClipDatasInTake.Count; i++)
            {
                var clipData = timelineData.ClipDatasInTake[i];

                //TODO: Will be updated to add the audio clip and facial animations as well
                if (clipData.Type != IT_ClipType.TransformClip) continue;
                
                var groupTrack = asset.CreateTrack<GroupTrack>();
                groupTrack.name = timelineData.GroupName;

                var objToBind = timelineData.FbxDataWithHolders[clipData.ModelName].FbxData.FbxGameObject;
                var bodyAnimationPath = IT_AnimotiveImporterEditorUtilities.GetBodyAnimationAssetDatabasePath(
                    clipData.animationClipDataPath);

                // TODO: REMOVE FACIAL ANIMATION PART LATER.
                // FACIAL ANIMATION 
                var facialPerformanceAnimationTrack = asset.CreateTrack<AnimationTrack>();
                facialPerformanceAnimationTrack.SetGroup(groupTrack);
                var blendshapeAnimationClip =
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(IT_AnimotiveImporterEditorConstants
                        .FacialAnimationCreatedPath);

                if (blendshapeAnimationClip)
                {
                    var facialPerformanceClip = facialPerformanceAnimationTrack.CreateClip(blendshapeAnimationClip);
                    facialPerformanceClip.start = 0;
                    playableDirector.SetGenericBinding(facialPerformanceAnimationTrack, objToBind);
                }

                // END OF FACIAL ANIMATION

                CreateAnimationTrack(asset, groupTrack, bodyAnimationPath, playableDirector, objToBind);
                // CreateAnimationTrack(); //facial animation
                CreateAudioTrack(asset, groupTrack, playableDirector, objToBind);
            }


            // var itSoundTrack = asset.CreateTrack<IT_SoundTrack>();
            // itSoundTrack.SetGroup(groupTrack);
            // var soundClip = itSoundTrack.CreateClip<IT_SoundClip>();
            // soundClip.displayName = "SOUND_CLIP_DISPLAY_NAME_HERE";
            // playableDirector.SetGenericBinding(itSoundTrack, objToBind);

            AssetDatabase.Refresh();

            var playableAsset =
                AssetDatabase.LoadAssetAtPath(assetPath, typeof(PlayableAsset)) as PlayableAsset;


            return playableAsset;
        }

        private static void CreateAnimationTrack(TimelineAsset asset, GroupTrack groupTrack, string bodyAnimationPath,
            PlayableDirector playableDirector, GameObject objToBind)
        {
            var animationTrack = asset.CreateTrack<AnimationTrack>();
            animationTrack.SetGroup(groupTrack);
            var bodyAnimationClip =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(bodyAnimationPath);
            var animationCliip = animationTrack.CreateClip(bodyAnimationClip);
            animationCliip.start = 0;
            playableDirector.SetGenericBinding(animationTrack, objToBind);
        }


        private static void CreateAudioTrack(TimelineAsset asset, GroupTrack groupTrack,
            PlayableDirector playableDirector, GameObject objToBind)
        {
            var itSoundTrack = asset.CreateTrack<IT_SoundTrack>();
            itSoundTrack.SetGroup(groupTrack);
            var soundClip = itSoundTrack.CreateClip<IT_SoundClip>();
            soundClip.displayName = "SOUND_CLIP_DISPLAY_NAME_HERE";
            playableDirector.SetGenericBinding(itSoundTrack, objToBind);
        }
    }
}