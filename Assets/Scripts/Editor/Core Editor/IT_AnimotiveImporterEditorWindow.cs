using System.Diagnostics;
using System.IO;
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
                await IT_AnimotiveImporterEditorUtilities.MoveAudiosIntoUnity(UserChosenDirectoryToImportUnityExports);
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

                await IT_EntityOperations.HandleEntityOperations(sceneData, groupDatas);

                //create timeline using animation clips
                await IT_AnimotiveImporterEditorTimeline.HandleTimeLineOperations(groupDatas, fbxDatasAndHoldersTuples,
                    sceneData);

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

            if (GUILayout.Button("Clear Accumulation")) IT_AnimotiveImporterEditorUtilities.ClearAccumulatedFiles();
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
        ///     Resets plugin window to it's default state
        /// </summary>
        internal static void ResetWindow()
        {
            UserChosenDirectoryToImportUnityExports = "";
            _isAnimotiveFolderImported = false;
            ReimportAssets = false;
        }
    }
}