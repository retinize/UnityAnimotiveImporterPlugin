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
        public List<IIT_ICluster> Clusters;

        public IT_TakeData(int takeIndex)
        {
            TakeIndex = takeIndex;
            Clusters = new List<IIT_ICluster>();
        }
    }

    public interface IIT_ICluster
    {
        public IT_ClusterType ClusterType { get; }
        public Dictionary<IT_ClipType, IT_ClipData<IT_ClipPlayerData>> ClipDatas { get; }
        public string EntityName { get; set; }
        public bool IsAnimationProcessInterrupted { get; }

        public int TakeIndex { get; set; }
    }


    public class IT_CameraCluster : IIT_ICluster
    {
        public IT_CameraCluster()
        {
            IsAnimationProcessInterrupted = false;
            ClipDatas = new Dictionary<IT_ClipType, IT_ClipData<IT_ClipPlayerData>>();
        }

        public IT_ClusterType ClusterType => IT_ClusterType.CameraCluster;
        public Dictionary<IT_ClipType, IT_ClipData<IT_ClipPlayerData>> ClipDatas { get; }
        public string EntityName { get; set; }
        public bool IsAnimationProcessInterrupted { get; }
        public int TakeIndex { get; set; }
    }

    /// <summary>
    ///     Class to hold animation, audio and property datas coupled.
    /// </summary>
    public class IT_CharacterCluster : IIT_ICluster
    {
        public IT_ClipData<FacialAnimationExportWrapper> FacialAnimationClipData { get; private set; }


        public int NumberOfCaptureInWhichItWasCaptured = -1;

        public IT_CharacterCluster()
        {
            FacialAnimationClipData = new IT_ClipData<FacialAnimationExportWrapper>();
            IsAnimationProcessInterrupted = false;
            ClipDatas = new Dictionary<IT_ClipType, IT_ClipData<IT_ClipPlayerData>>();
        }

        public IT_ClusterType ClusterType => IT_ClusterType.CharacterCluster;

        public Dictionary<IT_ClipType, IT_ClipData<IT_ClipPlayerData>> ClipDatas { get; }
        public string EntityName { get; set; }

        public int TakeIndex { get; set; }


        public bool IsAnimationProcessInterrupted { get; private set; }


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