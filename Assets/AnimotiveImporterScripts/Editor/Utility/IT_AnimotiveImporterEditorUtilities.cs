using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AnimotiveImporterDLL;
using OdinSerializer;
using UnityEditor;
using UnityEngine;
using SerializationUtility = OdinSerializer.SerializationUtility;

namespace Retinize.Editor.AnimotiveImporter
{
    /// <summary>
    ///     Utility class for general utility methods to use around the plugin
    /// </summary>
    public static class IT_AnimotiveImporterEditorUtilities
    {
        public static void RunFileCheck()
        {
            string[] files =
            {
                IT_AnimotiveImporterEditorConstants.UnityFilesBase,
                IT_AnimotiveImporterEditorConstants.UnityFilesAudioDirectory,
                IT_AnimotiveImporterEditorConstants.UnityFilesCharactersDirectory,
                IT_AnimotiveImporterEditorConstants.UnityFilesScenesDirectory,
                IT_AnimotiveImporterEditorConstants.UnityFilesPlayablesDirectory,
                IT_AnimotiveImporterEditorConstants.UnityFilesAnimationDirectory,
                IT_AnimotiveImporterEditorConstants.UnityFilesBodyAnimationDirectory,
                IT_AnimotiveImporterEditorConstants.UnityFilesFacialAnimationDirectory
            };

            for (int i = 0; i < files.Length; i++)
            {
                if (!Directory.Exists(files[i]))
                {
                    Directory.CreateDirectory(files[i]);
                }
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        ///     Checks if the folder that user trying to import is compatible with the plugin
        /// </summary>
        /// <param name="path">Path to folder that user chose</param>
        /// <returns>True: Yes folder is importable. False: No folder is not importable</returns>
        public static bool IsFolderInCorrectFormatToImport(string path)
        {
            var dirs = new HashSet<string>(Directory.GetDirectories(path));

            var result = dirs.Any(a => a.EndsWith("Clips")) && dirs.Any(a => a.EndsWith("SetAssets")) &&
                         dirs.Any(a => a.EndsWith("EntityAssets")) && dirs.Any(a => a.EndsWith("SceneDatas"));
            return result;
        }

        /// <summary>
        ///     Returns clip data (binary) path from "Clips" folder path using the clipname
        /// </summary>
        /// <param name="clipsPath">Clips folder path</param>
        /// <param name="clipName">Name of the clip to search in Clips folder</param>
        /// <returns>full path to binary clip data. If fails, returns empty</returns>
        public static string ReturnClipDataPathFromPath(string clipsPath, string clipName)
        {
            var files = Directory.GetFiles(clipsPath);

            for (var i = 0; i < files.Length; i++)
            {
                if (files[i].Contains(clipName)) return files[i];
            }


            return "";
        }

        /// <summary>
        ///     Determines the clip type by using the clip name (Every clip has it's type written in the file name)
        /// </summary>
        /// <param name="clipName">Clip name to determine type of</param>
        /// <returns>Type as IT_ClipType</returns>
        public static IT_ClipType GetClipTypeFromClipName(string clipName)
        {
            string clipNameWithoutSuffix =
                clipName.EndsWith("_Local") ? clipName.Split("_Local")[0] : clipName.Split("_Remote")[0];

            if (clipNameWithoutSuffix.EndsWith("0"))
            {
                return IT_ClipType.TransformAnimationClip;
            }

            if (clipNameWithoutSuffix.EndsWith("1"))
            {
                return IT_ClipType.PropertiesClip;
            }

            if (clipNameWithoutSuffix.EndsWith("2"))
            {
                return IT_ClipType.AudioClip;
            }

            if (clipNameWithoutSuffix.EndsWith("3"))
            {
                return IT_ClipType.FacialAnimationClip;
            }

            return IT_ClipType.None;
        }

        /// <summary>
        ///     Converts full path of anything from UnityExports into usable asset database path under the UnityFiles
        /// </summary>
        /// <param name="unityFilesDirectory">
        ///     Directory that user wants to put the new file in (Look
        ///     IT_AnimotiveImporterEditorConstants class for more detail )
        /// </param>
        /// <param name="dataPath">Full path of data under the UnityExported directory</param>
        /// <param name="extension">extension of file</param>
        /// <returns> asset database path to create assets at</returns>
        public static string ConvertFullFilePathIntoUnityFilesPath(string unityFilesDirectory, string dataPath,
            string extension)
        {
            var baseBodyPathWithNameWithoutExtension = Path.Combine(
                ConvertSystemPathToAssetDatabasePath(unityFilesDirectory),
                Path.GetFileNameWithoutExtension(dataPath));

            var path = string.Concat(baseBodyPathWithNameWithoutExtension, $"{extension}");
            return path;
        }

        /// <summary>
        ///     Converts system path of unity assets to asset database path
        /// </summary>
        /// <param name="fullOsPath">Full path of unity asset</param>
        /// <returns>asset database path</returns>
        public static string ConvertSystemPathToAssetDatabasePath(string fullOsPath)
        {
            var result = fullOsPath.Split(new[] { "Assets" }, StringSplitOptions.None)[1];
            result = string.Concat("Assets", result);
            return result;
        }

        /// <summary>
        ///     Converts asset database path to full os path (Item should exist in the Unity asset database)
        /// </summary>
        /// <param name="assetDbPath">asset database path of the asset</param>
        /// <returns>Full OS path of unity asset</returns>
        public static string ConvertAssetDatabasePathToSystemPath(string assetDbPath)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), assetDbPath);
        }

        /// <summary>
        ///     Collects and returns all usable group,cluster and take datas
        /// </summary>
        /// <param name="sceneData">Binary scene data</param>
        /// <param name="clipsPath">Path to Clips folder</param>
        /// <returns>List of IT_GroupData</returns>
        public static async Task<List<IT_GroupData>> GetGroupDataListByType(IT_SceneInternalData sceneData,
            string clipsPath)
        {
            var groupDatas = new List<IT_GroupData>();

            IT_ClipCluster currentCluster;

            foreach (var groupData in sceneData.groupDataBySerializedId.Values)
            {
                var readerGroupData = new IT_GroupData(groupData.serializedId, groupData.groupName);

                foreach (var entityId in groupData.entitiesIds)
                {
                    var entityData = sceneData.entitiesDataBySerializedId[entityId];
                    for (var i = 0; i < entityData.clipsByTrackByTakeIndex.Count; i++)
                    {
                        var take = entityData.clipsByTrackByTakeIndex[i];

                        if (!readerGroupData.TakeDatas.ContainsKey(i))
                            readerGroupData.TakeDatas.Add(i, new IT_TakeData(i));

                        currentCluster = new IT_ClipCluster();

                        for (var j = 0; j < take.Count; j++)
                        {
                            var track = take[j];
                            for (var k = 0; k < track.Count; k++)
                            {
                                var clipPlayerData = track[k];

                                string facialAnimationFileName = clipPlayerData.clipName;
                                facialAnimationFileName =
                                    facialAnimationFileName.Remove(facialAnimationFileName.Length - 1, 1) + 3;

                                var animationClipDataPath =
                                    ReturnClipDataPathFromPath(clipsPath,
                                        clipPlayerData.clipName);

                                var facialAnimationFilePath =
                                    ReturnClipDataPathFromPath(clipsPath, facialAnimationFileName);

                                if (File.Exists(facialAnimationFilePath))
                                {
                                    var facialAnimationClipData = new IT_ClipData<FacialAnimationExportWrapper>(
                                        IT_ClipType.FacialAnimationClip, null, facialAnimationFilePath,
                                        ".json");
                                    currentCluster.SetFacialAnimationData(facialAnimationClipData);
                                }


                                if (string.IsNullOrEmpty(animationClipDataPath))
                                {
                                    continue;
                                }


                                var type =
                                    GetClipTypeFromClipName(clipPlayerData.clipName);

                                var clipdata =
                                    new IT_ClipData<IT_ClipPlayerData>(type, clipPlayerData, animationClipDataPath,
                                        null);

                                switch (type)
                                {
                                    case IT_ClipType.None:
                                        throw new ArgumentOutOfRangeException();
                                    case IT_ClipType.PropertiesClip:
                                        currentCluster.SetPropertiesClipData(clipdata);
                                        break;
                                    case IT_ClipType.TransformAnimationClip:
                                        currentCluster.SetTransformClipData(clipdata);
                                        break;
                                    case IT_ClipType.AudioClip:


                                        var fileName = await FindLatestFileName(clipPlayerData.clipName,
                                            IT_AnimotiveImporterEditorConstants.UnityFilesAudioDirectory,
                                            IT_AnimotiveImporterEditorConstants.AudioExtensions[0]);

                                        if (string.IsNullOrEmpty(fileName))
                                        {
                                            fileName = await FindLatestFileName(clipPlayerData.clipName,
                                                IT_AnimotiveImporterEditorConstants.UnityFilesAudioDirectory,
                                                IT_AnimotiveImporterEditorConstants.AudioExtensions[1]);
                                        }


                                        if (!string.IsNullOrEmpty(fileName))
                                        {
                                            string fileExtension = Path.GetExtension(fileName);

                                            var currentClipDataPath = fileName;
                                            currentClipDataPath = currentClipDataPath.Split(
                                                new[] { fileExtension },
                                                StringSplitOptions.None)[0];
                                            clipdata = new IT_ClipData<IT_ClipPlayerData>(type,
                                                clipdata.ClipPlayerData,
                                                currentClipDataPath, fileExtension);
                                        }

                                        currentCluster.SetAudioClipData(clipdata);

                                        break;

                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }

                                currentCluster.ModelName =
                                    entityData.propertiesDataByTakeIndex[0]["displayName"] as string;
                            }

                            if (currentCluster.AudioClipData.ClipPlayerData != null)
                                readerGroupData.TakeDatas[i].Clusters.Add(currentCluster);
                        }
                    }
                }

                readerGroupData.TakeDatas = readerGroupData.TakeDatas.Where(a => a.Value.Clusters.Count != 0)
                    .ToDictionary(p => p.Key, p => p.Value);

                groupDatas.Add(readerGroupData);
            }

            return groupDatas;
        }

        public static async Task<string> FindLatestFileName(string clipName, string directory, string extension)
        {
            var files = Directory
                .GetFiles(directory);

            var nameWithoutExtension = Path
                .GetFileNameWithoutExtension(clipName)
                .ToLower();

            var clipFiles =
                files.Where(a => !a.EndsWith(".meta") &&
                                 a.EndsWith(extension))
                    .ToArray();

            clipFiles = clipFiles.Where(a =>
                    a.ToLower().Contains(nameWithoutExtension.ToLower()))
                .ToArray();

            clipFiles = clipFiles.OrderByDescending(a =>
                Path.GetFileNameWithoutExtension(a).Split(' ')[
                    Path.GetFileNameWithoutExtension(a).Split(' ').Length - 1]).ToArray();

            if (clipFiles.Length > 1) return clipFiles[1];
            return string.Empty;
        }


        /// <summary>
        ///     Called when you already have a file saved in the asset database with the same name that you're trying to add.
        ///     This function checks all the names and add distinctive number to end of name so that you can accumulate files.
        /// </summary>
        /// <param name="assetDatabaseDir">Asset database directory path to search similar files in</param>
        /// <param name="fullSourceFilePath">Full OS path of file </param>
        /// <param name="fileName">file name to search similarity</param>
        /// <param name="extension">file extension</param>
        /// <returns>
        ///     Returns the new name of your file. Example: Let's say your input was File, and there was File 0005 in the related
        ///     directory.
        ///     This function will return File 0006 so you can save your file with a unique name
        /// </returns>
        public static async Task<string> GetLatestSimilarFileName(string assetDatabaseDir, string fullSourceFilePath,
            string fileName,
            string extension)
        {
            var targetFileName = Path.Combine(assetDatabaseDir, fileName);

            var alreadySavedSimilarFiles = Directory.GetFiles(assetDatabaseDir)
                .Where(a => a.ToLower().Contains(Path.GetFileNameWithoutExtension(fullSourceFilePath).ToLower()) &
                            !a.EndsWith(".meta")).OrderByDescending(a =>
                    Path.GetFileNameWithoutExtension(a).Split(' ')[
                        Path.GetFileNameWithoutExtension(a).Split(' ').Length - 1])
                .ToList();

            if (alreadySavedSimilarFiles.Count > 1)
            {
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(alreadySavedSimilarFiles[1]);
                var split = nameWithoutExtension.Split(' ');
                var number = int.Parse(split[split.Length - 1]);
                number += 1;
                var numberAsString = 4 - number.ToString().Length;
                var temp = string.Concat(Enumerable.Repeat("0", numberAsString));
                temp = string.Concat(temp, number);

                targetFileName = Path.Combine(Path.GetDirectoryName(targetFileName), string.Concat(
                    Path.GetFileNameWithoutExtension(targetFileName), " ",
                    temp, $"{extension}"));
            }
            else
            {
                targetFileName = Path.Combine(Path.GetDirectoryName(targetFileName), string.Concat(
                    Path.GetFileNameWithoutExtension(targetFileName), " ", "0001",
                    $"{extension}"));
            }

            return targetFileName;
        }

        /// <summary>
        ///     Gets and returns some properties of entities  from the scene data to apply later on
        /// </summary>
        /// <param name="sceneData">Binary scene data</param>
        /// <returns></returns>
        public static async Task<Dictionary<IT_EntityType, List<IT_BaseEntity>>> GetPropertiesData(
            IT_SceneInternalData sceneData)
        {
            var entitiesWithType =
                new Dictionary<IT_EntityType, List<IT_BaseEntity>>();


            foreach (var groupData in sceneData.groupDataBySerializedId.Values)
            {
                foreach (var entityId in groupData.entitiesIds)
                {
                    var entityData = sceneData.entitiesDataBySerializedId[entityId];

                    foreach (var propertyDatasDict in entityData.propertiesDataByTakeIndex)
                    {
                        //it's not null always. Sometimes the data comes null. Keep this condition here.
                        if (propertyDatasDict == null)
                        {
                            continue;
                        }

                        var displayName = propertyDatasDict["displayName"].ToString();

                        var list = IT_AnimotiveImporterEditorConstants.EntityTypesByKeyword.Where(pair =>
                            displayName.Contains(pair.Value)).ToList();

                        if (list.Count > 0)
                        {
                            var entityType = list[0].Key;

                            if (!entitiesWithType.ContainsKey(entityType))
                                entitiesWithType.Add(entityType, new List<IT_BaseEntity>());

                            var holderPosition =
                                (Vector3)propertyDatasDict[IT_AnimotiveImporterEditorConstants.HolderPositionString];

                            var holderRotation =
                                (Quaternion)propertyDatasDict[
                                    IT_AnimotiveImporterEditorConstants.HolderRotationString];

                            var rootPosition =
                                (Vector3)propertyDatasDict[IT_AnimotiveImporterEditorConstants.RootPositionString];

                            var rootRotation =
                                (Quaternion)propertyDatasDict[IT_AnimotiveImporterEditorConstants.RootRotationString];

                            IT_BaseEntity itEntity;

                            switch (entityType)
                            {
                                case IT_EntityType.Camera:
                                {
                                    var focalLength = (float)propertyDatasDict["DepthOfFieldFocalLength"];
                                    itEntity = new IT_CameraEntity(holderPosition, rootPosition,
                                        holderRotation, rootRotation, displayName, focalLength);

                                    break;
                                }
                                default:
                                {
                                    itEntity = new IT_BaseEntity(holderPosition,
                                        rootPosition, holderRotation, rootRotation, displayName);
                                    break;
                                }
                            }

                            entitiesWithType[entityType].Add(itEntity);
                        }
                    }
                }
            }

            return entitiesWithType;
        }

        public static float GetFieldOfView(Camera camera, float newFocalLength)
        {
            return 2 * Mathf.Atan(camera.sensorSize.x / (2 * newFocalLength)) * (180 / Mathf.PI);
        }

        public static bool IsCharactersFolderEmpty()
        {
            var files = Directory.GetFiles(IT_AnimotiveImporterEditorConstants.UnityFilesCharactersDirectory)
                .Where(a => a.EndsWith(IT_AnimotiveImporterEditorConstants.ModelExtension)).ToArray();

            return files.Length == 0;
        }
    }
}