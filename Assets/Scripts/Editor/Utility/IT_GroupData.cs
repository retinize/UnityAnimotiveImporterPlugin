using System.Collections.Generic;

namespace Retinize.Editor.AnimotiveImporter
{
    public class IT_GroupData
    {
        public List<IT_ClipData> ClipDatas;
        public int serializedId { get; }

        public string GroupName { get; }

        public IT_GroupData(int serializedId, string groupName)
        {
            this.serializedId = serializedId;
            GroupName = groupName;
            ClipDatas = new List<IT_ClipData>();
        }
    }
}