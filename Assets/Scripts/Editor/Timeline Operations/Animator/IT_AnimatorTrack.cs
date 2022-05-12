using UnityEngine;

namespace AnimotiveImporterEditor
{
    using UnityEngine;
    using UnityEngine.Timeline;

    [TrackColor(0, 0, 255)]
    [TrackBindingType(typeof(Animator))]
    [TrackClipType(typeof(IT_AnimatorClip))]
    public class IT_AnimatorTrack : TrackAsset
    {
    }
}

public class TempClass
{
    public int someName;

    private void someFunction(int value)
    {
        someName++;
        int temp = 5;
        switch (value)
        {
            default:
                Debug.Log("aaa");
                break;
        }
    }
}