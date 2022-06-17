namespace Retinize.Editor.AnimotiveImporter
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Playables;
    using UnityEngine.Timeline;

    public static class IT_AnimotiveImporterEditorTimeline
    {
        /// <summary>
        ///     Creates the scene objects according to given group info and  creates&assigns them to their respective playable
        ///     asset.
        /// </summary>
        /// <param name="group">List of group info</param>
        public static void HandleGroups(List<IT_AnimotiveImporterEditorGroupInfo> group)
        {
            for (var i = 0; i < group.Count; i++)
            {
                var obj = group[i].fbx;
                obj.name = group[i].name;
                var audioSource = obj.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                var playableDirector = obj.AddComponent<PlayableDirector>();
                playableDirector.playableAsset = CreatePlayableAsset(obj, playableDirector);
            }
        }

        /// <summary>
        ///     Creates a playable asset, tracks and clips and binds them to the given gameObject
        /// </summary>
        /// <param name="objToBind">gameObject to bind the playable director to.</param>
        /// <param name="playableDirector">Playable object to bind playable asset and gameobject to. </param>
        /// <returns></returns>
        public static PlayableAsset CreatePlayableAsset(GameObject objToBind, PlayableDirector playableDirector)
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
                AssetDatabase.LoadAssetAtPath<AnimationClip>(IT_TransformAnimationClipEditor.bodyAnimationPath);
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