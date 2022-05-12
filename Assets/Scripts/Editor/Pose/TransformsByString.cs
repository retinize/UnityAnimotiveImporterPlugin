namespace Retinize.Editor.AnimotiveImporter
{
    using System;
    using UnityEngine;

    [Serializable]
    public class TransformsByString
    {
        public HumanBodyBones Name;
        public Vector3        LocalPosition;
        public Quaternion     LocalRotation;
        public Vector3        LocalScale;
        public Vector3        LocalEulerAngles;
        public Quaternion     GlobalRotation;

        public TransformsByString(Transform tr, HumanBodyBones humanBodyBones)
        {
            Name             = humanBodyBones;
            LocalPosition    = tr.localPosition;
            LocalRotation    = tr.localRotation;
            LocalScale       = tr.localScale;
            LocalEulerAngles = tr.localEulerAngles;
            GlobalRotation   = tr.rotation;
        }
    }
}