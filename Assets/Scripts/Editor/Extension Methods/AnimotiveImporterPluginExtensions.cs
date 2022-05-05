using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimotiveImporterPluginExtensions
{
    public static Transform FindChildRecursively(this Transform obj, string name)
    {
        Transform trans = obj.transform;
        Transform childTrans = trans.Find(name);
        if (childTrans != null)
        {
            return childTrans;
        }

        if (trans.childCount != 0)
        {
            for (int i = 0; i < trans.childCount; i++)
            {
                Transform result= FindChildRecursively(trans.GetChild(i), name);
                if (result!=null)
                {
                    return result;
                }
            }
        }

        return null;
    }
}