namespace Retinize.Editor.AnimotiveImporter
{
    using System;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

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
        public static Tuple<GameObject, Animator> LoadFbx()
        {
            var characterRoot = AssetDatabase.LoadAssetAtPath(
                IT_AnimotiveImporterEditorConstants.FBXPath,
                typeof(GameObject)) as GameObject;

            characterRoot = Object.Instantiate(characterRoot);
            var animator = characterRoot.GetComponent<Animator>();

            return new Tuple<GameObject, Animator>(characterRoot, animator);
        }

        public static bool IsFolderInCorrectFormatToImport(string path)
        {
            var dirs = Directory.GetDirectories(path);

            var result = dirs.Any(a => a.EndsWith("Clips")) && dirs.Any(a => a.EndsWith("SetAssets")) &&
                         dirs.Any(a => a.EndsWith("EntityAssets"));
            return result;
        }

        public static string ReturnClipDataFromPath(string clipsPath)
        {
            var files = Directory.GetFiles(clipsPath);

            for (var i = 0; i < files.Length; i++)
            {
                if (files[i].Contains(IT_AnimotiveImporterEditorConstants.TransformClipName)) return files[i];
            }

            return "";
        }
    }


#endif
}