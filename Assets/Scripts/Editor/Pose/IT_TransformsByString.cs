using System;
using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    /// <summary>
    ///     This struct is a data container for serialized json pose.
    /// </summary>
    [Serializable]
    public struct IT_TransformsByString
    {
        public HumanBodyBones Name;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;
        public Vector3 LocalEulerAngles;
        public Quaternion GlobalRotation;

        public IT_TransformsByString(Transform tr, HumanBodyBones humanBodyBones)
        {
            Name = humanBodyBones;
            LocalPosition = tr.localPosition;
            LocalRotation = tr.localRotation;
            LocalScale = tr.localScale;
            LocalEulerAngles = tr.localEulerAngles;
            GlobalRotation = tr.rotation;
        }
    }
}