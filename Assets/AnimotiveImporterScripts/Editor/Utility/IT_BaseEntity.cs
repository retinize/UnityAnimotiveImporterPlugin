using UnityEngine;

#if UNITY_EDITOR
namespace Retinize.Editor.AnimotiveImporter
{
    public abstract class IT_BaseEntity
    {
        public abstract IT_EntityType EntityType { get; protected set; }
        public abstract Vector3 HolderPosition { get; protected set; }
        public abstract Vector3 RootPosition { get; protected set; }
        public abstract Quaternion HolderRotation { get; protected set; }
        public abstract Quaternion RootRotation { get; protected set; }

        public abstract string DisplayName { get; protected set; }
    }

    public class IT_CameraEntity : IT_BaseEntity
    {
        public override IT_EntityType EntityType { get; protected set; }
        public override Vector3 HolderPosition { get; protected set; }
        public override Vector3 RootPosition { get; protected set; }
        public override Quaternion HolderRotation { get; protected set; }
        public override Quaternion RootRotation { get; protected set; }
        public override string DisplayName { get; protected set; }

        public float FocalLength { get; }

        public IT_CameraEntity(IT_EntityType entityType, Vector3 holderPosition, Vector3 rootPosition,
            Quaternion holderRotation, Quaternion rootRotation, string displayName, float focalLength)
        {
            EntityType = entityType;
            HolderPosition = holderPosition;
            RootPosition = rootPosition;
            HolderRotation = holderRotation;
            RootRotation = rootRotation;
            DisplayName = displayName;
            FocalLength = focalLength;
        }
    }

    public class IT_SpotLightEntity : IT_BaseEntity
    {
        public override IT_EntityType EntityType { get; protected set; }
        public override Vector3 HolderPosition { get; protected set; }
        public override Vector3 RootPosition { get; protected set; }
        public override Quaternion HolderRotation { get; protected set; }
        public override Quaternion RootRotation { get; protected set; }
        public override string DisplayName { get; protected set; }

        public IT_SpotLightEntity(IT_EntityType entityType, Vector3 holderPosition, Vector3 rootPosition,
            Quaternion holderRotation, Quaternion rootRotation, string displayName)
        {
            EntityType = entityType;
            HolderPosition = holderPosition;
            RootPosition = rootPosition;
            HolderRotation = holderRotation;
            RootRotation = rootRotation;
            DisplayName = displayName;
        }
    }
}
#endif