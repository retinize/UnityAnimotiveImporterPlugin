using System.IO;
using UnityEditor;
using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    public class IT_AnimotiveImporterEditorWindow : EditorWindow
    {
        public static string ImportedCharactersAssetdatabaseDirectory = Path.Combine(Directory.GetCurrentDirectory(),
            "Assets",
            "Imported Files", "Characters");

        private static bool _DisableImport;
        private static bool _IsAnimotiveFolderImported;

        private static string _UserChosenDirectoryToImportUnityExports = "";

        public static bool EnableImportConfig;
        private static bool _ReimportFbxs;

        private async void OnGUI()
        {
            #region Choose Animotive folder

            GUILayout.BeginHorizontal();


            EditorGUILayout.TextField("Animotive Export Folder :", _UserChosenDirectoryToImportUnityExports);

            if (GUILayout.Button("Choose Folder to Import"))
            {
                var choosenFolder = EditorUtility.OpenFolderPanel("Import Animotive into Unity ",
                    Directory.GetCurrentDirectory(), "");

                if (!string.IsNullOrEmpty(choosenFolder))
                {
                    if (IT_AnimotiveImporterEditorUtilities.IsFolderInCorrectFormatToImport(choosenFolder))
                    {
                        _UserChosenDirectoryToImportUnityExports = choosenFolder;
                        var fbxes = CheckCharacterFbxs(_UserChosenDirectoryToImportUnityExports);

                        if (fbxes) Debug.Log("Imported the Animotive files successfully !");
                        _UserChosenDirectoryToImportUnityExports = fbxes ? choosenFolder : "";
                        _IsAnimotiveFolderImported = fbxes;
                    }
                    else
                    {
                        _IsAnimotiveFolderImported = false;

                        EditorUtility.DisplayDialog(IT_AnimotiveImporterEditorConstants.WarningTitle,
                            "The folder you chose is not a valid Animotive Export folder ! ", "OK");
                    }
                }
                else
                    _IsAnimotiveFolderImported = false;
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Reimport Always");
            _ReimportFbxs = EditorGUILayout.Toggle(_ReimportFbxs);

            GUILayout.EndHorizontal();

            #endregion


            #region Import Animotive Button

            _DisableImport = _IsAnimotiveFolderImported;
            EditorGUI.BeginDisabledGroup(!_DisableImport);

            if (GUILayout.Button("Import Animotive"))
            {
                var clipsPath = Path.Combine(_UserChosenDirectoryToImportUnityExports, "Clips");

                var sceneData = IT_SceneDataOperations.LoadSceneData(clipsPath);
                IT_SceneEditor.CreateScene(sceneData.currentSetName);

                await IT_TransformAnimationClipEditor.HandleBodyAnimationClipOperations(sceneData, clipsPath);
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
            var window = GetWindow<IT_AnimotiveImporterEditorWindow>("Animotive Reader");
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
                if (!_ReimportFbxs)
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

        private static bool CheckCharacterFbxs(string unityExportPath)
        {
            var charactersPath = Path.Combine(unityExportPath, "EntityAssets", "Characters");
            var characterFolders = Directory.GetDirectories(charactersPath);

            if (characterFolders.Length == 0) return false;


            for (var i = 0; i < characterFolders.Length; i++)
            {
                var folder = characterFolders[i];
                var characterName = Path.GetFileName(folder);

                var characterNameWithExtension = string.Concat(characterName, ".fbx");
                var fullPathToExpectedFbx = Path.Combine(folder, characterNameWithExtension);
                var existence = File.Exists(fullPathToExpectedFbx);
                if (!existence)
                {
                    EditorUtility.DisplayDialog(IT_AnimotiveImporterEditorConstants.WarningTitle,
                        $@"Couldn't find {characterNameWithExtension} at the directory: {folder}", "OK");
                    return false;
                }

                ImportFbxIntoUnityAndProcessIt(fullPathToExpectedFbx);
            }

            return true;
        }
    }
}