using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Retinize.Editor.AnimotiveImporter
{
    public static class IT_SceneEditor
    {
        /// <summary>
        ///     Creates scene at the designated location.
        /// </summary>
        /// <param name="sceneName">Name of the scene to be created.</param>
        public static void CreateScene(string sceneName)
        {
            var hardcodedPath = @"Assets\AnimotivePluginExampleStructure\UnityFiles\Scenes\";
            var fullOsPath = Path.Combine(Directory.GetCurrentDirectory(), hardcodedPath);

            if (!Directory.Exists(fullOsPath)) Directory.CreateDirectory(fullOsPath);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var unitySceneName = string.Concat(sceneName, IT_AnimotiveImporterEditorConstants.UnitySceneExtension);
            var sceneFullPath = Path.Combine(hardcodedPath, unitySceneName);


            var fullSourcePath = Path.Combine(fullOsPath, unitySceneName);
            var similarName = IT_AnimotiveImporterEditorUtilities.GetLatestSimilarFileName(fullOsPath, fullSourcePath,
                unitySceneName,
                "unity");

            if (File.Exists(fullSourcePath))
            {
                similarName = IT_AnimotiveImporterEditorUtilities.ConvertPathToAssetDatabasePath(similarName);
                EditorSceneManager.SaveScene(scene, similarName);
            }
            else
                EditorSceneManager.SaveScene(scene, sceneFullPath);


            AssetDatabase.Refresh();
        }
    }
}