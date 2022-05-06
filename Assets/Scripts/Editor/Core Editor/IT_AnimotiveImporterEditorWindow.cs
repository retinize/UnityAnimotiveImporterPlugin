namespace AnimotiveImporterEditor
{
    using System;
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
                Tuple<GameObject, Animator> fbxTuple = IT_AnimotiveImporterEditorUtilities.LoadFbx();

                Tuple<IT_CharacterTransformAnimationClip, Tuple<Dictionary<HumanBodyBones, Transform>,
                    Dictionary<Transform, HumanBodyBones>>> clipAndDictionariesTuple =
                    IT_TransformAnimationClipEditor.PrepareAndGetAnimationData(fbxTuple);

                IT_AnimotiveImporterEditorUtilities
                    .DeleteAssetIfExists(IT_AnimotiveImporterEditorConstants.TransformAnimPath,
                                         typeof(AnimationClip));
                IT_TransformAnimationClipEditor.CreateTransformMovementsAnimationClip(clipAndDictionariesTuple,
                 fbxTuple.Item1);
            }

            if (GUILayout.Button("Test Json BlendShape"))
            {
                FacialAnimationExportWrapper wrapper =
                    IT_BlendshapeAnimationClipEditor.HandleBlendShapeAnimationCreation();
                IT_BlendshapeAnimationClipEditor.CreateBlendShapeAnimationClip(wrapper,
                                                                               IT_AnimotiveImporterEditorUtilities
                                                                                   .LoadFbx());
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