#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    public struct IT_FbxData
    {
        public GameObject FbxGameObject { get; }
        public Animator FbxAnimator { get; }

        public Dictionary<int, string> humanBodyBoneEnumAsIntByHumanoidBoneName { get; }

        public IT_FbxData(GameObject fbxGameObject, Animator fbxAnimator,
            Dictionary<int, string> humanBodyBoneEnumAsIntByHumanoidBoneName)
        {
            FbxGameObject = fbxGameObject;
            FbxAnimator = fbxAnimator;
            this.humanBodyBoneEnumAsIntByHumanoidBoneName = humanBodyBoneEnumAsIntByHumanoidBoneName;
        }
    }
}
#endif