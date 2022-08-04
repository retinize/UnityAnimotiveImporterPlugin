using System.Collections.Generic;

namespace Retinize.Editor.AnimotiveImporter
{
    /// <summary>
    ///     Grouping class to hold take datas, group names and id
    /// </summary>
    public class IT_GroupData
    {
        public IT_GroupData(int serializedId, string groupName)
        {
            SerializedId = serializedId;
            GroupName = groupName;
            TakeDatas = new Dictionary<int, IT_TakeData>();
        }

        public int SerializedId { get; }

        public string GroupName { get; }

        public Dictionary<int, IT_TakeData> TakeDatas { get; set; }
    }

    /// <summary>
    ///     Class to hold take datas as in cluster.
    /// </summary>
    public class IT_TakeData
    {
        public List<IT_ClipCluster> Clusters;

        public IT_TakeData(int takeIndex)
        {
            TakeIndex = takeIndex;
            Clusters = new List<IT_ClipCluster>();
        }

        public int TakeIndex { get; }
    }

    /// <summary>
    ///     Class to hold animation, audio and property datas coupled.
    /// </summary>
    public class IT_ClipCluster
    {
        public IT_ClipCluster()
        {
            AudioClip = new IT_ClipData();
            TransformClip = new IT_ClipData();
            PropertiesClip = new IT_ClipData();
            IsInit = true;
            IsAnimationProcessInterrupted = false;
        }

        public IT_ClipData AudioClip { get; private set; }
        public IT_ClipData TransformClip { get; private set; }
        public IT_ClipData PropertiesClip { get; private set; }

        public bool IsAnimationProcessInterrupted { get; private set; }
        public string ModelName { get; set; }
        public int TakeIndex { get; set; }
        public bool IsInit { get; }


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

        public void SetInterruptionValue(bool value)
        {
            IsAnimationProcessInterrupted = value;
        }
    }
}