using System.IO;
using AnimotiveImporterDLL;
using OdinSerializer;

namespace Retinize.Editor.AnimotiveImporter
{
    public static class IT_SceneDataOperations
    {
        /// <summary>
        ///     Loads, de-serializes and returns binary scene data exported from Animotive
        /// </summary>
        /// <param name="unityFilesDirectory">Unity files directory path</param>
        /// <returns></returns>
        public static IT_SceneInternalData LoadSceneData(string unityFilesDirectory)
        {
            var sceneDataFilePath = string.Concat(unityFilesDirectory, "/SceneData_UnityExport");
            var bytes = File.ReadAllBytes(sceneDataFilePath);
            var loadSceneData = SerializationUtility.DeserializeValue<IT_SceneInternalData>(bytes, DataFormat.Binary);
            return loadSceneData;
        }
    }
}