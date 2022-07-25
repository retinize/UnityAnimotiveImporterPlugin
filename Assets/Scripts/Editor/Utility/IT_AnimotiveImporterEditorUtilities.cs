using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnimotiveImporterDLL;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
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

        public static string GetImportedFbxAssetDatabasePathVariable(string fullPathToSaveFbx)
        {
            fullPathToSaveFbx =
                fullPathToSaveFbx.Split(new[] { "Assets" }, StringSplitOptions.None)[1];

            fullPathToSaveFbx = string.Concat("Assets", fullPathToSaveFbx);
            return fullPathToSaveFbx;
        }

        public static string GetBodyAssetDatabasePath(string dataPath,string extension )
        {
            var baseBodyPathWithNameWithoutExtension = string.Concat(
                IT_AnimotiveImporterEditorConstants.BodyAnimationDirectory,
                Path.GetFileNameWithoutExtension(dataPath));

            var bodyAnimationPath = string.Concat(baseBodyPathWithNameWithoutExtension, extension);
            return bodyAnimationPath;
        }


        public static List<IT_GroupData> GetClipsPathByType(IT_SceneInternalData sceneData,
            string clipsPath)
        {
            var groupDatas = new List<IT_GroupData>();

            var clipClusters = new List<IT_ClipCluster>();
            IT_ClipCluster currentCluster = null;

            foreach (var groupData in sceneData.groupDataById.Values)
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
                            if (track.Count != 0) clipClusters.Add(currentCluster);

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

        public static AudioClip GetAudioClip(string fullPath)
        {
            using (var www = UnityWebRequestMultimedia.GetAudioClip(fullPath, AudioType.MPEG))
            {
                www.SendWebRequest();

                
                if (www.result == UnityWebRequest.Result.ConnectionError ||
                    www.result == UnityWebRequest.Result.ProtocolError)
                    Debug.LogError(www.error);
                else
                {
                    var myClip = DownloadHandlerAudioClip.GetContent(www);
                    // audioSource.clip = myClip;
                    // audioSource.Play();

                    return myClip;
                }
            }

            return null;
        }
        
        // Assigns the loaded audio clip to the source or does nothing if the argument filename or path is inexistend. Call this method inside the StartCoroutine of C#
        public static IEnumerator<WWW> LoadAudioClip(string fullPathToFile, AudioSource audioSource)
        {
            if(!String.IsNullOrEmpty(fullPathToFile) && audioSource != null){

                if(File.Exists(fullPathToFile))
                {
                    WWW www = new WWW("file:" + fullPathToFile);
                    yield return www;
                    audioSource.clip = www.GetAudioClip(false, false, AudioType.WAV);
                    audioSource.clip.name = fullPathToFile;
                }
            }
        }
        
        // Returns the used path with filename and its ending
      
        
    }


#endif
}