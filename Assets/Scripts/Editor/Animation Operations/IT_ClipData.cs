using AnimotiveImporterDLL;

namespace Retinize.Editor.AnimotiveImporter
{
    /// <summary>
    ///     Holds clip player data (binary) with path to it and type of it.
    /// </summary>
    public struct IT_ClipData
    {
        public IT_ClipType Type { get; }
        public IT_ClipPlayerData ClipPlayerData { get; }

        public string ClipDataPath { get; }


        public IT_ClipData(IT_ClipType type, IT_ClipPlayerData clipPlayerData, string clipDataPath)
        {
            Type = type;
            ClipPlayerData = clipPlayerData;
            ClipDataPath = clipDataPath;
        }
    }
}