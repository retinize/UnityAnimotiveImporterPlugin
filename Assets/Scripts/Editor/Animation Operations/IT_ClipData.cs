namespace Retinize.Editor.AnimotiveImporter
{
    /// <summary>
    ///     Holds clip player data (binary) with path to it and type of it.
    /// </summary>
    public struct IT_ClipData<T>
    {
        public IT_ClipType Type { get; }
        public T ClipPlayerData { get; }

        public string ClipDataPath { get; }


        public IT_ClipData(IT_ClipType type, T clipPlayerData, string clipDataPath)
        {
            Type = type;
            ClipPlayerData = clipPlayerData;
            ClipDataPath = clipDataPath;
        }
    }
}