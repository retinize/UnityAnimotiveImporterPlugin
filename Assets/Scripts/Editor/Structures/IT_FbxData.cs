#if UNITY_EDITOR
using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    public struct IT_FbxData
    {
        public GameObject FbxGameObject { get; }
        public Animator FbxAnimator { get; }

        public IT_FbxData(GameObject fbxGameObject, Animator fbxAnimator)
        {
            FbxGameObject = fbxGameObject;
            FbxAnimator = fbxAnimator;
        }
    }
}
#endif