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

        public string TrimmedGroupName { get; }
        public string OriginalGroupName { get; }

        public Dictionary<int, IT_TakeData> TakeDatas { get; set; }

        public IT_GroupData(int serializedId, string groupName)
        {
            SerializedId = serializedId;
            TrimmedGroupName = groupName.Trim().Replace(" ", "");
            OriginalGroupName = groupName;
            TakeDatas = new Dictionary<int, IT_TakeData>();
        }
    }

    /// <summary>
    ///     Class to hold take datas as in cluster.
    /// </summary>
    public class IT_TakeData
    {
        public int TakeIndex { get; }
        public List<IT_ClipCluster> Clusters;

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
        public IT_ClipData<IT_ClipPlayerData> AudioClipData { get; private set; }
        public IT_ClipData<IT_ClipPlayerData> BodyAnimationClipData { get; private set; }
        public IT_ClipData<FacialAnimationExportWrapper> FacialAnimationClipData { get; private set; }
        public IT_ClipData<IT_ClipPlayerData> PropertiesClipData { get; private set; }

        public bool IsAnimationProcessInterrupted { get; private set; }
        public bool IsInit { get; }
        public string ModelName;
        public int NumberOfCaptureInWhichItWasCaptured = -1;
        public int TakeIndex;

        public IT_ClipCluster()
        {
            AudioClipData = new IT_ClipData<IT_ClipPlayerData>();
            BodyAnimationClipData = new IT_ClipData<IT_ClipPlayerData>();
            PropertiesClipData = new IT_ClipData<IT_ClipPlayerData>();
            FacialAnimationClipData = new IT_ClipData<FacialAnimationExportWrapper>();
            IsInit = true;
            IsAnimationProcessInterrupted = false;
        }


        public void SetAudioClipData(IT_ClipData<IT_ClipPlayerData> clipData)
        {
            AudioClipData = clipData;
        }

        public void SetTransformClipData(IT_ClipData<IT_ClipPlayerData> clipData)
        {
            BodyAnimationClipData = clipData;
        }

        public void SetPropertiesClipData(IT_ClipData<IT_ClipPlayerData> clipData)
        {
            PropertiesClipData = clipData;
        }

        public void SetFacialAnimationData(IT_ClipData<FacialAnimationExportWrapper> clipData)
        {
            FacialAnimationClipData = clipData;
        }

        public void SetInterruptionValue(bool value)
        {
            IsAnimationProcessInterrupted = value;
        }
    }
}