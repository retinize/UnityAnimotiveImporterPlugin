using UnityEngine;

public static class IT_AnimotiveImporterPluginExtensions
{
    public static Transform FindChildRecursively(this Transform obj, string name)
    {
        var trans = obj.transform;
        var childTrans = trans.Find(name);
        if (childTrans != null) return childTrans;


        if (trans.childCount != 0)
        {
            for (var i = 0; i < trans.childCount; i++)
            {
                var result = FindChildRecursively(trans.GetChild(i), name);
                if (result != null) return result;
            }
        }

        return null;
    }

    /// <summary>
    ///     Adds given component to the given game object if it doesn't exist already and returns it. If it does, gets and
    ///     returns it.
    /// </summary>
    /// <param name="obj">Object to add or get</param>
    /// <typeparam name="T">Component type</typeparam>
    /// <returns>Added/get component</returns>
    public static T AddOrGetComponent<T>(this GameObject obj) where T : Component
    {
        var get = obj.GetComponent<T>();
        if (get == null) return obj.AddComponent<T>();

        return get;
    }
}