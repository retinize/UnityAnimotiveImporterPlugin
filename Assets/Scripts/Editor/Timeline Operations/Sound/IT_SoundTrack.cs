namespace AnimotiveImporterEditor
{
    using UnityEngine;
    using UnityEngine.Timeline;

    [TrackColor(0, 255, 0)]
    [TrackBindingType(typeof(AudioSource))]
    [TrackClipType(typeof(IT_SoundClip))]
    public class IT_SoundTrack : TrackAsset
    {
    }
}