using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    public static class IT_FbxOperations
    {
        /// <summary>
        ///     Loads FBX from it's designated path and instantiates it to the current scene in the editor
        /// </summary>
        /// <returns>Tuple that contains instantiated character's root gameObject and it's Animator</returns>
        public static IT_FbxData LoadFbx(string fbxAssetDatabasePath)
        {
            var characterRoot = AssetDatabase.LoadAssetAtPath(fbxAssetDatabasePath,
                typeof(GameObject)) as GameObject;

            characterRoot = Object.Instantiate(characterRoot);
            characterRoot.AddComponent<AudioSource>();
            var animator = characterRoot.GetComponent<Animator>();

            return new IT_FbxData(characterRoot, animator);
        }

        /// <summary>
        ///     Loads FBXes into scene and populates a dictionary with the information about them.
        /// </summary>
        /// <param name="transformGroupDatas">List of group datas </param>
        /// <returns>A dictionary with the model name as key and models data tuple as value</returns>
        public static Dictionary<string, IT_FbxDatasAndHoldersTuple> GetFbxDataAndHolders(
            List<IT_GroupData> transformGroupDatas)
        {
            var fbxDatasAndHoldersTuples = new Dictionary<string, IT_FbxDatasAndHoldersTuple>();

            for (var i = 0; i < transformGroupDatas.Count; i++)
            {
                var groupdata = transformGroupDatas[i];

                foreach (var pair in groupdata.TakeDatas)
                {
                    var takeData = pair.Value;
                    for (var k = 0; k < takeData.Clusters.Count; k++)
                    {
                        var clipData = takeData.Clusters[k];

                        if (fbxDatasAndHoldersTuples.ContainsKey(clipData.ModelName)) continue;
                        var files = Directory.GetDirectories(Path.Combine(
                            IT_AnimotiveImporterEditorWindow.UserChosenDirectoryToImportUnityExports, "EntityAssets",
                            "Characters"));

                        files = files.Where(a => a.EndsWith(clipData.ModelName)).ToArray();
                        var modelDirectory = files[0];
                        var fbxes = Directory.GetFiles(modelDirectory)
                            .Where(a => a.Substring(a.Length - 4, 4).ToLower()
                                .EndsWith(IT_AnimotiveImporterEditorConstants.ModelExtension)).ToArray();

                        var fullOsPathToFbx = fbxes[0];
                        var modelName = Path.GetFileName(fullOsPathToFbx);

                        var pathToFbx = Path.Combine(
                            IT_AnimotiveImporterEditorConstants.UnityFilesCharactersDirectory
                            , modelName);

                        pathToFbx =
                            IT_AnimotiveImporterEditorUtilities.ConvertSystemPathToAssetDatabasePath(pathToFbx);

                        var fbxData = LoadFbx(pathToFbx);
                        var holderObject = new GameObject(string.Concat(fbxData.FbxGameObject.name, "_HOLDER"));
                        var pluginTPose = IT_PoseTestManager.SavePoseToJson(fbxData.FbxAnimator);

                        var temp = new IT_FbxDatasAndHoldersTuple(fbxData, holderObject, pluginTPose);

                        fbxDatasAndHoldersTuples.Add(clipData.ModelName, temp);
                    }
                }
            }

            return fbxDatasAndHoldersTuples;
        }
    }
}