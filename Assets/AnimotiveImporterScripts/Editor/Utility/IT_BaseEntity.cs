using UnityEngine;

#if UNITY_EDITOR
namespace Retinize.Editor.AnimotiveImporter
{
    public class IT_BaseEntity
    {
        public Vector3 HolderPosition { get; protected set; }
        public Vector3 RootPosition { get; protected set; }
        public Quaternion HolderRotation { get; protected set; }
        public Quaternion RootRotation { get; protected set; }

        public string DisplayName { get; protected set; }

        public IT_BaseEntity(Vector3 holderPosition, Vector3 rootPosition, Quaternion holderRotation,
            Quaternion rootRotation, string displayName)
        {
            HolderPosition = holderPosition;
            RootPosition = rootPosition;
            HolderRotation = holderRotation;
            RootRotation = rootRotation;
            DisplayName = displayName;
        }
    }

    public class IT_CameraEntity : IT_BaseEntity
    {
        public float FocalLength { get; }

        public IT_CameraEntity(Vector3 holderPosition, Vector3 rootPosition, Quaternion holderRotation,
            Quaternion rootRotation, string displayName, float focalLength) : base(holderPosition, rootPosition,
            holderRotation, rootRotation, displayName)
        {
            FocalLength = focalLength;
        }
    }
}
#endif