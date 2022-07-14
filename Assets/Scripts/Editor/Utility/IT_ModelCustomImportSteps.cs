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
                var directoryName = IT_AnimotiveImporterEditorWindow.ImportedCharactersAssetdatabaseDirectory;
                var texturesPath = Path.Combine(directoryName, "Textures");
                var fullOSPath = Path.Combine(Directory.GetCurrentDirectory(), texturesPath);
                if (!Directory.Exists(fullOSPath))
                {
                    Directory.CreateDirectory(fullOSPath);
                    AssetDatabase.Refresh();
                }

                modelImporter.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                modelImporter.ExtractTextures(texturesPath);
                AssetDatabase.Refresh();
            }
        }
    }
}