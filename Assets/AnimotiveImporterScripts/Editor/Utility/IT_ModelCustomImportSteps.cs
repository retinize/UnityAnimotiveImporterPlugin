using System.IO;
using UnityEditor;

namespace Retinize.Editor.AnimotiveImporter
{
    /// <summary>
    ///     Class to modify FBX imports for characters
    /// </summary>
    public class IT_ModelCustomImportSteps : AssetPostprocessor
    {
        private void OnPreprocessModel()
        {
            if (IT_AnimotiveImporterEditorWindow
                .EnableImportConfig) // when plugin gives a green light to alter import settings this plugin will work
            {
                var modelImporter = assetImporter as ModelImporter;

                if (modelImporter != null)
                {
                    modelImporter.animationType = ModelImporterAnimationType.Human;
                    modelImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    var directoryName = IT_AnimotiveImporterEditorConstants.UnityFilesCharactersDirectory;
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
}