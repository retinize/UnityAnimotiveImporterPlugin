namespace AnimotiveImporterEditor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public static class IT_BlendshapeAnimationClipEditor
    {
        /// <summary>
        ///     Reads blendShape animation values from json at it's designated path.
        ///     that
        /// </summary>
        /// <returns>Blendshape value read from the json in type of 'FacialAnimationExportWrapper' </returns>
        internal static FacialAnimationExportWrapper HandleBlendShapeAnimationCreation()
        {
            string hardCodedJsonPath = string.Concat(Directory.GetCurrentDirectory(),
                                                     IT_AnimotiveImporterEditorConstants.BlendshapeJsonPath);

            StreamReader reader   = new StreamReader(hardCodedJsonPath);
            string       jsonData = reader.ReadToEnd();

            reader.Close();
            reader.Dispose();
            FacialAnimationExportWrapper clip = JsonUtility.FromJson<FacialAnimationExportWrapper>(jsonData);
            return clip;
        }

        /// <summary>
        ///     Creates blendShape Animation Clip at the designated directory.
        /// </summary>
        /// <param name="clip">clip data read from json and casted to 'FacialAnimationExportWrapper'</param>
        /// <param name="tuple">
        ///     Tuple of character to apply animation . Tuple contains GameObject which is root of the character
        ///     and the animator of the character.
        /// </param>
        internal static void CreateBlendShapeAnimationClip(FacialAnimationExportWrapper clip,
                                                           Tuple<GameObject, Animator>  tuple)
        {
            AnimationClip animationClip = new AnimationClip();

            Dictionary<string, AnimationCurve> blendshapeCurves =
                new Dictionary<string, AnimationCurve>(clip.characterGeos.Count);

            for (int i = 0; i < clip.characterGeos.Count; i++)
            {
                blendshapeCurves.Add(clip.characterGeos[i].name, new AnimationCurve());
            }

            for (int i = 0; i < clip.facialAnimationFrames.Count; i++)
            {
                float time = i * clip.fixedDeltaTimeBetweenKeyFrames;
                for (int j = 0; j < clip.facialAnimationFrames[i].blendShapesUsed.Count; j++)
                {
                    IT_SpecificCharacterBlendShapeData
                        blendShapeData = clip.facialAnimationFrames[i].blendShapesUsed[j];

                    CharacterGeoDescriptor characterGeoDescriptor = clip.characterGeos[blendShapeData.geo];

                    string skinnedMeshRendererName = characterGeoDescriptor.name;

                    Transform           tr = tuple.Item1.transform.FindChildRecursively(skinnedMeshRendererName);
                    SkinnedMeshRenderer skinnedMeshRenderer = tr.gameObject.GetComponent<SkinnedMeshRenderer>();

                    string   blendshapeName  = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(blendShapeData.bsIndex);
                    float    blendshapeValue = blendShapeData.value;
                    Keyframe keyframe        = new Keyframe(time, blendshapeValue);

                    string relativePath = AnimationUtility.CalculateTransformPath(tr, tuple.Item1.transform);


                    AnimationCurve curve = blendshapeCurves[skinnedMeshRendererName];
                    curve.AddKey(keyframe);


                    string propertyName = string.Concat("blendShape.", blendshapeName);
                    animationClip.SetCurve(relativePath, typeof(SkinnedMeshRenderer), propertyName,
                                           curve);
                }
            }


            tuple.Item2.avatar = null;
            AssetDatabase.CreateAsset(animationClip, IT_AnimotiveImporterEditorConstants.BlendShapeAnimCreatedPath);
            AssetDatabase.Refresh();
        }
    }
}