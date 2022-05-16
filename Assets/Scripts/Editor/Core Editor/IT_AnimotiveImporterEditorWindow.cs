namespace Retinize.Editor.AnimotiveImporter
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class IT_AnimotiveImporterEditorWindow : EditorWindow
    {
        private void OnGUI()
        {
            if (GUILayout.Button("Create scene and playables"))

            {
                IT_SceneEditor.CreateScene("___scene_name_here___");

                IT_AnimotiveImporterEditorTimeline.HandleGroups(new List<IT_AnimotiveImporterEditorGroupInfo>
                {
                    new IT_AnimotiveImporterEditorGroupInfo(),
                    new IT_AnimotiveImporterEditorGroupInfo(),
                    new IT_AnimotiveImporterEditorGroupInfo(),
                    new IT_AnimotiveImporterEditorGroupInfo(),
                    new IT_AnimotiveImporterEditorGroupInfo(),
                    new IT_AnimotiveImporterEditorGroupInfo(),
                    new IT_AnimotiveImporterEditorGroupInfo()
                });
            }

            if (GUILayout.Button("Test Animation Clip"))
            {
                IT_TransformAnimationClipEditor.HandleTransformAnimationClipOperations();
            }

            if (GUILayout.Button("Test Json BlendShape"))
            {
                IT_BlendshapeAnimationClipEditor.HandleBlendShapeAnimationOperations();
            }
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