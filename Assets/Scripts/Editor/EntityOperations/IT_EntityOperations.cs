using System;
using System.Collections.Generic;
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
    public static async void HandleEntityOperations(IT_SceneInternalData sceneData)
    {
        var entityTypeList = await IT_AnimotiveImporterEditorUtilities.GetPropertiesData(sceneData);


        var entitiesRoot = new GameObject("Entities");

        var typeRoots = new Dictionary<IT_EntityType, GameObject>();


        foreach (var pair in entityTypeList)
        {
            var currentTypeRoot = new GameObject(string.Concat(pair.Key, "_Root"));
            currentTypeRoot.transform.SetParent(entitiesRoot.transform);
            typeRoots.Add(pair.Key, currentTypeRoot);

            for (var i = 0; i < pair.Value.Count; i++)
            {
                var baseEntity = pair.Value[i];

                var tuple = CreateObjectsForEntity(baseEntity, typeRoots[pair.Key]);
                var holderObject = tuple.Item1;
                var rootObject = tuple.Item2;


                switch (pair.Key)
                {
                    case IT_EntityType.Camera:
                    {
                        var cam = rootObject.AddComponent<Camera>();
                        var cameraEntity = (IT_CameraEntity) pair.Value[i];
                        var fov = IT_AnimotiveImporterEditorUtilities.GetFieldOfView(cam, cameraEntity.FocalLength);

                        cam.fieldOfView = fov;
                        break;
                    }
                    case IT_EntityType.Spotlight:
                    {
                        var light = rootObject.AddComponent<Light>();
                        light.type = LightType.Spot;
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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