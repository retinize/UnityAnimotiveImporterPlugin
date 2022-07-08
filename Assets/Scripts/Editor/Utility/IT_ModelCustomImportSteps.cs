using System.IO;
using Retinize.Editor.AnimotiveImporter;
using UnityEditor;

public class IT_ModelCustomImportSteps : AssetPostprocessor
{
    private void OnPreprocessModel()
    {
        if (IT_AnimotiveImporterEditorWindow.EnableImportConfig) // @-sign in the name triggers this step
        {
            var modelImporter = assetImporter as ModelImporter;
            if (modelImporter != null)
            {
                modelImporter.animationType = ModelImporterAnimationType.Human;
                modelImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                var directoryName = IT_AnimotiveImporterEditorWindow.ImportedFbxAssetDatabasePath;
                string dirName = Path.GetDirectoryName(directoryName);
                var texturesPath = Path.Combine(dirName, "Textures");
                var fullOSPath = Path.Combine(Directory.GetCurrentDirectory(), texturesPath);
                if (!Directory.Exists(fullOSPath))
                {
                    Directory.CreateDirectory(fullOSPath);
                    AssetDatabase.Refresh();
                }

                modelImporter.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                // modelImporter.materialLocation = ModelImporterMaterialLocation.InPrefab;
                modelImporter.ExtractTextures(texturesPath);
                AssetDatabase.Refresh();
            }
        }
    }
}