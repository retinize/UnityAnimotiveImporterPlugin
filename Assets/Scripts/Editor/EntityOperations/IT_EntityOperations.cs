using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnimotiveImporterDLL;
using Retinize.Editor.AnimotiveImporter;
using UnityEngine;

public static class IT_EntityOperations
{
    /// <summary>
    ///     Creates cameras and spotlights in the scene
    /// </summary>
    /// <param name="sceneData"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static async Task HandleEntityOperations(IT_SceneInternalData sceneData)
    {
        var entityTypeList = await IT_AnimotiveImporterEditorUtilities.GetPropertiesData(sceneData);


        var entitiesRoot = new GameObject("Entities");

        CreateArgsForEntitiesAndExecuteEntitySpecificOperations(entityTypeList, entitiesRoot);
    }

    private static void CreateArgsForEntitiesAndExecuteEntitySpecificOperations(
        Dictionary<IT_EntityType, List<IT_BaseEntity>> entityTypeList,
        GameObject entitiesRoot)
    {
        var currentTypeRoot = new GameObject(string.Concat(IT_EntityType.Camera, "_Root"));
        currentTypeRoot.transform.SetParent(entitiesRoot.transform);

        foreach (var pair in entityTypeList)
        {
            for (var i = 0; i < pair.Value.Count; i++)
            {
                var baseEntity = pair.Value[i];

                var tuple = CreateObjectsForEntity(baseEntity, currentTypeRoot);
                var holderObject = tuple.Item1;
                var rootObject = tuple.Item2;


                baseEntity.ExecuteEntitySpecificOperations(
                    new IT_EntitySpecificOperationsArgs(holderObject, rootObject));
            }
        }
    }

    private static Tuple<GameObject, GameObject> CreateObjectsForEntity(IT_BaseEntity baseEntity, GameObject typeRoot)
    {
        var holderObject = new GameObject(baseEntity.DisplayName + "_HOLDER");
        holderObject.transform.SetParent(typeRoot.transform);
        var rootObject = new GameObject(baseEntity.DisplayName + "_Root");

        rootObject.transform.SetParent(holderObject.transform);

        holderObject.transform.position = baseEntity.HolderPosition;
        holderObject.transform.rotation = baseEntity.HolderRotation;

        rootObject.transform.position = baseEntity.RootPosition;
        rootObject.transform.rotation = baseEntity.RootRotation;


        return new Tuple<GameObject, GameObject>(holderObject, rootObject);
    }
}