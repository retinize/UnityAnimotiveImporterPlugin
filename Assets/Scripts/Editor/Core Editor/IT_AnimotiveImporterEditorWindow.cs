using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Retinize.Editor.AnimotiveImporter
{
    public class IT_AnimotiveImporterEditorWindow : EditorWindow
    {
        private static bool _disableImport;
        private static bool _isAnimotiveFolderImported;

        public static string UserChosenDirectoryToImportUnityExports = "";

        public static bool EnableImportConfig;
        public static bool ReimportAssets { get; private set; }

        public async void OnGUI()
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

            #endregion


            #region Import Animotive Scene

            var isCharactersFolderEmpty = IT_AnimotiveImporterEditorUtilities.IsCharactersFolderEmpty();

            if (isCharactersFolderEmpty && _isAnimotiveFolderImported)
                Debug.LogError("No character found under Characters folder. Can't start the process...");

            _disableImport = _isAnimotiveFolderImported && !isCharactersFolderEmpty;
            EditorGUI.BeginDisabledGroup(!_disableImport);
            var sw = new Stopwatch();
            if (GUILayout.Button("Import Animotive Scene"))
            {
                sw.Start();
                await MoveAudiosIntoUnity(UserChosenDirectoryToImportUnityExports);
                await Task.Yield();

                var clipsFolderPath = Path.Combine(UserChosenDirectoryToImportUnityExports, "Clips");

                var sceneData = IT_SceneDataOperations.LoadSceneData(UserChosenDirectoryToImportUnityExports);
                var scene = await IT_SceneEditor.CreateScene(sceneData.currentSetName);

                AssetDatabase.Refresh();

                var groupDatas =
                    await IT_AnimotiveImporterEditorUtilities.GetGroupDataListByType(sceneData, clipsFolderPath);
                await Task.Yield();

                var fbxDatasAndHoldersTuples = IT_FbxOperations.GetFbxDataAndHolders(groupDatas);


                //create animation clips
                await IT_BodyAnimationClipEditor.HandleBodyAnimationClipOperations(
                    groupDatas,
                    fbxDatasAndHoldersTuples);
                AssetDatabase.Refresh();
                await Task.Yield();


                await IT_BlendshapeAnimationClipEditor.HandleFacialAnimationOperations(groupDatas,
                    fbxDatasAndHoldersTuples, clipsFolderPath);
                await Task.Yield();

                IT_EntityOperations.HandleEntityOperations(sceneData);


                //create timeline using animation clips
                IT_AnimotiveImporterEditorTimeline.HandleGroups(groupDatas, fbxDatasAndHoldersTuples, sceneData);

                EditorSceneManager.SaveScene(scene);

                AssetDatabase.Refresh();
            }

            if (sw.IsRunning && !EditorApplication.isUpdating)
            {
                sw.Stop();
                Debug.Log(string.Concat(sw.Elapsed.Minutes, " minutes  ", sw.Elapsed.Seconds, " seconds"));
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
            return Task.Run(async delegate
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
                        targetFileName = await IT_AnimotiveImporterEditorUtilities.GetLatestSimilarFileName(
                            IT_AnimotiveImporterEditorConstants.UnityFilesAudioDirectory, files[i], fileName,
                            IT_AnimotiveImporterEditorConstants.AudioExtension);
                    }

                    File.Copy(files[i], targetFileName, false);
                }
            });
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