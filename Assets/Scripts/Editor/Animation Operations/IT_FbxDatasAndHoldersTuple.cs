using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    public struct IT_FbxDatasAndHoldersTuple
    {
        public IT_FbxData FbxData { get; }
        public GameObject HolderObject { get; }

        public IT_TransformInfoList EditorTPose { get; }

        public IT_FbxDatasAndHoldersTuple(IT_FbxData fbxData, GameObject holderObject, IT_TransformInfoList editorTPose)
        {
            FbxData = fbxData;
            HolderObject = holderObject;
            EditorTPose = editorTPose;
        }
    }
}