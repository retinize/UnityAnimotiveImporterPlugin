using System.IO;
using UnityEditor;
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

            var hardcodedPath =
                IT_AnimotiveImporterEditorUtilities.ConvertSystemPathToAssetDatabasePath(
                    IT_AnimotiveImporterEditorConstants.UnityFilesScenesDirectory);
            var fullOsPath = Path.Combine(Directory.GetCurrentDirectory(), hardcodedPath);

            if (!Directory.Exists(fullOsPath)) Directory.CreateDirectory(fullOsPath);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var unitySceneName = string.Concat(sceneName, IT_AnimotiveImporterEditorConstants.UnitySceneExtension);
            var sceneFullPath = Path.Combine(hardcodedPath, unitySceneName);


            var fullSourcePath = Path.Combine(fullOsPath, unitySceneName);
            var similarName = IT_AnimotiveImporterEditorUtilities.GetLatestSimilarFileName(fullOsPath, fullSourcePath,
                unitySceneName,
                IT_AnimotiveImporterEditorConstants.UnitySceneExtension);

            if (File.Exists(fullSourcePath))
            {
                similarName = IT_AnimotiveImporterEditorUtilities.ConvertSystemPathToAssetDatabasePath(similarName);
                sceneFullPath = similarName;
            }


            EditorSceneManager.SaveScene(scene, sceneFullPath);

            AssetDatabase.Refresh();

            return scene;
        }
    }
}