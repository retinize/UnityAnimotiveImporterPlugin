using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            string jsonFileSystemFullPath)
        {
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
            Dictionary<string, IT_FbxDatasAndHoldersTuple> fbxDatasAndHoldersTuples, string clipsFolderPath)
        {
            // var clip = CreateBlendShapeAnimationClip(wrapper, fbxData);

            // IT_FixedCharacterPropertyClip

            var blendshapesDictionary = await GetAllFacialAnimations(clipsFolderPath);

            for (var i = 0; i < groupDatas.Count; i++)
            {
                var groupData = groupDatas[i];
                for (var j = 0; j < groupData.TakeDatas.Count; j++)
                {
                    var takeData = groupData.TakeDatas[j];
                    for (var k = 0; k < takeData.Clusters.Count; k++)
                    {
                        var cluster = takeData.Clusters[k];
                        var groupName = groupData.GroupName;
                        var takeIndex = takeData.TakeIndex;
                        var clipNumber = cluster.NumberOfCaptureInWhichItWasCaptured;
                    }
                }
            }

            AssetDatabase.Refresh();
        }

        private static async Task<Dictionary<string, FacialAnimationExportWrapper>> GetAllFacialAnimations(
            string clipsFolder)
        {
            var jsonFiles = Directory.GetFiles(clipsFolder).Where(a =>
                a.EndsWith(".json") && IT_AnimotiveImporterEditorUtilities.GetClipTypeFromClipName(a) ==
                IT_ClipType.FacialAnimationClip).ToList();

            var blendShapesFullPathAndWrappers = new Dictionary<string, FacialAnimationExportWrapper>();

            for (var i = 0; i < jsonFiles.Count; i++)
            {
                var jsonFileOsPath = jsonFiles[i];
                var wrapper =
                    HandleBlendShapeAnimationCreation(jsonFileOsPath);
                blendShapesFullPathAndWrappers.Add(jsonFileOsPath, wrapper);
            }

            return blendShapesFullPathAndWrappers;
        }
    }
}