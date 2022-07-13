using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnimotiveImporterDLL;
using OdinSerializer;

public static class IT_SceneDataOperations
{
    public static IT_SceneInternalData LoadSceneData(string importedFilesDirectory)
    {
        var files = Directory.GetFiles(importedFilesDirectory);
        var sceneDataFilePath =
            files.Where(a => Path.GetFileNameWithoutExtension(a).StartsWith("SceneData")).ToList();

        var sceneInternalDatas = new List<IT_SceneInternalData>(sceneDataFilePath.Count);

        for (var i = 0; i < sceneDataFilePath.Count; i++)
        {
            var value = SerializationUtility.DeserializeValue<IT_SceneInternalData>(
                File.ReadAllBytes(sceneDataFilePath[i]),
                DataFormat.Binary);

            sceneInternalDatas.Add(value);
        }

        return sceneInternalDatas[sceneInternalDatas.Count - 1];
    }
}