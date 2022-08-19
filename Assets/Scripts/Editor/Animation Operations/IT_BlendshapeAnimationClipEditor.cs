using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AnimotiveImporterDLL;
using UnityEditor;
using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    //TODO: Add support for facial animation.
    public static class IT_BlendshapeAnimationClipEditor
    {
        /// <summary>
        ///     Reads blendShape animation values from json at it's designated path.
        ///     that
        /// </summary>
        /// <returns>Blendshape value read from the json in type of 'FacialAnimationExportWrapper' </returns>
        private static FacialAnimationExportWrapper HandleBlendShapeAnimationCreation(
            string facialAnimationAssetDatabasePath)
        {
            var jsonFileSystemFullPath = string.Concat(Directory.GetCurrentDirectory(),
                facialAnimationAssetDatabasePath);

            var reader = new StreamReader(jsonFileSystemFullPath);
            var jsonData = reader.ReadToEnd();

            reader.Close();
            reader.Dispose();
            var clip = JsonUtility.FromJson<FacialAnimationExportWrapper>(jsonData);
            return clip;
        }

        /// <summary>
        ///     Creates blendShape Animation Clip at the designated directory.
        /// </summary>
        /// <param name="clip">clip data read from json and casted to 'FacialAnimationExportWrapper'</param>
        /// <param name="itFbxData">
        ///     Tuple of character to apply animation . Tuple contains GameObject which is root of the character
        ///     and the animator of the character.
        /// </param>
        private static AnimationClip CreateBlendShapeAnimationClip(FacialAnimationExportWrapper clip,
            IT_FbxData itFbxData)
        {
            var animationClip = new AnimationClip();

            var blendshapeCurves =
                new Dictionary<string, AnimationCurve>(clip.characterGeos.Count);

            for (var i = 0; i < clip.characterGeos.Count; i++)
            {
                blendshapeCurves.Add(clip.characterGeos[i].name, new AnimationCurve());
            }

            for (var i = 0; i < clip.facialAnimationFrames.Count; i++)
            {
                var time = i * clip.fixedDeltaTimeBetweenKeyFrames;
                for (var j = 0; j < clip.facialAnimationFrames[i].blendShapesUsed.Count; j++)
                {
                    var
                        blendShapeData = clip.facialAnimationFrames[i].blendShapesUsed[j];

                    var characterGeoDescriptor = clip.characterGeos[blendShapeData.geo];

                    var skinnedMeshRendererName = characterGeoDescriptor.name;

                    var tr = itFbxData.FbxGameObject.transform.FindChildRecursively(skinnedMeshRendererName);
                    var skinnedMeshRenderer = tr.gameObject.GetComponent<SkinnedMeshRenderer>();
                    var blendshapeName = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(blendShapeData.bsIndex);
                    var blendshapeValue = blendShapeData.value;
                    var keyframe = new Keyframe(time, blendshapeValue);

                    var relativePath = AnimationUtility.CalculateTransformPath(tr, itFbxData.FbxGameObject.transform);
                    var curve = blendshapeCurves[skinnedMeshRendererName];
                    curve.AddKey(keyframe);

                    var propertyName = string.Concat("blendShape.", blendshapeName);
                    animationClip.SetCurve(relativePath, typeof(SkinnedMeshRenderer), propertyName,
                        curve);
                }
            }


            itFbxData.FbxAnimator.avatar = null;
            AssetDatabase.CreateAsset(animationClip, IT_AnimotiveImporterEditorConstants.FacialAnimationCreatedPath);
            AssetDatabase.Refresh();

            return animationClip;
        }

        /// <summary>
        ///     Triggers all the necessary methods for the related animation clip creation PoC
        /// </summary>
        public static async Task HandleFacialAnimationOperations(List<IT_GroupData> groupDatas,
            Dictionary<string, IT_FbxDatasAndHoldersTuple> fbxDatasAndHoldersTuples)
        {
            // var wrapper =
            //     HandleBlendShapeAnimationCreation(jsonFileAssetDbPath);
            // var clip = CreateBlendShapeAnimationClip(wrapper, fbxData);

            AssetDatabase.Refresh();
        }
    }
}