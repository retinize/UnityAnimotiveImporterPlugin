namespace AnimotiveImporterEditor
{
    using System;
    using UnityEngine;

    [Serializable]
    public class TransformsByString
    {
        public string     Name;
        public Vector3    LocalPosition;
        public Quaternion LocalRotation;
        public Vector3    LocalScale;
        public Vector3    LocalEulerAngles;
        public Quaternion GlobalRotation;

        public TransformsByString(Transform tr)
        {
            Name             = tr.name;
            LocalPosition    = tr.localPosition;
            LocalRotation    = tr.localRotation;
            LocalScale       = tr.localScale;
            LocalEulerAngles = tr.localEulerAngles;
            GlobalRotation   = tr.rotation;
        }
    }
}