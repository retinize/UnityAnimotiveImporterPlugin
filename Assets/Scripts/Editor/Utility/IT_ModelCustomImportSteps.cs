using Retinize.Editor.AnimotiveImporter;
using UnityEditor;
 
public class IT_ModelCustomImportSteps : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        if (IT_AnimotiveImporterEditorWindow.EnableImportConfig)  // @-sign in the name triggers this step
        {
            ModelImporter modelImporter = assetImporter as ModelImporter;
            modelImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        }
    }
}