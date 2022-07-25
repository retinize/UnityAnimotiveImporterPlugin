using System.IO;
using AnimotiveImporterDLL;
using OdinSerializer;

public static class IT_SceneDataOperations
{
    public static IT_SceneInternalData LoadSceneData(string importedFilesDirectory)
    {
        // var files = Directory.GetFiles(importedFilesDirectory);
        // var sceneDataFilePath =
        //     files.Where(a => Path.GetFileNameWithoutExtension(a).StartsWith("SceneData") && !a.EndsWith(".meta"))
        //         .ToList();
        //
        // var sceneInternalDatas = new List<IT_SceneInternalData>(sceneDataFilePath.Count);
        //
        // for (var i = 0; i < sceneDataFilePath.Count; i++)
        // {
        //     var bytes = File.ReadAllBytes(sceneDataFilePath[i]);
        //     var value = SerializationUtility.DeserializeValue<IT_SceneInternalData>(bytes
        //         ,
        //         DataFormat.Binary);
        //
        //     sceneInternalDatas.Add(value);
        // }
        //
        // return sceneInternalDatas[sceneInternalDatas.Count - 1];


        var sceneDataFilePath = string.Concat(importedFilesDirectory, "/SceneData_UnityExport");
        var bytes = File.ReadAllBytes(sceneDataFilePath);
        var loadSceneData = SerializationUtility.DeserializeValue<IT_SceneInternalData>(bytes, DataFormat.Binary);
        return loadSceneData;
    }
}