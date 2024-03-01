using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    public static class IT_AnimotiveImporterPluginExtensions
    {
        public static Transform FindChildRecursively(this Transform thisTransform, string name)
        {
            var childTrans = thisTransform.Find(name);
            if (childTrans != null) return childTrans;

            if (thisTransform.childCount != 0)
            {
                for (var i = 0; i < thisTransform.childCount; i++)
                {
                    var result = FindChildRecursively(thisTransform.GetChild(i), name);
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
}