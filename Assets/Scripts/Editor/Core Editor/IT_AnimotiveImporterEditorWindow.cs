using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    public class IT_AnimotiveImporterEditorWindow : EditorWindow
    {
        public static string ImportedCharactersAssetdatabaseDirectory = Path.Combine(Directory.GetCurrentDirectory(),
            "Assets",
            "Imported Files", "Characters");


        public static string ImportedAudiosAssetdatabaseDirectory = Path.Combine(Directory.GetCurrentDirectory(),
            "Assets",
            "Imported Files", "Audio");

        private static string _parentDirName = "";

        private static bool _DisableImport;
        private static bool _IsAnimotiveFolderImported;

        public static string UserChosenDirectoryToImportUnityExports = "";

        public static bool EnableImportConfig;
        private static bool _ReimportAssets;

        private async void OnGUI()
        {
            #region Choose Animotive folder

            GUILayout.BeginHorizontal();


            EditorGUILayout.TextField("Animotive Export Folder :", UserChosenDirectoryToImportUnityExports);

            if (GUILayout.Button("Choose Folder to Import"))
            {
                var choosenFolder = EditorUtility.OpenFolderPanel("Import Animotive into Unity ",
                    Directory.GetCurrentDirectory(), "");

                if (!string.IsNullOrEmpty(choosenFolder))
                {
                    if (IT_AnimotiveImporterEditorUtilities.IsFolderInCorrectFormatToImport(choosenFolder))
                    {
                        UserChosenDirectoryToImportUnityExports = choosenFolder;

                        var parentDirectory = Directory.GetParent(UserChosenDirectoryToImportUnityExports)?.Name;
                        parentDirectory = parentDirectory?.Trim().Replace(' ', '-');
                        _parentDirName = parentDirectory;

                        var fbxes = await CheckCharacterFbxs(UserChosenDirectoryToImportUnityExports);

                        if (fbxes)
                        {
                            await MoveAudiosIntoUnity(UserChosenDirectoryToImportUnityExports);

                            Debug.Log("Imported the Animotive files successfully !");
                        }

                        UserChosenDirectoryToImportUnityExports = fbxes ? choosenFolder : "";
                        _IsAnimotiveFolderImported = fbxes;
                    }
                    else
                    {
                        _IsAnimotiveFolderImported = false;

                        EditorUtility.DisplayDialog(IT_AnimotiveImporterEditorConstants.WarningTitle,
                            "Please choose a valid Animotive Export folder", "OK");
                    }
                }
                else
                    _IsAnimotiveFolderImported = false;
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Reimport Characters : ");
            _ReimportAssets = EditorGUILayout.Toggle(_ReimportAssets);

            GUILayout.EndHorizontal();

            // if (GUILayout.Button("Clear Accumulated Files")) ClearAccumulatedFiles();

            #endregion


            #region Import Animotive Button

            _DisableImport = _IsAnimotiveFolderImported;
            EditorGUI.BeginDisabledGroup(!_DisableImport);

            if (GUILayout.Button("Import Animotive Scene"))
            {
                var clipsFolderPath = Path.Combine(UserChosenDirectoryToImportUnityExports, "Clips");

                var sceneData = IT_SceneDataOperations.LoadSceneData(clipsFolderPath);
                IT_SceneEditor.CreateScene(sceneData.currentSetName, _parentDirName);


                //create animation clips
                var animationClipOperations =
                    await IT_TransformAnimationClipEditor.HandleBodyAnimationClipOperations(sceneData, clipsFolderPath);


                IT_EntityOperations.HandleEntityOperations(sceneData);


                //create timeline using animation clips
                IT_AnimotiveImporterEditorTimeline.HandleGroups(animationClipOperations.Item1,
                    animationClipOperations.Item2, sceneData);
            }


            EditorGUI.EndDisabledGroup();

            #endregion
        }

        /// <summary>
        ///     Function to show EditorWindow in UnityEditor.
        /// </summary>
        [MenuItem("Animotive/Importer")]
        public static void ShowWindow()
        {
            var window = GetWindow<IT_AnimotiveImporterEditorWindow>("Animotive Importer");
            window.Show();
        }


        public static async void ImportFbxIntoUnityAndProcessIt(string fullOsPath)
        {
            var strippedFfileName = Path.GetFileName(fullOsPath);
            if (!Directory.Exists(ImportedCharactersAssetdatabaseDirectory))
                Directory.CreateDirectory(ImportedCharactersAssetdatabaseDirectory);

            var fullPathToSaveFbx = Path.Combine(ImportedCharactersAssetdatabaseDirectory, strippedFfileName);

            if (File.Exists(fullPathToSaveFbx))
            {
                if (!_ReimportAssets)
                    return;
                File.Delete(fullPathToSaveFbx);
                AssetDatabase.Refresh();
            }

            EnableImportConfig = true;
            File.Copy(fullOsPath, fullPathToSaveFbx, true);

            AssetDatabase.Refresh();
            EnableImportConfig = false;


            AssetDatabase.Refresh();
        }

        private static Task<bool> CheckCharacterFbxs(string unityExportPath)
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
                    .Where(a => a.Substring(a.Length - 4, 4).ToLower().EndsWith(".fbx")).ToList();


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

        private static Task MoveAudiosIntoUnity(string unityExportPath)
        {
            var charactersPath = Path.Combine(unityExportPath, "Clips");

            var files = Directory.GetFiles(charactersPath)
                .Where(a => !a.EndsWith(".meta") &&
                            a.ToLower().EndsWith(IT_AnimotiveImporterEditorConstants.AudioFileExtension)).ToList();

            if (!Directory.Exists(ImportedAudiosAssetdatabaseDirectory))
                Directory.CreateDirectory(ImportedAudiosAssetdatabaseDirectory);
            for (var i = 0; i < files.Count; i++)
            {
                var fileName = Path.GetFileName(files[i]);
                var targetFileName = Path.Combine(ImportedAudiosAssetdatabaseDirectory, fileName);
                if (!File.Exists(targetFileName))
                    File.Copy(files[i], targetFileName, false);
                else
                {
                    targetFileName = IT_AnimotiveImporterEditorUtilities.GetLatestSimilarFileName(
                        ImportedAudiosAssetdatabaseDirectory,
                        files[i], fileName, IT_AnimotiveImporterEditorConstants.AudioFileExtension);
                    File.Copy(files[i], targetFileName, false);
                }
            }

            AssetDatabase.Refresh();
            return Task.CompletedTask;
        }

        private static void ClearAccumulatedFiles()
        {
            Debug.Log(ImportedAudiosAssetdatabaseDirectory);

            var files = Directory.GetFiles(ImportedAudiosAssetdatabaseDirectory);
        }
    }
}