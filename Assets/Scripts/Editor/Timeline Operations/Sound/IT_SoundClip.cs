namespace AnimotiveImporterEditor
{
    using UnityEngine;
    using UnityEngine.Playables;
    using UnityEngine.Timeline;

    public class IT_SoundClip : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField] private IT_SoundBehaviour _behaviour = new IT_SoundBehaviour();

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<IT_SoundBehaviour>.Create(graph, _behaviour);
        }
    }
}