namespace AnimotiveImporterEditor
{
    using UnityEngine;
    using UnityEngine.Playables;
    using UnityEngine.Timeline;

    public class IT_AnimatorClip : PlayableAsset, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            Playable playable = Playable.Create(graph);

            return playable;
        }
    }
}