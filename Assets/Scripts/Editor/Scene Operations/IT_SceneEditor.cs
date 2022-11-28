using System.IO;
using System.Threading.Tasks;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Retinize.Editor.AnimotiveImporter
{
    public static class IT_SceneEditor
    {
        /// <summary>
        ///     Creates scene asset at the designated location
        /// </summary>
        /// <param name="sceneName">Name of the scene to be created.</param>
        public static Scene CreateScene(string sceneName)
        {
            if (!Directory.Exists(IT_AnimotiveImporterEditorConstants.UnityFilesScenesDirectory))
                Directory.CreateDirectory(IT_AnimotiveImporterEditorConstants.UnityFilesScenesDirectory);


            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var assetNameToSave = string.Concat(sceneName,
                IT_AnimotiveImporterEditorConstants.UnitySceneExtension);

            assetNameToSave = IT_AnimotiveImporterEditorUtilities.GetUniqueAssetDatabaseName(
                assetNameToSave);

            var assetDbPathToSave = Path.Combine(
                IT_AnimotiveImporterEditorConstants.UnityFilesScenesDirectory,
                assetNameToSave);

            EditorSceneManager.SaveScene(scene, assetDbPathToSave);


            return scene;
        }
    }
}