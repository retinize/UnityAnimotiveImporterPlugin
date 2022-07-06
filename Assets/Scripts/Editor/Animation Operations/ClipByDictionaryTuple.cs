using AnimotiveImporterDLL;

namespace Retinize.Editor.AnimotiveImporter
{
    public struct ClipByDictionaryTuple
    {
        public IT_CharacterTransformAnimationClip Clip { get; private set; }
        public DictionaryTuple DictTuple { get; private set; }

        public ClipByDictionaryTuple(IT_CharacterTransformAnimationClip clip, DictionaryTuple dictTuple)
        {
            Clip = clip;
            DictTuple = dictTuple;
        }
    }
}