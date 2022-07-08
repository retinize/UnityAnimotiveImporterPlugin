using AnimotiveImporterDLL;

namespace Retinize.Editor.AnimotiveImporter
{
    public struct IT_ClipData
    {
        public IT_ClipPlayerData ClipPlayerData { get; }

        public string animationClipDataPath { get; }

        public IT_ClipData(IT_ClipPlayerData clipPlayerData, string animationClipDataPath)
        {
            ClipPlayerData = clipPlayerData;
            this.animationClipDataPath = animationClipDataPath;
        }
    }
}