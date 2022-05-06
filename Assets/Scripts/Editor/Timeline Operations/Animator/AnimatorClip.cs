using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class AnimatorClip : PlayableAsset, ITimelineClipAsset
{
    public ClipCaps clipCaps { get; }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        Playable         playable = Playable.Create(graph);

        return playable;
    }
}