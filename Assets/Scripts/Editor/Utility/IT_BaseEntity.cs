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

    public abstract class IT_BaseEntity
    {
        public abstract IT_EntityType EntityType { get; }
        public abstract Vector3 HolderPosition { get; protected set; }
        public abstract Vector3 RootPosition { get; protected set; }
        public abstract Quaternion HolderRotation { get; protected set; }
        public abstract Quaternion RootRotation { get; protected set; }

        public abstract string DisplayName { get; protected set; }

        public abstract void ExecuteEntitySpecificOperations(IT_EntitySpecificOperationsArgs args);
    }

    public class IT_CameraEntity : IT_BaseEntity
    {
        public sealed override IT_EntityType EntityType { get; }
        public sealed override Vector3 HolderPosition { get; protected set; }
        public sealed override Vector3 RootPosition { get; protected set; }
        public sealed override Quaternion HolderRotation { get; protected set; }
        public sealed override Quaternion RootRotation { get; protected set; }
        public sealed override string DisplayName { get; protected set; }

        public float FocalLength { get; }

        public IT_CameraEntity(Vector3 holderPosition, Vector3 rootPosition,
            Quaternion holderRotation, Quaternion rootRotation, string displayName, float focalLength)
        {
            EntityType = IT_EntityType.Camera;
            HolderPosition = holderPosition;
            RootPosition = rootPosition;
            HolderRotation = holderRotation;
            RootRotation = rootRotation;
            DisplayName = displayName;
            FocalLength = focalLength;
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


            Debug.Log("root position " + RootPosition);
            Debug.Log("holder position " + HolderPosition);
            cameraRoot.transform.position = RootPosition;
            cameraRoot.transform.rotation = RootRotation;


            cameraHolder.transform.position = HolderPosition;
            cameraHolder.transform.rotation = HolderRotation;


            var cam = entityGameObjectInTheScene.AddComponent<Camera>();
            var fov = IT_AnimotiveImporterEditorUtilities.GetFieldOfView(cam, FocalLength);

            cam.nearClipPlane = 0.001f;
            cam.farClipPlane = 1000;
            cam.fieldOfView = fov;

            entityGameObjectInTheScene.transform.localPosition = new Vector3(-0.0079f, 0.2397f, 0.0255f);
        }
    }

    public class IT_SpotLightEntity : IT_BaseEntity
    {
        public sealed override IT_EntityType EntityType { get; }
        public sealed override Vector3 HolderPosition { get; protected set; }
        public sealed override Vector3 RootPosition { get; protected set; }
        public sealed override Quaternion HolderRotation { get; protected set; }
        public sealed override Quaternion RootRotation { get; protected set; }
        public sealed override string DisplayName { get; protected set; }

        public IT_SpotLightEntity(Vector3 holderPosition, Vector3 rootPosition,
            Quaternion holderRotation, Quaternion rootRotation, string displayName)
        {
            EntityType = IT_EntityType.Spotlight;
            HolderPosition = holderPosition;
            RootPosition = rootPosition;
            HolderRotation = holderRotation;
            RootRotation = rootRotation;
            DisplayName = displayName;
        }

        public override void ExecuteEntitySpecificOperations(IT_EntitySpecificOperationsArgs args)
        {
            var entityGameObjectInTheScene = new GameObject(DisplayName);

            var light = entityGameObjectInTheScene.AddComponent<Light>();
            light.type = LightType.Spot;
        }
    }
}
#endif