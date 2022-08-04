using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    public class IT_AnimotiveImporterEditorWindow : EditorWindow
    {
        private static bool _disableImport;
        private static bool _isAnimotiveFolderImported;

        public static string UserChosenDirectoryToImportUnityExports = "";

        public static bool EnableImportConfig;
        private static bool _reimportAssets;

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

                        var fbxes = await CheckCharacterFbxs(UserChosenDirectoryToImportUnityExports);

                        if (fbxes) Debug.Log("Imported the Animotive files successfully !");

                        UserChosenDirectoryToImportUnityExports = fbxes ? choosenFolder : "";
                        _isAnimotiveFolderImported = fbxes;
                    }
                    else
                    {
                        _isAnimotiveFolderImported = false;

                        EditorUtility.DisplayDialog(IT_AnimotiveImporterEditorConstants.WarningTitle,
                            "Please choose a valid Animotive Export folder", "OK");
                    }
                }
                else
                    _isAnimotiveFolderImported = false;
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Reimport Characters : ");
            _reimportAssets = EditorGUILayout.Toggle(_reimportAssets);

            GUILayout.EndHorizontal();

            // if (GUILayout.Button("Clear Accumulated Files")) ClearAccumulatedFiles();

            #endregion


            #region Import Animotive Button

            _disableImport = _isAnimotiveFolderImported;
            EditorGUI.BeginDisabledGroup(!_disableImport);

            if (GUILayout.Button("Import Animotive Scene"))
            {
                await MoveAudiosIntoUnity(UserChosenDirectoryToImportUnityExports);

                var clipsFolderPath = Path.Combine(UserChosenDirectoryToImportUnityExports, "Clips");

                var sceneData = IT_SceneDataOperations.LoadSceneData(clipsFolderPath);
                var scene = IT_SceneEditor.CreateScene(sceneData.currentSetName);


                //create animation clips
                var animationClipOperations =
                    await IT_BodyAnimationClipEditor.HandleBodyAnimationClipOperations(sceneData, clipsFolderPath);


                IT_EntityOperations.HandleEntityOperations(sceneData);


                //create timeline using animation clips
                IT_AnimotiveImporterEditorTimeline.HandleGroups(animationClipOperations.Item1,
                    animationClipOperations.Item2, sceneData);

                EditorSceneManager.SaveScene(scene);

                AssetDatabase.Refresh();
            }


            EditorGUI.EndDisabledGroup();

            #endregion


            if (GUILayout.Button("Clear Accumulation")) ClearAccumulatedFiles();
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

        /// <summary>
        ///     This function imports FBX and configures the custom model importer to change the FBX importer settings to suit to
        ///     this reader's needs.
        /// </summary>
        /// <param name="fullOsPath">Full path to fbx. Do not use assetDatabase path.</param>
        public static void ImportFbxIntoUnityAndProcessIt(string fullOsPath)
        {
            var strippedFfileName = Path.GetFileName(fullOsPath);


            if (!Directory.Exists(IT_AnimotiveImporterEditorConstants.UnityFilesCharactersDirectory))
                Directory.CreateDirectory(IT_AnimotiveImporterEditorConstants.UnityFilesCharactersDirectory);

            var fullPathToSaveFbx = Path.Combine(IT_AnimotiveImporterEditorConstants.UnityFilesCharactersDirectory,
                strippedFfileName);

            if (File.Exists(fullPathToSaveFbx))
            {
                if (!_reimportAssets)
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

        /// <summary>
        ///     This function checks if there's FBXes under their relative folder and if so, checks if there's only one.
        /// </summary>
        /// <param name="unityExportPath">Path to user browsed and selected folder usually called "UnityExported" </param>
        /// <returns></returns>
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

        /// <summary>
        ///     Moves audio files into Unity editor and sorts them.
        /// </summary>
        /// <param name="unityExportPath">Path to user browsed and selected folder usually called "UnityExported" </param>
        /// <returns></returns>
        private static Task MoveAudiosIntoUnity(string unityExportPath)
        {
            var charactersPath = Path.Combine(unityExportPath, "Clips");

            var files = Directory.GetFiles(charactersPath)
                .Where(a => !a.EndsWith(".meta") &&
                            a.ToLower().EndsWith(IT_AnimotiveImporterEditorConstants.AudioExtension)).ToList();

            if (!Directory.Exists(IT_AnimotiveImporterEditorConstants.UnityFilesAudioDirectory))
                Directory.CreateDirectory(IT_AnimotiveImporterEditorConstants.UnityFilesAudioDirectory);
            for (var i = 0; i < files.Count; i++)
            {
                var fileName = Path.GetFileName(files[i]);
                var targetFileName =
                    Path.Combine(IT_AnimotiveImporterEditorConstants.UnityFilesAudioDirectory, fileName);

                if (File.Exists(targetFileName))
                {
                    targetFileName = IT_AnimotiveImporterEditorUtilities.GetLatestSimilarFileName(
                        IT_AnimotiveImporterEditorConstants.UnityFilesAudioDirectory, files[i], fileName,
                        IT_AnimotiveImporterEditorConstants.AudioExtension);
                }

                File.Copy(files[i], targetFileName, false);
            }

            AssetDatabase.Refresh();
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Deletes all accumulated files such as; Scenes, audios, animations and playables. But doesn't delete characters
        /// </summary>
        private static void ClearAccumulatedFiles()
        {
            string[] directories =
            {
                IT_AnimotiveImporterEditorConstants.UnityFilesAnimationDirectory,
                IT_AnimotiveImporterEditorConstants.UnityFilesPlayablesDirectory,
                IT_AnimotiveImporterEditorConstants.UnityFilesScenesDirectory,
                IT_AnimotiveImporterEditorConstants.UnityFilesAudioDirectory
            };

            for (var i = 0; i < directories.Length; i++)
            {
                Directory.Delete(directories[i], true);
            }


            ResetWindow();
            AssetDatabase.Refresh();
        }


        /// <summary>
        ///     Resets plugin window to it's default state
        /// </summary>
        private static void ResetWindow()
        {
            UserChosenDirectoryToImportUnityExports = "";
            _isAnimotiveFolderImported = false;
            _reimportAssets = false;
        }
    }
}