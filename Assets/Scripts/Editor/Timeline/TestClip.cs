using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TestClip : PlayableAsset, ITimelineClipAsset
{
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        throw new System.NotImplementedException();
    }

    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending; }
    }
}