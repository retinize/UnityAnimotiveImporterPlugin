using System.Collections.Generic;
using AnimotiveImporterDLL;
using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    public struct IT_ClipByDictionaryTuple
    {
        public IT_CharacterTransformAnimationClip Clip { get; }
        public Dictionary<HumanBodyBones, Transform> TransformsByHumanBodyBones { get; }

        public IT_ClipByDictionaryTuple(IT_CharacterTransformAnimationClip clip,
            Dictionary<HumanBodyBones, Transform> transformsByHumanBodyBones)
        {
            Clip = clip;
            TransformsByHumanBodyBones = transformsByHumanBodyBones;
        }
    }
}