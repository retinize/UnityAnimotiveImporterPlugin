using System.Collections.Generic;

namespace Retinize.Editor.AnimotiveImporter
{
    public class IT_GroupData
    {
        public int serializedId { get; }

        public string GroupName { get; }

        public List<IT_TakeData> TakeDatas { get; set; }

        public IT_GroupData(int serializedId, string groupName)
        {
            this.serializedId = serializedId;
            GroupName = groupName;
            TakeDatas = new List<IT_TakeData>();
        }
    }

    public class IT_TakeData
    {
        public List<IT_ClipData> ClipDatas;

        public int TakeIndex { get; }

        public IT_TakeData(int takeIndex)
        {
            TakeIndex = takeIndex;
            ClipDatas = new List<IT_ClipData>();
        }
    }
}