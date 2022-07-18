using System.Collections.Generic;
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
            Dictionary<string, IT_FbxDatasAndHoldersTuple> fbxDatasAndHoldersTuples)
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
                        fbxDatasAndHoldersTuples);
                    
                    for (var k = 0; k < takeData.ClipDatas.Count; k++)
                    {
                        var clipData = takeData.ClipDatas[k];
                        if (clipData.Type != IT_ClipType.TransformClip) continue;

                        var characterInScene = fbxDatasAndHoldersTuples[clipData.ModelName].FbxData.FbxGameObject;

                        var audioSource = characterInScene.AddOrGetComponent<AudioSource>();
                        audioSource.playOnAwake = false;


                        var temp =
                            IT_AnimotiveImporterEditorUtilities.GetBodyAnimationAssetDatabasePath(
                                clipData.animationClipDataPath);
                        playableDirector.playableAsset =
                            CreatePlayableAsset(characterInScene, playableDirector, groupData.GroupName, temp);
                    }
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
        public static PlayableAsset CreatePlayableAsset(GameObject objToBind, PlayableDirector playableDirector,
            string groupName, string bodyAnimationPath)
        {
            var assetPath = string.Concat(IT_AnimotiveImporterEditorConstants.PlayablesCreationPath,
                objToBind.GetInstanceID().ToString(),
                ".playable");

            //TODO: Creating tracks for animationclips should be done in one script which would reduce the duplicate code.
            var asset = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(asset, assetPath);

            var groupTrack = asset.CreateTrack<GroupTrack>();
            groupTrack.name = groupName;

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


            // facialPerformanceClip.displayName = "FACIAL_ANIMATOR_CLIP_DISPLAY_NAME_HERE";

            var bodyPerformanceAnimationTrack = asset.CreateTrack<AnimationTrack>();
            bodyPerformanceAnimationTrack.SetGroup(groupTrack);
            var bodyAnimationClip =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(bodyAnimationPath);
            var bodyPerformanceClip = bodyPerformanceAnimationTrack.CreateClip(bodyAnimationClip);
            bodyPerformanceClip.start = 0;
            // bodyPerformanceClip.displayName = "BODY_ANIMATOR_CLIP_DISPLAY_NAME_HERE";
            playableDirector.SetGenericBinding(bodyPerformanceAnimationTrack, objToBind);


            var itSoundTrack = asset.CreateTrack<IT_SoundTrack>();
            itSoundTrack.SetGroup(groupTrack);
            var soundClip = itSoundTrack.CreateClip<IT_SoundClip>();
            soundClip.displayName = "SOUND_CLIP_DISPLAY_NAME_HERE";
            playableDirector.SetGenericBinding(itSoundTrack, objToBind);

            AssetDatabase.Refresh();

            var playableAsset =
                AssetDatabase.LoadAssetAtPath(assetPath, typeof(PlayableAsset)) as PlayableAsset;


            return playableAsset;
        }
    }
}