using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class SoundClip : PlayableAsset, ITimelineClipAsset
{
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        return new Playable();
    }

    public ClipCaps clipCaps { get; }
}