using System.Collections.Generic;

namespace Retinize.Editor.AnimotiveImporter
{
    public class IT_GroupData
    {
        public int serializedId { get; }

        public string GroupName { get; }

        public Dictionary<int, IT_TakeData> TakeDatas { get; set; }

        public IT_GroupData(int serializedId, string groupName)
        {
            this.serializedId = serializedId;
            GroupName = groupName;
            TakeDatas = new Dictionary<int, IT_TakeData>();
        }
    }

    public class IT_TakeData
    {
        public List<IT_ClipCluster> Clusters;

        public int TakeIndex { get; }

        public IT_TakeData(int takeIndex)
        {
            TakeIndex = takeIndex;
            Clusters = new List<IT_ClipCluster>();
        }
    }

    public class IT_ClipCluster
    {
        public IT_ClipData AudioClip { get; private set; }
        public IT_ClipData TransformClip { get; private set; }
        public IT_ClipData PropertiesClip { get; private set; }

        public string ModelName { get; set; }
        public int TakeIndex { get; set; }
        public bool IsInit { get; }

        public IT_ClipCluster()
        {
            AudioClip = new IT_ClipData();
            TransformClip = new IT_ClipData();
            PropertiesClip = new IT_ClipData();
            IsInit = true;
        }


        public void SetAudioClip(IT_ClipData clipData)
        {
            AudioClip = clipData;
        }

        public void SetTransformClip(IT_ClipData clipData)
        {
            TransformClip = clipData;
        }

        public void SetPropertiesClip(IT_ClipData clipData)
        {
            PropertiesClip = clipData;
        }
    }
}