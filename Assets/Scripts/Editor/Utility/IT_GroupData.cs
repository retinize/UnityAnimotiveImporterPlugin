using System.Collections.Generic;
using AnimotiveImporterDLL;

namespace Retinize.Editor.AnimotiveImporter
{
    /// <summary>
    ///     Grouping class to hold take datas, group names and id
    /// </summary>
    public class IT_GroupData
    {
        public int SerializedId { get; }

        public string GroupName { get; }

        public Dictionary<int, IT_TakeData> TakeDatas { get; set; }

        public IT_GroupData(int serializedId, string groupName)
        {
            SerializedId = serializedId;
            GroupName = groupName;
            TakeDatas = new Dictionary<int, IT_TakeData>();
        }
    }

    /// <summary>
    ///     Class to hold take datas as in cluster.
    /// </summary>
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

    /// <summary>
    ///     Class to hold animation, audio and property datas coupled.
    /// </summary>
    public class IT_ClipCluster
    {
        public IT_ClipData<IT_ClipPlayerData> AudioClip { get; private set; }
        public IT_ClipData<IT_ClipPlayerData> TransformClip { get; private set; }
        public IT_ClipData<IT_ClipPlayerData> PropertiesClip { get; private set; }

        public bool IsAnimationProcessInterrupted { get; private set; }
        public string ModelName { get; set; }
        public int TakeIndex { get; set; }
        public bool IsInit { get; }

        public IT_ClipCluster()
        {
            AudioClip = new IT_ClipData<IT_ClipPlayerData>();
            TransformClip = new IT_ClipData<IT_ClipPlayerData>();
            PropertiesClip = new IT_ClipData<IT_ClipPlayerData>();
            IsInit = true;
            IsAnimationProcessInterrupted = false;
        }


        public void SetAudioClip(IT_ClipData<IT_ClipPlayerData> clipData)
        {
            AudioClip = clipData;
        }

        public void SetTransformClip(IT_ClipData<IT_ClipPlayerData> clipData)
        {
            TransformClip = clipData;
        }

        public void SetPropertiesClip(IT_ClipData<IT_ClipPlayerData> clipData)
        {
            PropertiesClip = clipData;
        }

        public void SetInterruptionValue(bool value)
        {
            IsAnimationProcessInterrupted = value;
        }
    }
}