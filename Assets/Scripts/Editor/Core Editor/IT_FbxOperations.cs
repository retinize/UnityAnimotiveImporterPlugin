using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
                        var pluginTPose = IT_PoseTestManager.GetPoseFromAnimator(fbxData.FbxAnimator);

                        var temp = new IT_FbxDatasAndHoldersTuple(fbxData, holderObject, pluginTPose);

                        fbxDatasAndHoldersTuples.Add(clipData.ModelName, temp);
                    }
                }
            }

            return fbxDatasAndHoldersTuples;
        }

        /// <summary>
        ///     This function imports FBX and configures the custom model importer to change the FBX importer settings to suit to
        ///     this reader's needs.
        /// </summary>
        /// <param name="fullOsPath">Full path to fbx. Do not use assetDatabase path.</param>
        public static void ImportFbxIntoUnityAndProcessIt(string fullOsPath)
        {
            var strippedFileName = Path.GetFileName(fullOsPath);

            if (!Directory.Exists(IT_AnimotiveImporterEditorConstants.UnityFilesCharactersDirectory))
                Directory.CreateDirectory(IT_AnimotiveImporterEditorConstants.UnityFilesCharactersDirectory);

            var fullPathToSaveFbx = Path.Combine(IT_AnimotiveImporterEditorConstants.UnityFilesCharactersDirectory,
                strippedFileName);

            if (File.Exists(fullPathToSaveFbx))
            {
                if (!IT_AnimotiveImporterEditorWindow.ReimportAssets)
                    return;
                File.Delete(fullPathToSaveFbx);
                AssetDatabase.Refresh();
            }

            IT_AnimotiveImporterEditorWindow.EnableImportConfig = true;
            File.Copy(fullOsPath, fullPathToSaveFbx, true);

            AssetDatabase.Refresh();
            IT_AnimotiveImporterEditorWindow.EnableImportConfig = false;

            AssetDatabase.Refresh();
        }

        /// <summary>
        ///     This function checks if there's FBXes under their relative folder and if so, checks if there's only one.
        /// </summary>
        /// <param name="unityExportPath">Path to user browsed and selected folder usually called "UnityExported" </param>
        /// <returns></returns>
        public static Task<bool> CheckCharacterFbxs(string unityExportPath)
        {
            var charactersPath = Path.Combine(unityExportPath, "EntityAssets", "Characters");
            var characterFolders = Directory.GetDirectories(charactersPath);

            if (characterFolders.Length == 0)
            {
                EditorUtility.DisplayDialog(IT_AnimotiveImporterEditorConstants.WarningTitle,
                    @"Couldn't find any Character folder to import FBX", "OK");

                return Task.FromResult(false);
            }

            for (var i = 0; i < characterFolders.Length; i++)
            {
                var folder = characterFolders[i];
                var fbxes = Directory.GetFiles(folder)
                    .Where(a => a.Substring(a.Length - 4, 4).ToLower()
                        .EndsWith(IT_AnimotiveImporterEditorConstants.ModelExtension)).ToList();

                var existence = fbxes.Count != 0;
                if (!existence)
                {
                    EditorUtility.DisplayDialog(IT_AnimotiveImporterEditorConstants.WarningTitle,
                        $@"Couldn't find any FBX file at the directory: {folder}", "OK");
                    return Task.FromResult(false);
                }

                existence = fbxes.Count == 1;
                if (!existence)
                {
                    EditorUtility.DisplayDialog(IT_AnimotiveImporterEditorConstants.WarningTitle,
                        $@"More than one FBX is detected at : {folder}", "OK");
                    return Task.FromResult(false);
                }

                var fullPathToExpectedFbx = fbxes[0];
                ImportFbxIntoUnityAndProcessIt(fullPathToExpectedFbx);
            }

            return Task.FromResult(true);
        }
    }
}