using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnimotiveImporterDLL;
using OdinSerializer;
using UnityEngine;

public static class IT_SceneDataOperations 
{
    public static  IT_SceneInternalData LoadSceneData(string importedFilesDirectory)
    {
        string[] files = Directory.GetFiles(importedFilesDirectory);
        var sceneDataFilePath =
            files.Where(a => Path.GetFileNameWithoutExtension(a).StartsWith("SceneData")).ToList()[0];

        return SerializationUtility.DeserializeValue<IT_SceneInternalData>(File.ReadAllBytes(sceneDataFilePath),
            DataFormat.Binary);
    }
}