using System.Collections.Generic;
using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    public struct DictionaryTuple
    {
        public Dictionary<HumanBodyBones, Transform> TransformsByHumanBodyBones{ get;  }
        public Dictionary<Transform, HumanBodyBones> HumanBodyBonesByTransform { get;  }

        public DictionaryTuple(Dictionary<HumanBodyBones, Transform> transformsByHumanBodyBones, Dictionary<Transform, HumanBodyBones> humanBodyBonesByTransform)
        {
            TransformsByHumanBodyBones = transformsByHumanBodyBones;
            HumanBodyBonesByTransform = humanBodyBonesByTransform;
        }

        public DictionaryTuple(int a=0)
        {
            TransformsByHumanBodyBones = new Dictionary<HumanBodyBones, Transform>();
            HumanBodyBonesByTransform = new Dictionary<Transform, HumanBodyBones>();
        }
    }
}