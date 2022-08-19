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
        public static bool ReimportAssets { get; private set; }
        private static bool _disableImport;
        private static bool _isAnimotiveFolderImported;

        public static string UserChosenDirectoryToImportUnityExports = "";

        public static bool EnableImportConfig;

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

                        var fbxes = await IT_FbxOperations.CheckCharacterFbxs(UserChosenDirectoryToImportUnityExports);

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
            ReimportAssets = EditorGUILayout.Toggle(ReimportAssets);

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

                var sceneData = IT_SceneDataOperations.LoadSceneData(UserChosenDirectoryToImportUnityExports);
                var scene = IT_SceneEditor.CreateScene(sceneData.currentSetName);

                var groupDatas =
                    IT_AnimotiveImporterEditorUtilities.GetGroupDataListByType(sceneData, clipsFolderPath);
                var fbxDatasAndHoldersTuples = IT_FbxOperations.GetFbxDataAndHolders(groupDatas);


                //create animation clips
                var animationClipOperations =
                    await IT_BodyAnimationClipEditor.HandleBodyAnimationClipOperations(groupDatas,
                        fbxDatasAndHoldersTuples);
                await IT_BlendshapeAnimationClipEditor.HandleFacialAnimationOperations(groupDatas,
                    fbxDatasAndHoldersTuples);

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
            ReimportAssets = false;
        }
    }
}