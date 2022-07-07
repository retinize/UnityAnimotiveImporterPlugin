using System.IO;
using Retinize.Editor.AnimotiveImporter;
using UnityEditor;

public class IT_ModelCustomImportSteps : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        if (IT_AnimotiveImporterEditorWindow.EnableImportConfig) // @-sign in the name triggers this step
        {
            ModelImporter modelImporter = assetImporter as ModelImporter;
            if (modelImporter != null)
            {
                modelImporter.animationType = ModelImporterAnimationType.Human;
                modelImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                string directoryName = IT_AnimotiveImporterEditorWindow.ImportedFbxAssetDatabasePath;
                string texturesPath = Path.Combine(Path.GetDirectoryName(directoryName),
                    "Textures");
                string fullOSPath = Path.Combine(Directory.GetCurrentDirectory(), texturesPath);
                if (!Directory.Exists(fullOSPath))
                {
                    Directory.CreateDirectory(fullOSPath);
                }

                modelImporter.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                // modelImporter.materialLocation = ModelImporterMaterialLocation.InPrefab;
                modelImporter.ExtractTextures(texturesPath);
                AssetDatabase.Refresh();
            }
        }
    }
}