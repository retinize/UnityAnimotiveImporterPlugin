using System;

namespace Retinize.Editor.AnimotiveImporter
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public class IT_AnimotiveImporterEditorWindow : EditorWindow
    {
        
        
        private static bool _DisableImport = true;
        private static string _UserChosenDirectoryToImportUnityExports = "";
        private static string _UserChosenDirectoryToImportCharacterFbxModels = "";
        public static string ImportedFbxAssetDatabasePath = "";

        public static bool EnableImportConfig = false;
        
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
                        Debug.Log("Imported the Animotive files successfully !");
                        // _DisableImport = false;
                    }
                    else
                    {
                        _DisableImport = true;

                        EditorUtility.DisplayDialog(IT_AnimotiveImporterEditorConstants.WarningTitle,
                            "The folder you chose is not a valid Animotive Export folder ! ", "OK");
                    }
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
                        fullPathToSaveFbx,true);
                    ImportedFbxAssetDatabasePath = fullPathToSaveFbx.Split(new string[] { "Assets" }, StringSplitOptions.None)[1];
                    ImportedFbxAssetDatabasePath = string.Concat("Assets" , ImportedFbxAssetDatabasePath);
                    
                    
                    AssetDatabase.Refresh();

                    EnableImportConfig = false;
   
                    
                    Debug.Log("Imported the Character model successfully !");
                    _DisableImport = false;

                }
               
                
            }

            GUILayout.EndHorizontal();

            #endregion

            #region Import Animotive Button

            EditorGUI.BeginDisabledGroup(_DisableImport);

            if (GUILayout.Button("Import Animotive"))
            {
                IT_SceneEditor.CreateScene("___scene_name_here___");

                var clipsPath = Path.Combine(_UserChosenDirectoryToImportUnityExports, "Clips");
                var animationClipDataPath =
                    IT_AnimotiveImporterEditorUtilities.ReturnClipDataFromPath(clipsPath);


                var sceneData = IT_SceneDataOperations.LoadSceneData(clipsPath);
                Debug.Log(sceneData.currentSetName);

                var groupInfos = new List<IT_AnimotiveImporterEditorGroupInfo>(1);
                var animationClipObj =
                    IT_TransformAnimationClipEditor.HandleBodyAnimationClipOperations(animationClipDataPath);

                var animationGroup =
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
            var window = GetWindow<IT_AnimotiveImporterEditorWindow>("Example");
            window.Show();
        }
    }
}