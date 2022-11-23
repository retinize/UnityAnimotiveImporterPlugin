using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
namespace Retinize.Editor.AnimotiveImporter
{
    public sealed class IT_EntitySpecificOperationsArgs
    {
        public GameObject TypeHeadInTheScene;

        public IT_EntitySpecificOperationsArgs(GameObject typeHeadInTheScene)
        {
            TypeHeadInTheScene = typeHeadInTheScene;
        }
    }

    public abstract class IT_BaseEntity : IEntity
    {
        public Dictionary<string, object> PropertiesDataDictionary { get; }

        public IT_BaseEntity(IT_EntityType entityType, Vector3 holderPosition, Vector3 rootPosition,
            Quaternion holderRotation, Quaternion rootRotation, string displayName,
            Dictionary<string, object> propertiesDataDictionary
        )
        {
            EntityType = entityType;
            HolderPosition = holderPosition;
            RootPosition = rootPosition;
            HolderRotation = holderRotation;
            RootRotation = rootRotation;
            DisplayName = displayName;
            PropertiesDataDictionary = propertiesDataDictionary;
        }

        public IT_EntityType EntityType { get; protected set; }
        public Vector3 HolderPosition { get; protected set; }
        public Vector3 RootPosition { get; protected set; }
        public Quaternion HolderRotation { get; protected set; }
        public Quaternion RootRotation { get; protected set; }
        public string DisplayName { get; protected set; }

        public abstract void ExecuteEntitySpecificOperations(IT_EntitySpecificOperationsArgs args);
    }

    public interface IEntity
    {
        public IT_EntityType EntityType { get; }
        public Vector3 HolderPosition { get; }
        public Vector3 RootPosition { get; }
        public Quaternion HolderRotation { get; }
        public Quaternion RootRotation { get; }

        public string DisplayName { get; }

        public void ExecuteEntitySpecificOperations(IT_EntitySpecificOperationsArgs args);
    }

    public class IT_CameraEntity : IT_BaseEntity
    {
        public IT_CameraEntity(IT_EntityType entityType, Vector3 holderPosition, Vector3 rootPosition,
            Quaternion holderRotation, Quaternion rootRotation, string displayName,
            Dictionary<string, object> propertiesDataDictionary) : base(entityType, holderPosition, rootPosition,
            holderRotation, rootRotation, displayName, propertiesDataDictionary)
        {
        }

        public override void ExecuteEntitySpecificOperations(IT_EntitySpecificOperationsArgs args)
        {
            var entityGameObjectInTheScene = new GameObject(DisplayName);
            var cameraRoot = new GameObject(string.Concat(DisplayName, "_Root"));
            var cameraHolder = new GameObject(string.Concat(DisplayName, "_Holder"));
            var currentTypeHead = args.TypeHeadInTheScene;


            cameraHolder.transform.SetParent(currentTypeHead.transform);
            cameraRoot.transform.SetParent(cameraHolder.transform);
            entityGameObjectInTheScene.transform.SetParent(cameraRoot.transform);

            cameraHolder.transform.position = HolderPosition;
            cameraHolder.transform.rotation = HolderRotation;


            var focalLength =
                (float) PropertiesDataDictionary[
                    IT_AnimotiveImporterEditorConstants.DepthOfFieldFocalLength];

            var cam = entityGameObjectInTheScene.AddComponent<Camera>();
            var fov = IT_AnimotiveImporterEditorUtilities.GetFieldOfView(cam, focalLength);

            cam.nearClipPlane = 0.001f;
            cam.farClipPlane = 1000;
            cam.fieldOfView = fov;

            entityGameObjectInTheScene.transform.localPosition = new Vector3(-0.0079f, 0.2397f, 0.0255f);
        }
    }

    public class IT_SpotLightEntity : IT_BaseEntity
    {
        public IT_SpotLightEntity(IT_EntityType entityType, Vector3 holderPosition, Vector3 rootPosition,
            Quaternion holderRotation, Quaternion rootRotation, string displayName,
            Dictionary<string, object> propertiesDataDictionary) : base(entityType, holderPosition, rootPosition,
            holderRotation, rootRotation, displayName, propertiesDataDictionary)
        {
        }

        public override void ExecuteEntitySpecificOperations(IT_EntitySpecificOperationsArgs args)
        {
            var currentTypeHead = args.TypeHeadInTheScene;

            var entityGameObjectInTheScene = new GameObject(DisplayName);
            entityGameObjectInTheScene.transform.SetParent(currentTypeHead.transform);
            var light = entityGameObjectInTheScene.AddComponent<Light>();
            light.type = LightType.Spot;
        }
    }
}
#endif