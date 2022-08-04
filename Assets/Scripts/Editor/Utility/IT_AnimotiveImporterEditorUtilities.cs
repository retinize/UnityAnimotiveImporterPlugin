using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnimotiveImporterDLL;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Retinize.Editor.AnimotiveImporter
{
#if UNITY_EDITOR
    public static class IT_AnimotiveImporterEditorUtilities
    {
        /// <summary>
        ///     Deletes the asset if it already exists in the AssetDatabase
        /// </summary>
        /// <param name="path">Path of the asset in the asset database.</param>
        /// <param name="type">Type of the asset to look for and delete.</param>
        public static void DeleteAssetIfExists(string path, Type type)
        {
            if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path)))
            {
                if (AssetDatabase.LoadAssetAtPath(path, type) != null)
                    AssetDatabase.DeleteAsset(path);
            }
        }

        /// <summary>
        ///     Loads FBX from it's designated path and instantiates it to the current scene in the editor
        /// </summary>
        /// <returns>Tuple that contains instantiated character's root gameObject and it's Animator</returns>
        public static IT_FbxData LoadFbx(string fbxAssetDatabasePath)
        {
            var characterRoot = AssetDatabase.LoadAssetAtPath(fbxAssetDatabasePath,
                typeof(GameObject)) as GameObject;

            characterRoot = Object.Instantiate(characterRoot);
            characterRoot.AddComponent<AudioSource>();
            var animator = characterRoot.GetComponent<Animator>();

            return new IT_FbxData(characterRoot, animator);
        }

        public static bool IsFolderInCorrectFormatToImport(string path)
        {
            var dirs = Directory.GetDirectories(path);

            var result = dirs.Any(a => a.EndsWith("Clips")) && dirs.Any(a => a.EndsWith("SetAssets")) &&
                         dirs.Any(a => a.EndsWith("EntityAssets"));
            return result;
        }

        public static string ReturnClipDataFromPath(string clipsPath, string clipName)
        {
            var files = Directory.GetFiles(clipsPath);

            for (var i = 0; i < files.Length; i++)
            {
                if (files[i].Contains(clipName)) return files[i];
            }

            return "";
        }

        public static IT_ClipType GetClipTypeFromClipName(string clipName)
        {
            foreach (var pair in IT_AnimotiveImporterEditorConstants.ClipNamesByType)
            {
                if (clipName.Contains(pair.Value)) return pair.Key;
            }

            return IT_ClipType.None;
        }

        public static T AddOrGetComponent<T>(this GameObject obj) where T : Component
        {
            var get = obj.GetComponent<T>();
            if (get == null) return obj.AddComponent<T>();

            return get;
        }

        public static string GetBodyAssetDatabasePath(string dataPath, string extension)
        {
            var baseBodyPathWithNameWithoutExtension = string.Concat(
                IT_AnimotiveImporterEditorConstants.BodyAnimationDirectory,
                Path.GetFileNameWithoutExtension(dataPath));

            var bodyAnimationPath = string.Concat(baseBodyPathWithNameWithoutExtension, $".{extension}");
            return bodyAnimationPath;
        }

        public static string ConvertSystemPathToAssetDatabasePath(string fullOsPath)
        {
            var result = fullOsPath.Split(new[] {"Assets"}, StringSplitOptions.None)[1];
            result = string.Concat("Assets", result);
            return result;
        }


        public static string ConvertAssetDatabasePathToSystemPath(string assetDbPath)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), assetDbPath);
        }

        public static List<IT_GroupData> GetClipsPathByType(IT_SceneInternalData sceneData,
            string clipsPath)
        {
            var groupDatas = new List<IT_GroupData>();

            IT_ClipCluster currentCluster = null;

            foreach (var groupData in sceneData.groupDataById.Values)
            {
                var readerGroupData =
                    new IT_GroupData(groupData.serializedId, groupData.groupName.Trim().Replace(" ", ""));
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
                                var clip = track[k];

                                var animationClipDataPath =
                                    ReturnClipDataFromPath(clipsPath,
                                        clip.clipName);

                                var type =
                                    GetClipTypeFromClipName(clip.clipName);

                                var clipdata = new IT_ClipData(type, clip, animationClipDataPath);

                                switch (type)
                                {
                                    case IT_ClipType.None:
                                        throw new ArgumentOutOfRangeException();
                                    case IT_ClipType.PropertiesClip:
                                        currentCluster.SetPropertiesClip(clipdata);
                                        break;
                                    case IT_ClipType.TransformClip:
                                        currentCluster.SetTransformClip(clipdata);
                                        break;
                                    case IT_ClipType.AudioClip:

                                        var files = Directory
                                            .GetFiles(IT_AnimotiveImporterEditorWindow
                                                .ImportedAudiosAssetdatabaseDirectory);

                                        var nameWithoutExtension = Path
                                            .GetFileNameWithoutExtension(clip.clipName)
                                            .ToLower();

                                        var audioFiles =
                                            files.Where(a => !a.EndsWith(".meta") &&
                                                             a.EndsWith(".wav"))
                                                .ToArray();

                                        audioFiles = audioFiles.Where(a =>
                                                a.ToLower().Contains(nameWithoutExtension.ToLower()))
                                            .ToArray();

                                        audioFiles = audioFiles.OrderByDescending(a =>
                                            Path.GetFileNameWithoutExtension(a).Split(' ')[
                                                Path.GetFileNameWithoutExtension(a).Split(' ').Length - 1]).ToArray();

                                        if (audioFiles.Length > 1)
                                        {
                                            var currentClipDataPath = audioFiles[1];
                                            currentClipDataPath = currentClipDataPath.Split(
                                                new[] {".wav"},
                                                StringSplitOptions.None)[0];

                                            currentCluster.SetAudioClip(new IT_ClipData(type, clipdata.ClipPlayerData,
                                                currentClipDataPath));
                                        }
                                        else
                                            currentCluster.SetAudioClip(clipdata);


                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }

                                currentCluster.TakeIndex = i;
                                currentCluster.ModelName = entityData.entityInstantiationTokenData;
                            }

                            if (currentCluster.AudioClip.ClipPlayerData != null)
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


        public static string GetLatestSimilarFileName(string assetDatabaseDir, string fullSourceFilePath,
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
                    temp, $".{extension}"));
            }
            else
            {
                targetFileName = Path.Combine(Path.GetDirectoryName(targetFileName), string.Concat(
                    Path.GetFileNameWithoutExtension(targetFileName), " ", "0001",
                    $".{extension}"));
            }

            return targetFileName;
        }

        public static Dictionary<IT_EntityType, List<IT_BaseEntity>> GetPropertiesData(IT_SceneInternalData sceneData)
        {
            var entitiesWithType =
                new Dictionary<IT_EntityType, List<IT_BaseEntity>>();


            foreach (var groupData in sceneData.groupDataById.Values)
            {
                foreach (var entityId in groupData.entitiesIds)
                {
                    var entityData = sceneData.entitiesDataBySerializedId[entityId];

                    foreach (var propertyDatasDict in entityData.propertiesDataByTakeIndex)
                    {
                        var displayName = propertyDatasDict["displayName"].ToString();

                        var list = IT_AnimotiveImporterEditorConstants.EntityTypesByKeyword.Where(pair =>
                            displayName.Contains(pair.Value)).ToList();

                        if (list.Count > 0)
                        {
                            var entityType = list[0].Key;

                            if (!entitiesWithType.ContainsKey(entityType))
                                entitiesWithType.Add(entityType, new List<IT_BaseEntity>());

                            var holderPosition =
                                (Vector3) propertyDatasDict[IT_AnimotiveImporterEditorConstants.HolderPositionString];

                            var holderRotation =
                                (Quaternion) propertyDatasDict[
                                    IT_AnimotiveImporterEditorConstants.HolderRotationString];

                            var rootPosition =
                                (Vector3) propertyDatasDict[IT_AnimotiveImporterEditorConstants.RootPositionString];

                            var rootRotation =
                                (Quaternion) propertyDatasDict[IT_AnimotiveImporterEditorConstants.RootRotationString];


                            IT_BaseEntity itEntity;

                            switch (entityType)
                            {
                                case IT_EntityType.Camera:
                                {
                                    var focalLength = (float) propertyDatasDict["DepthOfFieldFocalLength"];
                                    itEntity = new IT_CameraEntity(IT_EntityType.Camera, holderPosition, rootPosition,
                                        holderRotation, rootRotation, displayName, focalLength);

                                    break;
                                }
                                case IT_EntityType.Spotlight:
                                {
                                    itEntity = new IT_SpotLightEntity(IT_EntityType.Spotlight, holderPosition,
                                        rootPosition, holderRotation, rootRotation, displayName);
                                    break;
                                }
                                default:
                                {
                                    throw new ArgumentOutOfRangeException();
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
    }


#endif
}