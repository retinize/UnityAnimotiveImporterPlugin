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
    /// <param name="groupDatas"></param>
    public static async Task HandleEntityOperations(IT_SceneInternalData sceneData, List<IT_GroupData> groupDatas)
    {
        var entityTypeList = await IT_AnimotiveImporterEditorUtilities.GetPropertiesData(sceneData);


        var entitiesRoot = new GameObject("Entities");

        CreateArgsForEntitiesAndExecuteEntitySpecificOperations(entityTypeList, entitiesRoot);

        CreateAnimationClip(groupDatas);
    }


    private static void CreateAnimationClip(List<IT_GroupData> groupDatas)
    {
        for (var i = 0; i < groupDatas.Count; i++)
        {
            var groupData = groupDatas[i];

            for (var j = 0; j < groupData.TakeDatas.Count; j++)
            {
                var takeData = groupData.TakeDatas[j];

                for (var k = 0; k < takeData.Clusters.Count; k++)
                {
                    var cluster = takeData.Clusters[k];

                    if (cluster.ClusterType != IT_ClusterType.CameraCluster)
                        continue; //if not camera cluster then continue

                    var transformClip = cluster.ClipDatas[IT_ClipType.TransformAnimationClip];

                    Debug.Log("----------");
                    Debug.Log(cluster.EntityName);
                    Debug.Log(transformClip.ClipDataPath); //full os path
                    Debug.Log(transformClip.ClipPlayerData.clipName);
                }
            }
        }
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