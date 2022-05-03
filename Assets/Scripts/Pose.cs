using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Pose : MonoBehaviour
{
    [ContextMenu("Load Pose")]
    public void LoadPose()
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        string            text              = File.ReadAllText(desktopPath + "\\test.json");
        TransformInfoList transformInfoList = JsonUtility.FromJson<TransformInfoList>(text);

        for (int i = 0; i < transformInfoList.TransformsByStrings.Count; i++)
        {
            Transform tr = transform.FindChildRecursively(transformInfoList.TransformsByStrings[i].Name);

            if (tr != null)
            {
                tr.localPosition    = transformInfoList.TransformsByStrings[i].LocalPosition;
                tr.localRotation    = transformInfoList.TransformsByStrings[i].LocalRotation;
                tr.localScale       = transformInfoList.TransformsByStrings[i].LocalScale;
                tr.localEulerAngles = transformInfoList.TransformsByStrings[i].LocalEulerAngles;
            }
        }
    }


    [ContextMenu("Save Pose")]
    public void SaveThisPose()
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        List<Transform> transforms = new List<Transform>();

        TransformInfoList temp = new TransformInfoList();

        GetAllChildrenRecursively(ref transforms, transform, ref temp);


        string jsonString = JsonUtility.ToJson(temp, true);
        File.WriteAllText(desktopPath + "\\test.json", jsonString);
    }

    private void GetAllChildrenRecursively(ref List<Transform>   transforms, Transform targetTransform,
                                           ref TransformInfoList transformInfoList)
    {
        for (int i = 0; i < targetTransform.childCount; i++)
        {
            Transform child = targetTransform.GetChild(i);
            if (!transforms.Contains(child))
            {
                transforms.Add(child);
                transformInfoList.TransformsByStrings.Add(new TransformsByString(child));
            }

            if (child.childCount > 0)
            {
                GetAllChildrenRecursively(ref transforms, child, ref transformInfoList);
            }
        }
    }
}

[Serializable]
public class TransformsByString
{
    public string     Name;
    public Vector3    LocalPosition;
    public Quaternion LocalRotation;
    public Vector3    LocalScale;
    public Vector3    LocalEulerAngles;

    public TransformsByString(Transform tr)
    {
        Name             = tr.name;
        LocalPosition    = tr.localPosition;
        LocalRotation    = tr.localRotation;
        LocalScale       = tr.localScale;
        LocalEulerAngles = tr.localEulerAngles;
    }
}


[Serializable]
public class TransformInfoList
{
    public List<TransformsByString> TransformsByStrings = new List<TransformsByString>();
}