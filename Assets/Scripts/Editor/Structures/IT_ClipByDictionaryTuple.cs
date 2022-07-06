using AnimotiveImporterDLL;

namespace Retinize.Editor.AnimotiveImporter
{
    public struct IT_ClipByDictionaryTuple
    {
        public IT_CharacterTransformAnimationClip Clip { get; private set; }
        public IT_DictionaryTuple DictTuple { get; private set; }

        public IT_ClipByDictionaryTuple(IT_CharacterTransformAnimationClip clip, IT_DictionaryTuple dictTuple)
        {
            Clip = clip;
            DictTuple = dictTuple;
        }
    }
}