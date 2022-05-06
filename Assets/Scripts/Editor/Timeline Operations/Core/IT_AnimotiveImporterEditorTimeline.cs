namespace AnimotiveImporterEditor
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
            for (int i = 0; i < group.Count; i++)
            {
                GameObject obj = new GameObject(string.Format("<group name here : {0}>", i));
                obj.AddComponent<AudioSource>();
                obj.AddComponent<Animator>();
                PlayableDirector playableDirector = obj.AddComponent<PlayableDirector>();
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
            string assetPath = string.Concat(IT_AnimotiveImporterEditorConstants.PlayablesCreationPath,
                                             objToBind.GetInstanceID().ToString(),
                                             ".playable");

            TimelineAsset asset = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(asset, assetPath);

            GroupTrack groupTrack = asset.CreateTrack<GroupTrack>();
            groupTrack.name = "GROUP_NAME_HERE";

            AnimatorTrack facialPerformanceAnimationTrack = asset.CreateTrack<AnimatorTrack>();
            facialPerformanceAnimationTrack.SetGroup(groupTrack);
            TimelineClip facialPerformanceClip = facialPerformanceAnimationTrack.CreateClip<AnimatorClip>();
            facialPerformanceClip.displayName = "FACIAL_ANIMATOR_CLIP_DISPLAY_NAME_HERE";
            playableDirector.SetGenericBinding(facialPerformanceAnimationTrack, objToBind);

            AnimatorTrack bodyPerformanceAnimationTrack = asset.CreateTrack<AnimatorTrack>();
            bodyPerformanceAnimationTrack.SetGroup(groupTrack);
            TimelineClip bodyPerformanceClip = bodyPerformanceAnimationTrack.CreateClip<AnimatorClip>();
            bodyPerformanceClip.displayName = "BODY_ANIMATOR_CLIP_DISPLAY_NAME_HERE";
            playableDirector.SetGenericBinding(bodyPerformanceAnimationTrack, objToBind);


            SoundTrack soundTrack = asset.CreateTrack<SoundTrack>();
            soundTrack.SetGroup(groupTrack);
            TimelineClip soundClip = soundTrack.CreateClip<SoundClip>();
            soundClip.displayName = "SOUND_CLIP_DISPLAY_NAME_HERE";
            playableDirector.SetGenericBinding(soundTrack, objToBind);

            AssetDatabase.Refresh();

            PlayableAsset playableAsset =
                AssetDatabase.LoadAssetAtPath(assetPath, typeof(PlayableAsset)) as PlayableAsset;


            return playableAsset;
        }
    }
}