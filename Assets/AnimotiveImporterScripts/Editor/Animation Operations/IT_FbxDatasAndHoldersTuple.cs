﻿using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    /// <summary>
    ///     Holds T-Pose, fbx data and holder object for convenience.
    /// </summary>
    public struct IT_FbxDatasAndHoldersTuple
    {
        public IT_FbxData FbxData { get; }
        public GameObject HolderObject { get; }

        public IT_FbxDatasAndHoldersTuple(IT_FbxData fbxData, GameObject holderObject)
        {
            FbxData = fbxData;
            HolderObject = holderObject;
        }
    }
}