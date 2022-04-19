using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IT_SceneInternalData
{
    public string currentSetName;
    public int currentSessionNumber;
    public int lastEntityIdAssigned;
    public int numberOfTakesCaptured;
    public Dictionary<int, IT_EntitySceneData> entitiesDataById;
}

[Serializable]
public class IT_EntitySceneData
{
    public int entityPrefabId; //Useful only for the SERVER to instantiate the prefab

    public int
        sessionNumberInWhichWasCreated; //Useful to everyone to check if the entity was created in this session or not

    public object[] propertiesData; //Useful only for the SERVER when instantiating the prefab

    public string
        entityInstantiationTokenData; //Useful only for the SERVER. For now we're just using a string, since all entities are using IT_StringProtocolToken. 

    public List<List<string>> animationInternalData; //Useful for everyone.
}

[Serializable]
public class IT_TransformClip : IT_ClipBase
{
    public IT_ObjectTransformCurves[] singleObjectCurves;

    public IT_TransformClip(IT_ObjectTransformCurves[] singleObjectCurves, string nameOfClip, int initFrame = 0)
    {
        this.singleObjectCurves = singleObjectCurves;
        this.nameOfClip = nameOfClip;
        this.initFrame = initFrame;
        //TODO: Compute length here, or somewhere
    }
}

[Serializable]
public class IT_FixedAnimationBaseClip : IT_ClipBase
{
    public float fixedDeltaTime;
    public int keyFramesCount;
}


[Serializable]
public class IT_CharacterTransformAnimationClip : IT_FixedAnimationBaseClip
{
    public float[] pureIkKeyframesCurve0;
    public float[] pureIkKeyframesCurve1;
    public float[] pureIkKeyframesCurve2;
    public float[] pureIkKeyframesCurve3;
    public float[] pureIkKeyframesCurve4;
    public float[] pureIkKeyframesCurve5;
    public float[] pureIkKeyframesCurve6;

    public float[] physicsKeyframesCurve0;
    public float[] physicsKeyframesCurve1;
    public float[] physicsKeyframesCurve2;
    public float[] physicsKeyframesCurve3;
    public float[] physicsKeyframesCurve4;
    public float[] physicsKeyframesCurve5;
    public float[] physicsKeyframesCurve6;

    public int[] humanoidBonesEnumThatAreUsed;

    public Vector3 worldPositionHolder;
    public Quaternion worldRotationHolder;
    public float lossyScaleHolder;

    public IT_CharacterTransformAnimationClip(int[] humanoidBonesEnumThatAreUsed,
        float[] pureIkKeyframesCurve0, float[] pureIkKeyframesCurve1,
        float[] pureIkKeyframesCurve2, float[] pureIkKeyframesCurve3, float[] pureIkKeyframesCurve4,
        float[] pureIkKeyframesCurve5, float[] pureIkKeyframesCurve6, float[] physicsKeyframesCurve0,
        float[] physicsKeyframesCurve1, float[] physicsKeyframesCurve2, float[] physicsKeyframesCurve3,
        float[] physicsKeyframesCurve4, float[] physicsKeyframesCurve5, float[] physicsKeyframesCurve6,
        string nameOfClip, int numberOfTakeInWhatWasCaptured, Vector3 worldPositionHolder,
        Quaternion worldRotationHolder, float lossyScaleHolder, int initFrame = 0,
        float fixedDeltaTime = IT_PhysicsManager.FixedDeltaTime)
    {
        this.fixedDeltaTime = fixedDeltaTime;
        this.nameOfClip = nameOfClip;
        this.initFrame = initFrame;
        this.numberOfTakeInWhatWasCaptured = numberOfTakeInWhatWasCaptured;
        this.worldPositionHolder = worldPositionHolder;
        this.worldRotationHolder = worldRotationHolder;
        this.lossyScaleHolder = lossyScaleHolder;
        this.humanoidBonesEnumThatAreUsed = humanoidBonesEnumThatAreUsed;

        this.pureIkKeyframesCurve0 = pureIkKeyframesCurve0;
        this.pureIkKeyframesCurve1 = pureIkKeyframesCurve1;
        this.pureIkKeyframesCurve2 = pureIkKeyframesCurve2;
        this.pureIkKeyframesCurve3 = pureIkKeyframesCurve3;
        this.pureIkKeyframesCurve4 = pureIkKeyframesCurve4;
        this.pureIkKeyframesCurve5 = pureIkKeyframesCurve5;
        this.pureIkKeyframesCurve6 = pureIkKeyframesCurve6;

        this.physicsKeyframesCurve0 = physicsKeyframesCurve0;
        this.physicsKeyframesCurve1 = physicsKeyframesCurve1;
        this.physicsKeyframesCurve2 = physicsKeyframesCurve2;
        this.physicsKeyframesCurve3 = physicsKeyframesCurve3;
        this.physicsKeyframesCurve4 = physicsKeyframesCurve4;
        this.physicsKeyframesCurve5 = physicsKeyframesCurve5;
        this.physicsKeyframesCurve6 = physicsKeyframesCurve6;

        keyFramesCount = physicsKeyframesCurve0.Length / humanoidBonesEnumThatAreUsed.Length;
        lengthInFrames = keyFramesCount;
    }

    public int GetIndexInCurveArrayOfATransformAndKeyFrame(int transformIndex, int keyIndex)
    {
        return keyIndex * humanoidBonesEnumThatAreUsed.Length + transformIndex;
    }

    public override IT_ClipPlayerBase GenerateAndInitializeClipPlayer(IT_ClipBase clip,
        Transform objectInWhichIsGoingToBePlayed, IT_AnimatorControllerBase animatorController)
    {
        return null;
    }
}


[Serializable]
public class IT_ClipBase
{
    public string nameOfClip;
    public int initFrame;
    public int lengthInFrames;
    public int numberOfTakeInWhatWasCaptured;

    public int lastFrame => initFrame + lengthInFrames - 1;

    public bool IsFrameWithinClipTime(int frame)
    {
        return (frame >= initFrame) && (frame <= lastFrame);
    }

    public virtual IT_ClipPlayerBase GenerateAndInitializeClipPlayer(IT_ClipBase clip,
        Transform objectInWhichIsGoingToBePlayed, IT_AnimatorControllerBase animatorController)
    {
        return null;
    }
}

[Serializable]
public class IT_FixedCurve
{
    public List<float> KeyFrames = new List<float>();

    public void AddValue(float value)
    {
        KeyFrames.Add(value);
    }

    public void RemoveValuesOverFrame(int lastFrameToInclude)
    {
        IT_Utils.RemoveValuesFromListOverIndex(KeyFrames, lastFrameToInclude);
    }
}


public class IT_PhysicsManager : MonoBehaviour
{
    public const float FixedDeltaTime = 0.01666667f;
}

public static class IT_Utils
{
    public static void RemoveValuesFromListOverIndex(IList list, int lastIndexToInclude)
    {
        var listLength = list.Count;
        var indexToDelete = lastIndexToInclude + 1;
        for (int i = indexToDelete; i < listLength; i++)
        {
            list.RemoveAt(indexToDelete);
        }
    }
}


public abstract class IT_ClipPlayerBase : IIT_Disposable
{
    public event Action OnTimeSampledOutsideClipTime;
    public abstract int initFrameOfClip { get; }
    public bool playingAnimation { get; protected set; }

    protected void TriggerOnTimeSampledOutsideClipTime()
    {
        OnTimeSampledOutsideClipTime?.Invoke();
    }


    /// <summary> Gets called when the clip is over and the next one passes to play and when the player pauses playback performance</summary>
    public virtual void PauseClip()
    {
        playingAnimation = false;
    }

    /// <summary> Return value indicates if it the frame is within the animation clip or not </summary>
    public abstract bool ExplicitSampleAnimation(int frame);

    /// <summary> Should be call every time that we delete an animationClipPlayer from an animator </summary>
    public virtual void Dispose()
    {
    }
}

public interface IIT_Disposable
{
    void Dispose();
}

public class IT_AnimatorControllerBase
{
}

[Serializable]
public struct IT_ObjectTransformCurves
{
    public IT_Curve[] curves;
    public List<int> hierarchyPath;

    public IT_ObjectTransformCurves(List<int> hierarchyPath)
    {
        this.hierarchyPath = hierarchyPath;

        curves = new IT_Curve[7];

        curves[0] = new IT_Curve();
        curves[1] = new IT_Curve();
        curves[2] = new IT_Curve();

        curves[3] = new IT_Curve();
        curves[4] = new IT_Curve();
        curves[5] = new IT_Curve();
        curves[6] = new IT_Curve();
    }

    /// <summary> Caveat: It should to save the keyframes for the same object always </summary>
    public void AddFrame(float time, Transform gameObject)
    {
        curves[0].AddValue(time, gameObject.localPosition.x);
        curves[1].AddValue(time, gameObject.localPosition.y);
        curves[2].AddValue(time, gameObject.localPosition.z);

        curves[3].AddValue(time, gameObject.localRotation.x);
        curves[4].AddValue(time, gameObject.localRotation.y);
        curves[5].AddValue(time, gameObject.localRotation.z);
        curves[6].AddValue(time, gameObject.localRotation.w);
    }
}


[Serializable]
public class IT_Curve
{
    public List<IT_KeyFrame> KeyFrames = new List<IT_KeyFrame>();

    public void AddValue(float time, float value)
    {
        KeyFrames.Add(new IT_KeyFrame(time, value));
    }

    public float EvaluateWithLinearInterpolation(float time)
    {
        //TODO: Binary search
        throw new NotImplementedException();
    }
}


[Serializable]
public class IT_KeyFrame
{
    public float time;

    public float value;
    /*
    public float inTangent;
    public float outTangent;
    public float inWeight;
    public float outWeight;*/

    public IT_KeyFrame(Keyframe keyframe)
    {
        time = keyframe.time;
        value = keyframe.value;
        /*
        inTangent = keyframe.inTangent;
        outTangent = keyframe.outTangent;
        inWeight = keyframe.inWeight;
        outWeight = keyframe.outWeight;*/
    }

    public IT_KeyFrame(float nTime, float nValue)
    {
        time = nTime;
        value = nValue;
    }

    /*
    public IT_KeyFrame(float nTime, float nValue, float nInTangent, float nOutTangent)
    {
        time = nTime;
        value = nValue;
        
        inTangent = nInTangent;
        outTangent = nOutTangent;
    }
    public IT_KeyFrame(float nTime, float nValue, float nInTangent, float nOutTangent, float nInWeight, float nOutWeight)
    {
        time = nTime;
        value = nValue;
        inTangent = nInTangent;
        outTangent = nOutTangent;
        inWeight = nInWeight;
        outWeight = nOutWeight;
    }*/
}