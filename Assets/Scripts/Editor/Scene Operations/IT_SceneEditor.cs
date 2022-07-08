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
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene,
                string.Concat(hardcodedPath, Path.DirectorySeparatorChar, sceneName,
                    IT_AnimotiveImporterEditorConstants.UnitySceneExtension));


            AssetDatabase.Refresh();
        }
    }
}