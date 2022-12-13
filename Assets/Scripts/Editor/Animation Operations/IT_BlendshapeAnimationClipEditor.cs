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
        private static FacialAnimationExportWrapper GetBlendShapeAnimationDataFromFile(
            string jsonFileSystemFullPath)
        {
            FacialAnimationExportWrapper clip = null;
            var json = File.ReadAllText(jsonFileSystemFullPath);
            clip = JsonConvert.DeserializeObject<FacialAnimationExportWrapper>(json);

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
        private static void CreateBlendShapeAnimationClip(FacialAnimationExportWrapper clip,
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
                for (var j = 0; j < clip.facialAnimationFrames[i].bU.Count; j++)
                {
                    var blendShapeData = clip.facialAnimationFrames[i].bU[j];

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
                    var blendshapeName = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(blendShapeData.bI);
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
            // 
            var fileName = string.Concat(fileNameWithoutExtension,
                IT_AnimotiveImporterEditorConstants.AnimationExtension);

            var assetNameToSave = IT_AnimotiveImporterEditorUtilities.GetUniqueAssetDatabaseName(fileName);
            var assetDbPathToSave = Path.Combine(IT_AnimotiveImporterEditorConstants.UnityFilesFacialAnimationDirectory,
                assetNameToSave);

            AssetDatabase.CreateAsset(animationClip, assetDbPathToSave);
            AssetDatabase.Refresh();
        }

        /// <summary>
        ///     Triggers all the necessary methods for the related animation clip creation PoC
        /// </summary>
        public static void HandleFacialAnimationOperations(List<IT_GroupData> groupDatas,
            Dictionary<string, IT_FbxDatasAndHoldersTuple> fbxDatasAndHoldersTuples, string clipsFolderPath)
        {
            var blendshapesDictionary = GetAllFacialAnimations(clipsFolderPath);

            for (var i = 0; i < groupDatas.Count; i++)
            {
                var groupData = groupDatas[i];
                for (var j = 0; j < groupData.TakeDatas.Count; j++)
                {
                    var takeData = groupData.TakeDatas[j];
                    //j => take index

                    for (var k = 0; k < takeData.Clusters.Count; k++)
                    {
                        if (takeData.Clusters[k].ClusterType != IT_ClusterType.CharacterCluster)
                            continue; //if it's not character cluster then move on to the next

                        var cluster = (IT_CharacterCluster)takeData.Clusters[k];
                        var groupName = groupData.OriginalGroupName;
                        var takeIndex = takeData.TakeIndex;
                        var clipNumber = cluster.NumberOfCaptureInWhichItWasCaptured;

                        var fullFileName = string.Concat(cluster.EntityName, "_",
                            IT_AnimotiveImporterEditorConstants.FacialAnimationClipContentString, "_Clip_", clipNumber,
                            "_", groupName, "_Take_", takeIndex,
                            IT_AnimotiveImporterEditorConstants.FacialAnimationFileExtension);

                        var jsonFullName = Path.Combine(clipsFolderPath, fullFileName);

                        var contains = blendshapesDictionary.ContainsKey(jsonFullName);

                        if (!contains) continue;

                        var fbxData = fbxDatasAndHoldersTuples[cluster.EntityName].FbxData;
                        var wrappedData = blendshapesDictionary[jsonFullName];

                        var isEmptyFile = wrappedData.facialAnimationFrames.All(x => x.bU.Count == 0);

                        if (isEmptyFile) break;

                        CreateBlendShapeAnimationClip(wrappedData, fbxData,
                            Path.GetFileNameWithoutExtension(fullFileName));

                        var facialAnimationClipData = new IT_ClipData<FacialAnimationExportWrapper>(
                            IT_ClipType.FacialAnimationClip,
                            wrappedData, jsonFullName, j);
                        cluster.SetFacialAnimationData(facialAnimationClipData);
                    }
                }
            }

            AssetDatabase.Refresh();
        }

        private static Dictionary<string, FacialAnimationExportWrapper> GetAllFacialAnimations(
            string clipsFolder)
        {
            var jsonFiles = Directory.GetFiles(clipsFolder).Where(a =>
                a.EndsWith(IT_AnimotiveImporterEditorConstants.FacialAnimationFileExtension) &&
                IT_AnimotiveImporterEditorUtilities.GetClipTypeFromClipName(a) ==
                IT_ClipType.FacialAnimationClip).ToList();

            var blendShapesFullPathAndWrappers = new Dictionary<string, FacialAnimationExportWrapper>();

            for (var i = 0; i < jsonFiles.Count; i++)
            {
                var jsonFileOsPath = jsonFiles[i];
                var wrapper = GetBlendShapeAnimationDataFromFile(jsonFileOsPath);
                blendShapesFullPathAndWrappers.Add(jsonFileOsPath, wrapper);
            }

            return blendShapesFullPathAndWrappers;
        }
    }
}