using System.IO;
using AnimotiveImporterDLL;
using OdinSerializer;

public static class IT_SceneDataOperations
{
    public static IT_SceneInternalData LoadSceneData(string importedFilesDirectory)
    {
        var sceneDataFilePath = importedFilesDirectory + "/SceneData_UnityExport";
        var bytes = File.ReadAllBytes(sceneDataFilePath);
        var loadSceneData = SerializationUtility.DeserializeValue<IT_SceneInternalData>(bytes, DataFormat.Binary);
        return loadSceneData;
    }
}