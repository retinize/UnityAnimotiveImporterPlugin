using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AnimotiveImporterDLL;
using OdinSerializer;
using Retinize.Editor.AnimotiveImporter;
using UnityEditor;
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

                    var propertyClip = cluster.ClipDatas[IT_ClipType.PropertiesClip];
                    var cameraCluster = (IT_CameraCluster) cluster;

                    cameraCluster.ReferenceInScene = GameObject.Find(cluster.EntityName);


                    var deserializeValue = SerializationUtility.DeserializeValue<IT_FixedVideoCameraPropertyClip>(
                        File.ReadAllBytes(propertyClip.ClipDataPath), DataFormat.Binary);


                    var keyframesList = new List<List<Keyframe>>();
                    keyframesList = new List<List<Keyframe>>();


                    keyframesList.Add(GetFramesAsList(deserializeValue.curves[1].KeyFrames,
                        deserializeValue.fixedDeltaTime)); //camera local position x
                    keyframesList.Add(GetFramesAsList(deserializeValue.curves[2].KeyFrames,
                        deserializeValue.fixedDeltaTime)); //camera local position y
                    keyframesList.Add(GetFramesAsList(deserializeValue.curves[3].KeyFrames,
                        deserializeValue.fixedDeltaTime)); //camera local position z
                    keyframesList.Add(GetFramesAsList(deserializeValue.curves[4].KeyFrames,
                        deserializeValue.fixedDeltaTime)); //camera local rotation x
                    keyframesList.Add(GetFramesAsList(deserializeValue.curves[5].KeyFrames,
                        deserializeValue.fixedDeltaTime)); //camera local rotation y
                    keyframesList.Add(GetFramesAsList(deserializeValue.curves[6].KeyFrames,
                        deserializeValue.fixedDeltaTime)); //camera local rotation z
                    keyframesList.Add(GetFramesAsList(deserializeValue.curves[7].KeyFrames,
                        deserializeValue.fixedDeltaTime)); //camera local rotation w

                    var animationClip = CreateAnimationClip(keyframesList);

                    var animationClipAssetDatabasePath = IT_AnimotiveImporterEditorUtilities
                        .ConvertFullFilePathIntoUnityFilesPath(
                            IT_AnimotiveImporterEditorConstants.UnityFilesCameraAnimationDirectory,
                            propertyClip.ClipDataPath, IT_AnimotiveImporterEditorConstants.AnimationExtension);

                    AssetDatabase.CreateAsset(animationClip, animationClipAssetDatabasePath);

                    cameraCluster.PropertiesDataAnimationClipAssetDatabasePath = animationClipAssetDatabasePath;
                }
            }
        }
    }

    private static AnimationClip CreateAnimationClip(List<List<Keyframe>> listOfKeyframeLists)
    {
        var animationClip = new AnimationClip();

        var cameraLocalPositionXCurve = new AnimationCurve(listOfKeyframeLists[0].ToArray());
        var cameraLocalPositionYCurve = new AnimationCurve(listOfKeyframeLists[1].ToArray());
        var cameraLocalPositionZCurve = new AnimationCurve(listOfKeyframeLists[2].ToArray());
        var cameraLocalRotationXCurve = new AnimationCurve(listOfKeyframeLists[3].ToArray());
        var cameraLocalRotationYCurve = new AnimationCurve(listOfKeyframeLists[4].ToArray());
        var cameraLocalRotationZCurve = new AnimationCurve(listOfKeyframeLists[5].ToArray());
        var cameraLocalRotationWCurve = new AnimationCurve(listOfKeyframeLists[6].ToArray());

        animationClip.SetCurve("", typeof(Transform), "localPosition.x", cameraLocalPositionXCurve);
        animationClip.SetCurve("", typeof(Transform), "localPosition.y", cameraLocalPositionYCurve);
        animationClip.SetCurve("", typeof(Transform), "localPosition.z", cameraLocalPositionZCurve);

        animationClip.SetCurve("", typeof(Transform), "localRotation.x", cameraLocalRotationXCurve);
        animationClip.SetCurve("", typeof(Transform), "localRotation.y", cameraLocalRotationYCurve);
        animationClip.SetCurve("", typeof(Transform), "localRotation.z", cameraLocalRotationZCurve);
        animationClip.SetCurve("", typeof(Transform), "localRotation.w", cameraLocalRotationWCurve);


        return animationClip;
    }


    private static List<Keyframe> GetFramesAsList(List<float> frameList, float step)
    {
        var list = new List<Keyframe>();

        for (var i = 0; i < frameList.Count; i++)
        {
            list.Add(new Keyframe(step * i, frameList[i]));
        }

        return list;
    }


    private static void CreateArgsForEntitiesAndExecuteEntitySpecificOperations(
        Dictionary<IT_EntityType, List<IT_BaseEntity>> entityTypeList,
        GameObject entitiesRoot)
    {
        var currentTypeHead = new GameObject(string.Concat(IT_EntityType.Camera));
        currentTypeHead.transform.SetParent(entitiesRoot.transform);


        foreach (var pair in entityTypeList)
        {
            for (var i = 0; i < pair.Value.Count; i++)
            {
                var baseEntity = pair.Value[i];
                baseEntity.ExecuteEntitySpecificOperations(
                    new IT_EntitySpecificOperationsArgs(currentTypeHead));
            }
        }
    }
}