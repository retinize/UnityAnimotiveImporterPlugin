using UnityEngine;

#if UNITY_EDITOR
namespace Retinize.Editor.AnimotiveImporter
{
    public sealed class IT_EntitySpecificOperationsArgs
    {
        public GameObject holderObject;
        public GameObject rootObject;

        public IT_EntitySpecificOperationsArgs(GameObject holderObject, GameObject rootObject)
        {
            this.holderObject = holderObject;
            this.rootObject = rootObject;
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
            var cam = args.rootObject.AddComponent<Camera>();
            var fov = IT_AnimotiveImporterEditorUtilities.GetFieldOfView(cam, FocalLength);

            cam.fieldOfView = fov;
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
            var light = args.rootObject.AddComponent<Light>();
            light.type = LightType.Spot;
        }
    }
}
#endif