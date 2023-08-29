using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AnimotiveImporterDLL;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    public static class IT_BlendshapeAnimationClipEditor
    {
        /// <summary>
        ///     Reads blendShape animation values from json at it's designated path.
        ///     that
        /// </summary>
        /// <returns>Blendshape value read from the json in type of 'FacialAnimationExportWrapper' </returns>
        private static async Task<FacialAnimationExportWrapper> GetBlendShapeAnimationDataFromFile(
            string jsonFileSystemFullPath)
        {
            FacialAnimationExportWrapper clip = null;
            await Task.Run(delegate
            {
                var json = File.ReadAllText(jsonFileSystemFullPath);
                clip = JsonConvert.DeserializeObject<FacialAnimationExportWrapper>(json);
            });

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
        /// <param name="fileNameWithoutExtension"></param>
        private static async void CreateBlendShapeAnimationClip(FacialAnimationExportWrapper clip,
            IT_FbxData itFbxData, string fileNameWithoutExtension)
        {
            var animationClip = new AnimationClip();
            var blendshapeCurves = new Dictionary<string, AnimationCurve>(clip.characterGeos.Count);

            var auxiliary = new Dictionary<string, Tuple<string, string>>();

            for (var i = 0; i < clip.characterGeos.Count; i++)
            {
                for (var j = 0; j < clip.characterGeos[i].blendShapeNames.Count; j++)
                {
                    var blendShapeName = clip.characterGeos[i].blendShapeNames[j];
                    blendshapeCurves.Add(blendShapeName, new AnimationCurve());
                    auxiliary.Add(blendShapeName, new Tuple<string, string>("", ""));
                }
            }

            for (var i = 0; i < clip.facialAnimationFrames.Count; i++)
            {
                var time = i * clip.fixedDeltaTimeBetweenKeyFrames;
                for (var j = 0; j < clip.facialAnimationFrames[i].blendShapesUsed.Count; j++)
                {
                    var blendShapeData = clip.facialAnimationFrames[i].blendShapesUsed[j];

                    var characterGeoDescriptor = clip.characterGeos[blendShapeData.g];

                    var skinnedMeshRendererName = characterGeoDescriptor.skinnedMeshRendererName;

                    Transform tr;
                    var skinnedMeshRenderers = itFbxData.FbxGameObject.GetComponentsInChildren<SkinnedMeshRenderer>()
                        .Where(a => a.name == skinnedMeshRendererName).ToList();

                    if (skinnedMeshRenderers.Count == 0)
                    {
                        throw new Exception(
                            "Couldn't find the skinnedmeshrenderer that you recorded with. Please make sure that you're using the correct character model that you recorded the animation with");
                    }

                    tr = skinnedMeshRenderers[0].transform;

                    var skinnedMeshRenderer = tr.gameObject.GetComponent<SkinnedMeshRenderer>();
                    var blendshapeName = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(blendShapeData.i);
                    var blendshapeValue = blendShapeData.v;
                    var keyframe = new Keyframe(time, blendshapeValue);

                    var relativePath = AnimationUtility.CalculateTransformPath(tr, itFbxData.FbxGameObject.transform);

                    var blendshapeSplittedName = blendshapeName.Split('.')[1];

                    var curve = blendshapeCurves[blendshapeSplittedName];
                    curve.AddKey(keyframe);

                    var propertyName = string.Concat("blendShape.", blendshapeName);
                    auxiliary[blendshapeSplittedName] = new Tuple<string, string>(relativePath, propertyName);
                }
            }


            foreach (var pair in blendshapeCurves)
            {
                var relativePath = auxiliary[pair.Key].Item1;
                var propertyName = auxiliary[pair.Key].Item2;
                var curve = pair.Value;

                animationClip.SetCurve(relativePath, typeof(SkinnedMeshRenderer), propertyName,
                    curve);
            }

            itFbxData.FbxAnimator.avatar = null;

            var fullOsPathToSave = Path.Combine(IT_AnimotiveImporterEditorConstants.UnityFilesFacialAnimationDirectory,
                fileNameWithoutExtension);
            fullOsPathToSave = string.Concat(fullOsPathToSave, IT_AnimotiveImporterEditorConstants.AnimationExtension);

            var assetDbPathToSave =
                IT_AnimotiveImporterEditorUtilities.ConvertSystemPathToAssetDatabasePath(fullOsPathToSave);


            if (File.Exists(fullOsPathToSave))
            {
                var assetDbDir =
                    IT_AnimotiveImporterEditorUtilities.ConvertAssetDatabasePathToSystemPath(assetDbPathToSave);
                assetDbDir = Path.GetDirectoryName(assetDbDir);

                var similarFileName = await IT_AnimotiveImporterEditorUtilities.GetLatestSimilarFileName(assetDbDir,
                    fullOsPathToSave,
                    Path.GetFileName(fullOsPathToSave), IT_AnimotiveImporterEditorConstants.AnimationExtension);

                assetDbPathToSave =
                    IT_AnimotiveImporterEditorUtilities.ConvertSystemPathToAssetDatabasePath(similarFileName);
            }

            AssetDatabase.CreateAsset(animationClip, assetDbPathToSave);
            AssetDatabase.Refresh();
        }

        /// <summary>
        ///     Triggers all the necessary methods for the related animation clip creation PoC
        /// </summary>
        public static async Task HandleFacialAnimationOperations(List<IT_GroupData> groupDatas,
            Dictionary<string, IT_FbxDatasAndHoldersTuple> fbxDatasAndHoldersTuples, string clipsFolderPath)
        {
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
                        var jsonFullPath = cluster.FacialAnimationClipData.ClipDataPath;
                        var fullFileName = Path.GetFileName(jsonFullPath);

                        var contains = blendshapesDictionary.ContainsKey(jsonFullPath);

                        if (!contains) continue;

                        var fbxData = fbxDatasAndHoldersTuples[cluster.ModelName].FbxData;
                        var wrappedData = blendshapesDictionary[jsonFullPath];

                        CreateBlendShapeAnimationClip(wrappedData, fbxData,
                            Path.GetFileNameWithoutExtension(fullFileName));

                        var facialAnimationClipData = new IT_ClipData<FacialAnimationExportWrapper>(
                            IT_ClipType.FacialAnimationClip,
                            wrappedData, jsonFullPath,
                            IT_AnimotiveImporterEditorConstants.FacialAnimationFileExtension);
                        cluster.SetFacialAnimationData(facialAnimationClipData);
                    }
                }
            }

            AssetDatabase.Refresh();
        }

        private static async Task<Dictionary<string, FacialAnimationExportWrapper>> GetAllFacialAnimations(
            string clipsFolder)
        {
            var jsonFiles = Directory.GetFiles(clipsFolder).Where(a =>
                a.EndsWith(IT_AnimotiveImporterEditorConstants.FacialAnimationFileExtension) &&
                IT_AnimotiveImporterEditorUtilities.GetClipTypeFromClipName(
                    Path.GetFileNameWithoutExtension(a)
                ) ==
                IT_ClipType.FacialAnimationClip).ToList();

            var blendShapesFullPathAndWrappers = new Dictionary<string, FacialAnimationExportWrapper>();

            for (var i = 0; i < jsonFiles.Count; i++)
            {
                var jsonFileOsPath = jsonFiles[i];
                var wrapper = await GetBlendShapeAnimationDataFromFile(jsonFileOsPath);
                blendShapesFullPathAndWrappers.Add(jsonFileOsPath, wrapper);
            }

            return blendShapesFullPathAndWrappers;
        }
    }
}