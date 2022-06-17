namespace Retinize.Editor.AnimotiveImporter
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public class IT_AnimotiveImporterEditorWindow : EditorWindow
    {
        private static bool _DisableImport = true;
        private static string _UserChosenDirectoryToImport = "";

        private async void OnGUI()
        {
            GUILayout.BeginHorizontal();


            EditorGUILayout.TextField("Animotive Export Folder :", _UserChosenDirectoryToImport);

            if (GUILayout.Button("Choose Folder to Import"))
            {
                _UserChosenDirectoryToImport = EditorUtility.OpenFolderPanel("Open Animotive ",
                    string.Concat(Directory.GetCurrentDirectory(), @"\Assets\"), "");

                if (!string.IsNullOrEmpty(_UserChosenDirectoryToImport))
                {
                    if (IT_AnimotiveImporterEditorUtilities.IsFolderInCorrectFormatToImport(
                            _UserChosenDirectoryToImport))
                        _DisableImport = false;
                    else
                    {
                        _DisableImport = true;
                        Debug.LogWarning("The folder you chose is not a valid Animotive Export folder ! ");
                    }
                }
                else
                {
                    _DisableImport = true;
                    Debug.LogWarning("Please choose a folder to import !");
                }
            }

            GUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(_DisableImport);

            if (GUILayout.Button("Import Animotive"))
            {
                IT_SceneEditor.CreateScene("___scene_name_here___");

                var clipsPath = Path.Combine(_UserChosenDirectoryToImport, "Clips");
                var animationClipDataPath =
                    IT_AnimotiveImporterEditorUtilities.ReturnClipDataFromPath(clipsPath);


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