using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Retinize.Editor.AnimotiveImporter
{
#if UNITY_EDITOR
    public static class IT_AnimotiveImporterEditorUtilities
    {
        /// <summary>
        ///     Deletes the asset if it already exists in the AssetDatabase
        /// </summary>
        /// <param name="path">Path of the asset in the asset database.</param>
        /// <param name="type">Type of the asset to look for and delete.</param>
        public static void DeleteAssetIfExists(string path, Type type)
        {
            if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path)))
            {
                if (AssetDatabase.LoadAssetAtPath(path, type) != null)
                    AssetDatabase.DeleteAsset(path);
            }
        }

        /// <summary>
        ///     Loads FBX from it's designated path and instantiates it to the current scene in the editor
        /// </summary>
        /// <returns>Tuple that contains instantiated character's root gameObject and it's Animator</returns>
        public static IT_FbxData LoadFbx(string fbxAssetDatabasePath)
        {
            var characterRoot = AssetDatabase.LoadAssetAtPath(fbxAssetDatabasePath,
                typeof(GameObject)) as GameObject;

            characterRoot = Object.Instantiate(characterRoot);
            var animator = characterRoot.GetComponent<Animator>();

            return new IT_FbxData(characterRoot, animator);
        }

        public static bool IsFolderInCorrectFormatToImport(string path)
        {
            var dirs = Directory.GetDirectories(path);

            var result = dirs.Any(a => a.EndsWith("Clips")) && dirs.Any(a => a.EndsWith("SetAssets")) &&
                         dirs.Any(a => a.EndsWith("EntityAssets"));
            return result;
        }

        public static string ReturnClipDataFromPath(string clipsPath, string clipName)
        {
            var files = Directory.GetFiles(clipsPath);

            for (var i = 0; i < files.Length; i++)
            {
                if (files[i].Contains(clipName)) return files[i];
            }

            return "";
        }

        public static IT_ClipType GetClipTypeFromClipName(string clipName)
        {
            foreach (var pair in IT_AnimotiveImporterEditorConstants.ClipNamesByType)
            {
                if (clipName.Contains(pair.Value)) return pair.Key;
            }

            return IT_ClipType.None;
        }

        public static T AddOrGetComponent<T>(this GameObject obj) where T : Component
        {
            var get = obj.GetComponent<T>();
            if (get == null) return obj.AddComponent<T>();

            return get;
        }
    }

#endif
}