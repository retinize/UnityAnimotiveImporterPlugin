using AnimotiveImporterDLL;

namespace Retinize.Editor.AnimotiveImporter
{
    public struct IT_ClipData
    {
        public IT_ClipType Type { get; }
        public IT_ClipPlayerData ClipPlayerData { get; }

        public string animationClipDataPath { get; }

        public string ModelName { get; }

        public int TakeIndex { get; }

        public IT_ClipData(IT_ClipType type, IT_ClipPlayerData clipPlayerData, string animationClipDataPath,
            string modelName, int takeIndex)
        {
            Type = type;
            ClipPlayerData = clipPlayerData;
            this.animationClipDataPath = animationClipDataPath;
            ModelName = modelName;
            TakeIndex = takeIndex;
        }
    }
}