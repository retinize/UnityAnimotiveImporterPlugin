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
        public static void HandleGroups(Dictionary<int, List<IT_AnimotiveImporterEditorGroupMemberInfo>> group)
        {
            var index = 0;
            foreach (var gr in group)
            {
                index++;
                var groupObjectName = string.Concat("Group", "_", index.ToString());
                var groupObject = new GameObject(groupObjectName);
                for (var i = 0; i < gr.Value.Count; i++)
                {
                    var groupMemberInfo = gr.Value[i];

                    var characterInScene = groupMemberInfo.ObjectInScene;
                    var audioSource = characterInScene.AddOrGetComponent<AudioSource>();
                    audioSource.playOnAwake = false;

                    var groupMemberInScene = new GameObject(groupMemberInfo.name);
                    groupMemberInScene.transform.SetParent(groupObject.transform);

                    var playableDirector = groupMemberInScene.AddOrGetComponent<PlayableDirector>();
                    playableDirector.playableAsset =
                        CreatePlayableAsset(characterInScene, playableDirector, groupMemberInfo);
                }
            }
        }

        /// <summary>
        ///     Creates a playable asset, tracks and clips and binds them to the given gameObject
        /// </summary>
        /// <param name="objToBind">gameObject to bind the playable director to.</param>
        /// <param name="playableDirector">Playable object to bind playable asset and gameobject to. </param>
        /// <param name="groupMemberInfo"></param>
        /// <returns></returns>
        public static PlayableAsset CreatePlayableAsset(GameObject objToBind, PlayableDirector playableDirector,
            IT_AnimotiveImporterEditorGroupMemberInfo groupMemberInfo)
        {
            var assetPath = string.Concat(IT_AnimotiveImporterEditorConstants.PlayablesCreationPath,
                objToBind.GetInstanceID().ToString(),
                ".playable");


            var asset = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(asset, assetPath);

            var groupTrack = asset.CreateTrack<GroupTrack>();
            groupTrack.name = "GROUP_NAME_HERE";

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
                AssetDatabase.LoadAssetAtPath<AnimationClip>(groupMemberInfo.BodyAnimationPath);
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