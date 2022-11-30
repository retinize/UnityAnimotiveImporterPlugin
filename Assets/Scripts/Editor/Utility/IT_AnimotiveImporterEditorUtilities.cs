using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AnimotiveImporterDLL;
using UnityEditor;
using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    /// <summary>
    ///     Utility class for general utility methods to use around the plugin
    /// </summary>
    public static class IT_AnimotiveImporterEditorUtilities
    {
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
            foreach (var pair in IT_AnimotiveImporterEditorConstants.ClipNamesByType)
            {
                if (clipName.Contains(pair.Value)) return pair.Key;
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
        public static List<IT_GroupData> GetGroupDataListByType(IT_SceneInternalData sceneData,
            string clipsPath)
        {
            var groupsHead = new GameObject("Groups");
            var groupDatas = new List<IT_GroupData>();

            IIT_ICluster currentCluster;

            foreach (var groupData in sceneData.groupDataBySerializedId.Values)
            {
                var readerGroupData =
                    new IT_GroupData(groupData.serializedId, groupData.groupName, groupsHead);

                foreach (var entityId in groupData.entitiesIds)
                {
                    var entityData = sceneData.entitiesDataBySerializedId[entityId];


                    for (var i = 0; i < entityData.clipsByTrackByTakeIndex.Count; i++)
                    {
                        var take = entityData.clipsByTrackByTakeIndex[i];
                        if (take.Count == 0) continue;

                        //i => take index

                        var displayName =
                            (string)entityData.propertiesDataByTakeIndex[i][
                                IT_AnimotiveImporterEditorConstants.DisplayName];

                        if (!readerGroupData.TakeDatas.ContainsKey(i))
                            readerGroupData.TakeDatas.Add(i, new IT_TakeData(i));

                        if (displayName.ToLower().Contains("camera"))
                            currentCluster = new IT_CameraCluster();
                        else
                        {
                            currentCluster = new IT_CharacterCluster();
                            if (take.Count < 3)
                                continue; //if take doesn't have audio,properties and transform datas all at once then it's not useful for characters
                        }

                        currentCluster.TakeIndex = i;
                        currentCluster.EntityName = displayName;


                        var tempItemList = new List<Dictionary<IT_ClipType, IT_ClipData<IT_ClipPlayerData>>>();
                        for (int j = 0; j < take.Count; j++)
                        {
                            var track = take[j];

                            if (track.Count == 0)
                            {
                                continue;
                            }

                            for (int k = 0; k < track.Count; k++)
                            {
                                tempItemList.Add(new Dictionary<IT_ClipType, IT_ClipData<IT_ClipPlayerData>>());
                            }
                        }


                        for (var j = 0; j < take.Count; j++)
                        {
                            var track = take[j];

                            if (track.Count == 0) continue;

                            for (var k = 0; k < track.Count; k++)
                            {
                                var clip = track[k];

                                if (clip == null)
                                {
                                    continue;
                                }


                                var animationClipDataPath =
                                    ReturnClipDataPathFromPath(clipsPath,
                                        clip.clipName);

                                var type =
                                    GetClipTypeFromClipName(clip.clipName);

                                var clipdata = new IT_ClipData<IT_ClipPlayerData>(type, clip, animationClipDataPath, i);

                                // if (type == IT_ClipType.AudioClip)
                                // {
                                //     var fileName = GetLastFileName(clip.clipName,
                                //         IT_AnimotiveImporterEditorConstants.UnityFilesAudioDirectory,
                                //         IT_AnimotiveImporterEditorConstants.AudioExtension);
                                //
                                //     if (!string.IsNullOrEmpty(fileName))
                                //     {
                                //         var currentClipDataPath = fileName;
                                //         currentClipDataPath = currentClipDataPath.Split(
                                //             new[] { IT_AnimotiveImporterEditorConstants.AudioExtension },
                                //             StringSplitOptions.None)[0];
                                //         clipdata = new IT_ClipData<IT_ClipPlayerData>(type,
                                //             clipdata.ClipPlayerData,
                                //             currentClipDataPath, i);
                                //     }
                                // }

                                tempItemList[k].Add(type, clipdata);
                            }
                        }

                        tempItemList = tempItemList.Where(x => x.Count != 0).ToList();
                        currentCluster.ClipDatas = tempItemList;

                        if (!readerGroupData.TakeDatas[i].Clusters.Contains(currentCluster))
                            readerGroupData.TakeDatas[i].Clusters.Add(currentCluster);
                    }
                }

                readerGroupData.TakeDatas = readerGroupData.TakeDatas
                    .Where(a => a.Value.Clusters.Count != 0)
                    .ToDictionary(p => p.Key, p => p.Value);


                groupDatas.Add(readerGroupData);
            }

            return groupDatas;
        }

        public static string GetLastFileName(string clipName, string assetDatabaseDirectory, string extension)
        {
            var lowerCaseNameWithoutExtension = Path
                .GetFileNameWithoutExtension(clipName)
                .ToLower();
            string osDirectory = Path.Combine(Directory.GetCurrentDirectory(), assetDatabaseDirectory);

            var files = Directory
                .GetFiles(osDirectory)
                .Where(a => !a.EndsWith(".meta") &&
                            a.EndsWith(extension) &&
                            a.ToLower().Contains(lowerCaseNameWithoutExtension))
                .ToArray();

            if (files.Length > 1)
                return ConvertSystemPathToAssetDatabasePath(files[files.Length - 2]);
            return string.Empty;
        }


        /// <summary>
        ///     Called when you already have a file saved in the asset database with the same name that you're trying to add.
        ///     This function checks all the names and add distinctive number to end of name so that you can accumulate files.
        /// </summary>
        /// <param name="pathToFile">file name to search similarity</param>
        /// <returns>
        ///     Returns the new name of your file. Example: Let's say your input was File, and there was File 0005 in the related
        ///     directory.
        ///     This function will return File 0006 so you can save your file with a unique name
        /// </returns>
        public static string GetUniqueAssetDatabaseName(string pathToFile)
        {
            var extension = Path.GetExtension(pathToFile);
            var fileName = Path.GetFileNameWithoutExtension(pathToFile);
            var targetFileName = Path.GetFileName(pathToFile);

            for (var i = 1;; ++i)
            {
                if (!DoesAssetExist(targetFileName)) return targetFileName;

                var indexString = i.ToString();
                int zeroCount = 4 - indexString.Length;

                string zeros = new string('0', zeroCount);

                targetFileName = string.Concat(fileName, " ", zeros, i, extension);
            }
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

        /// <summary>
        ///     Moves audio files into Unity editor and sorts them.
        /// </summary>
        /// <param name="unityExportPath">Path to user browsed and selected folder usually called "UnityExported" </param>
        /// <returns></returns>
        internal static async Task MoveAudiosIntoUnity(string unityExportPath)
        {
            var clipsPath = Path.Combine(unityExportPath, "Clips");

            var files = Directory.GetFiles(clipsPath)
                .Where(a => a.ToLower().EndsWith(IT_AnimotiveImporterEditorConstants.AudioExtension)).ToList();


            for (var i = 0; i < files.Count; i++)
            {
                var fileName = Path.GetFileName(files[i]);
                var uniqueFileName = GetUniqueAssetDatabaseName(fileName);


                var targetFileName =
                    Path.Combine(Directory.GetCurrentDirectory(),
                        IT_AnimotiveImporterEditorConstants.UnityFilesAudioDirectory, uniqueFileName);

                File.Copy(files[i], targetFileName, false);
            }
        }

        /// <summary>
        ///     Deletes all accumulated files such as; Scenes, audios, animations and playables. But doesn't delete characters
        /// </summary>
        internal static void ClearAccumulatedFiles()
        {
            string baseDir = Directory.GetCurrentDirectory();
            string[] directories =
            {
                Path.Combine(baseDir, IT_AnimotiveImporterEditorConstants.UnityFilesAnimationDirectory),
                Path.Combine(baseDir, IT_AnimotiveImporterEditorConstants.UnityFilesPlayablesDirectory),
                Path.Combine(baseDir, IT_AnimotiveImporterEditorConstants.UnityFilesScenesDirectory),
                Path.Combine(baseDir, IT_AnimotiveImporterEditorConstants.UnityFilesAudioDirectory),
            };

            for (var i = 0; i < directories.Length; i++)
            {
                var dir = directories[i];
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }


            IT_AnimotiveImporterEditorWindow.ResetWindow();
            AssetDatabase.Refresh();
        }

        public static void CreateAssetsFolders()
        {
            string baseDir = Directory.GetCurrentDirectory();

            string[] animationDirectories =
            {
                Path.Combine(baseDir, IT_AnimotiveImporterEditorConstants.UnityFilesAnimationDirectory),
                Path.Combine(baseDir, IT_AnimotiveImporterEditorConstants.UnityFilesBodyAnimationDirectory),
                Path.Combine(baseDir, IT_AnimotiveImporterEditorConstants.UnityFilesFacialAnimationDirectory),
                Path.Combine(baseDir, IT_AnimotiveImporterEditorConstants.UnityFilesCameraAnimationDirectory),
                Path.Combine(baseDir, IT_AnimotiveImporterEditorConstants.UnityFilesAudioDirectory),
            };

            for (var i = 0; i < animationDirectories.Length; i++)
            {
                if (!Directory.Exists(animationDirectories[i])) Directory.CreateDirectory(animationDirectories[i]);
            }
        }

        public static bool DoesAssetExist(string assetNameWithExtension)
        {
            var extension = Path.GetExtension(assetNameWithExtension);
            var assetPaths = AssetDatabase.GetAllAssetPaths()
                .Where(x => x.StartsWith("Assets") && x.EndsWith(extension)).ToArray();

            return assetPaths.Any(a => a.EndsWith(assetNameWithExtension));
        }
    }
}