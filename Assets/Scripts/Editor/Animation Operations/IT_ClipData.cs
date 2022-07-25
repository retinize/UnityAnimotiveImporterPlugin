using AnimotiveImporterDLL;

namespace Retinize.Editor.AnimotiveImporter
{
    public struct IT_ClipData
    {
        public IT_ClipType Type { get; }
        public IT_ClipPlayerData ClipPlayerData { get; }

        public string clipDataPath { get; }


        public IT_ClipData(IT_ClipType type, IT_ClipPlayerData clipPlayerData, string clipDataPath)
        {
            Type = type;
            ClipPlayerData = clipPlayerData;
            this.clipDataPath = clipDataPath;
        }
    }
}