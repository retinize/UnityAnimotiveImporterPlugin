using System;
using System.Collections.Generic;
using System.IO;
using AnimotiveImporterDLL;
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

        private void OnGUI()
        {
            #region Choose Animotive folder

            GUILayout.BeginHorizontal();


            EditorGUILayout.TextField("Animotive Export Folder :", _UserChosenDirectoryToImportUnityExports);

            if (GUILayout.Button("Choose Folder to Import"))
            {
                string choosenFolder = EditorUtility.OpenFolderPanel("Import Animotive into Unity ",
                    Directory.GetCurrentDirectory(), "");

                if (!string.IsNullOrEmpty(choosenFolder))
                {
                    if (IT_AnimotiveImporterEditorUtilities.IsFolderInCorrectFormatToImport(choosenFolder))
                    {
                        _UserChosenDirectoryToImportUnityExports = choosenFolder;
                        Debug.Log("Imported the Animotive files successfully !");
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
                {
                    _IsAnimotiveFolderImported = false;
                }
            }

            GUILayout.EndHorizontal();

            #endregion

            #region Choose Character FBX

            GUILayout.BeginHorizontal();

            EditorGUILayout.TextField("Character FBX Folder :", _UserChosenDirectoryToImportCharacterFbxModels);

            if (GUILayout.Button("Import Character Model"))
            {
                _UserChosenDirectoryToImportCharacterFbxModels =
                    EditorUtility.OpenFilePanel("Import Character into Unity",
                        Directory.GetCurrentDirectory(), "*.fbx");

                if (!string.IsNullOrEmpty(_UserChosenDirectoryToImportCharacterFbxModels))
                {
                    string strippedFfileNameWithoutExtension =
                        Path.GetFileNameWithoutExtension(_UserChosenDirectoryToImportCharacterFbxModels);

                    string targetDirectoryToSaveFbx = Path.Combine(Directory.GetCurrentDirectory(), "Assets",
                        "Imported Files", "Characters");

                    if (!Directory.Exists(targetDirectoryToSaveFbx))
                    {
                        Directory.CreateDirectory(targetDirectoryToSaveFbx);
                    }

                    string fullPathToSaveFbx =
                        Path.Combine(targetDirectoryToSaveFbx, strippedFfileNameWithoutExtension);

                    EnableImportConfig = true;
                    File.Copy(_UserChosenDirectoryToImportCharacterFbxModels,
                        fullPathToSaveFbx, true);
                    ImportedFbxAssetDatabasePath =
                        fullPathToSaveFbx.Split(new[] { "Assets" }, StringSplitOptions.None)[1];
                    ImportedFbxAssetDatabasePath = string.Concat("Assets", ImportedFbxAssetDatabasePath);


                    AssetDatabase.Refresh();

                    EnableImportConfig = false;

                    _IsModelImported = true;
                    Debug.Log("Imported the Character model successfully !");
                }
                else
                {
                    _IsModelImported = false;
                }
            }

            GUILayout.EndHorizontal();

            #endregion

            #region Import Animotive Button

            _DisableImport = _IsModelImported & _IsAnimotiveFolderImported;
            EditorGUI.BeginDisabledGroup(!_DisableImport);

            if (GUILayout.Button("Import Animotive"))
            {
                string clipsPath = Path.Combine(_UserChosenDirectoryToImportUnityExports, "Clips");
                string animationClipDataPath =
                    IT_AnimotiveImporterEditorUtilities.ReturnClipDataFromPath(clipsPath);

                IT_SceneInternalData sceneData = IT_SceneDataOperations.LoadSceneData(clipsPath);
                Debug.Log(sceneData.currentSetName);

                IT_SceneEditor.CreateScene(sceneData.currentSetName);

                List<IT_AnimotiveImporterEditorGroupInfo> groupInfos = new List<IT_AnimotiveImporterEditorGroupInfo>(1);
                GameObject animationClipObj =
                    IT_TransformAnimationClipEditor.HandleBodyAnimationClipOperations(animationClipDataPath);

                IT_AnimotiveImporterEditorGroupInfo animationGroup =
                    new IT_AnimotiveImporterEditorGroupInfo(IT_TransformAnimationClipEditor.bodyAnimationName,
                        animationClipObj);

                groupInfos.Add(animationGroup);


                IT_AnimotiveImporterEditorTimeline.HandleGroups(groupInfos);
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
            IT_AnimotiveImporterEditorWindow window = GetWindow<IT_AnimotiveImporterEditorWindow>("Example");
            window.Show();
        }
    }
}