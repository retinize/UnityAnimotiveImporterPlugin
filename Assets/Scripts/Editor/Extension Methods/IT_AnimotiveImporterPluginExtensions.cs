using UnityEngine;

public static class IT_AnimotiveImporterPluginExtensions
{
    public static Transform FindChildRecursively(this Transform obj, string name)
    {
        Transform trans      = obj.transform;
        Transform childTrans = trans.Find(name);
        if (childTrans != null)
        {
            return childTrans;
        }


        if (trans.childCount != 0)
        {
            for (int i = 0; i < trans.childCount; i++)
            {
                Transform result = FindChildRecursively(trans.GetChild(i), name);
                if (result != null)
                {
                    return result;
                }
            }
        }

        return null;
    }
    
    public static string TrimStartByString(this string target, string trimString)
    {
        if (string.IsNullOrEmpty(trimString)) return target;

        string result = target;
        while (result.StartsWith(trimString))
        {
            result = result.Substring(trimString.Length);
        }

        return result;
    }
    
    public static string TrimEndByString(this string target, string trimString)
    {
        if (string.IsNullOrEmpty(trimString)) return target;

        string result = target;
        while (result.EndsWith(trimString))
        {
            result = result.Substring(0, result.Length - trimString.Length);
        }

        return result;
    }
    
}