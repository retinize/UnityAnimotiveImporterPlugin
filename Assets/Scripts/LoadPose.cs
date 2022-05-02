using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadPose : MonoBehaviour
{
    [ContextMenu("Load Pose")]
    public void LoadThisPose()
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        string            text              = File.ReadAllText(desktopPath + "\\test.json");
        TransformInfoList transformInfoList = JsonUtility.FromJson<TransformInfoList>(text);

        for (int i = 0; i < transformInfoList.TransformsByStrings.Count; i++)
        {
            Transform tr = transform.FindChildRecursively(transformInfoList.TransformsByStrings[i].Name);

            if (tr != null)
            {
                tr.localPosition = transformInfoList.TransformsByStrings[i].Position;
                tr.localRotation = transformInfoList.TransformsByStrings[i].Rotation;
            }
        }
    }
}

[Serializable]
public class TransformsByString
{
    public string     Name;
    public Vector3    Position;
    public Quaternion Rotation;

    public TransformsByString(string name, Vector3 position, Quaternion rotation)
    {
        Name     = name;
        Position = position;
        Rotation = rotation;
    }
}


[Serializable]
public class TransformInfoList
{
    public List<TransformsByString> TransformsByStrings = new List<TransformsByString>();
}