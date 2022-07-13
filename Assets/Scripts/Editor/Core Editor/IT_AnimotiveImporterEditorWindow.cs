using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    public class IT_AnimotiveImporterEditorWindow : EditorWindow
    {
        private static bool _DisableImport;
        private static bool _IsAnimotiveFolderImported;
        private static bool _IsModelImported;

        private static string _UserChosenDirectoryToImportUnityExports = "";
        private static string _UserChosenDirectoryToImportCharacterFbxModels = "";
        public static string ImportedFbxAssetDatabasePath = "";

        public static bool EnableImportConfig;

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
                        Debug.Log("Selected the Animotive files successfully !");
                        _IsAnimotiveFolderImported = true;
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

            #endregion

            #region Choose Character FBX

            GUILayout.BeginHorizontal();

            EditorGUILayout.TextField("Character FBX Folder :", _UserChosenDirectoryToImportCharacterFbxModels);

            if (GUILayout.Button("Import Character Model(FBX)"))
            {
                _UserChosenDirectoryToImportCharacterFbxModels =
                    EditorUtility.OpenFilePanel("Import Character into Unity",
                        Directory.GetCurrentDirectory(), "fbx");


                if (!string.IsNullOrEmpty(_UserChosenDirectoryToImportCharacterFbxModels))
                {
                    if (!_UserChosenDirectoryToImportCharacterFbxModels.ToLower().EndsWith(".fbx"))
                    {
                        EditorUtility.DisplayDialog(IT_AnimotiveImporterEditorConstants.WarningTitle,
                            "Can't import models other than fbx ! ", "OK");

                        _IsModelImported = false;
                    }
                    else
                    {
                        ImportFbxIntoUnityAndProcessIt();

                        if (_IsModelImported) Debug.Log("Selected the Character model successfully !");
                    }
                }
                else
                    _IsModelImported = false;
            }

            GUILayout.EndHorizontal();

            #endregion

            #region Import Animotive Button

            _DisableImport = _IsModelImported & _IsAnimotiveFolderImported;
            EditorGUI.BeginDisabledGroup(!_DisableImport);

            if (GUILayout.Button("Import Animotive"))
            {
                var clipsPath = Path.Combine(_UserChosenDirectoryToImportUnityExports, "Clips");

                var sceneData = IT_SceneDataOperations.LoadSceneData(clipsPath);

                IT_SceneEditor.CreateScene(sceneData.currentSetName);


                var fbxData = IT_AnimotiveImporterEditorUtilities.LoadFbx();
                await IT_TransformAnimationClipEditor.HandleBodyAnimationClipOperations(sceneData, clipsPath,
                    fbxData);
                fbxData.FbxAnimator.avatar = null;
            }


            EditorGUI.EndDisabledGroup();

            #endregion

            // if (GUILayout.Button("Test Animation Clip"))
            // {
            // }
            //
            // if (GUILayout.Button("Test Json BlendShape"))
            // {
            //     IT_BlendshapeAnimationClipEditor.HandleFacialAnimationOperations();
            // }
        }

        /// <summary>
        ///     Function to show EditorWindow in UnityEditor.
        /// </summary>
        [MenuItem("Animotive/Importer")]
        public static void ShowWindow()
        {
            var window = GetWindow<IT_AnimotiveImporterEditorWindow>("Example");
            window.Show();
        }


        public static async void ImportFbxIntoUnityAndProcessIt()
        {
            var strippedFfileName =
                Path.GetFileName(_UserChosenDirectoryToImportCharacterFbxModels);

            var targetDirectoryToSaveFbx = Path.Combine(Directory.GetCurrentDirectory(), "Assets",
                "Imported Files", "Characters");

            if (!Directory.Exists(targetDirectoryToSaveFbx))
                Directory.CreateDirectory(targetDirectoryToSaveFbx);

            var fullPathToSaveFbx =
                Path.Combine(targetDirectoryToSaveFbx, strippedFfileName);

            if (File.Exists(fullPathToSaveFbx))
            {
                var option = EditorUtility.DisplayDialogComplex(IT_AnimotiveImporterEditorConstants.WarningTitle,
                    "The selected model is already present in the current project !", "Reimport", "Cancel",
                    "Continue with existing");

                switch (option)
                {
                    case 0:
                    {
                        await Task.Run(delegate { File.Delete(fullPathToSaveFbx); });

                        AssetDatabase.Refresh();

                        EnableImportConfig = true;
                        await Task.Run(delegate
                        {
                            File.Copy(_UserChosenDirectoryToImportCharacterFbxModels,
                                fullPathToSaveFbx, true);
                        });
                        SetImportedFbxAssetDatabasePathVariable(fullPathToSaveFbx);


                        AssetDatabase.Refresh();

                        EnableImportConfig = false;

                        _IsModelImported = true;
                    }
                        break;
                    case 1:
                        _UserChosenDirectoryToImportCharacterFbxModels = "";
                        _IsModelImported = false;
                        break;
                    case 2:
                    {
                        SetImportedFbxAssetDatabasePathVariable(fullPathToSaveFbx);
                        _IsModelImported = true;
                    }
                        break;
                    default:
                        Debug.LogError("Unknown option !");
                        break;
                }
            }
            else
            {
                EnableImportConfig = true;
                await Task.Run(delegate
                {
                    File.Copy(_UserChosenDirectoryToImportCharacterFbxModels, fullPathToSaveFbx, true);
                });

                SetImportedFbxAssetDatabasePathVariable(fullPathToSaveFbx);
                _IsModelImported = true;
                AssetDatabase.Refresh();
                EnableImportConfig = false;
            }

            AssetDatabase.Refresh();
        }

        private static void SetImportedFbxAssetDatabasePathVariable(string fullPathToSaveFbx)
        {
            ImportedFbxAssetDatabasePath =
                fullPathToSaveFbx.Split(new[] { "Assets" }, StringSplitOptions.None)[1];

            ImportedFbxAssetDatabasePath = string.Concat("Assets", ImportedFbxAssetDatabasePath);
        }
    }
}