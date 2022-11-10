using System.Collections.Generic;
using UnityEngine.Playables;

namespace Retinize.Editor.AnimotiveImporter
{
    /// <summary>
    ///     Class to hold all needed data for creating fully-blown timeline with  plugin.
    /// </summary>
    public class IT_TimelineData
    {
        public string GroupName { get; }
        public List<IIT_ICluster> ClipClustersInTake { get; }
        public PlayableDirector PlayableDirector { get; }
        public Dictionary<string, IT_FbxDatasAndHoldersTuple> FbxDataWithHolders { get; }

        public IT_TakeData TakeData { get; }

        public IT_TimelineData(string groupName, List<IIT_ICluster> clipClustersInTake,
            PlayableDirector playableDirector,
            Dictionary<string, IT_FbxDatasAndHoldersTuple> fbxDataWithHolders, IT_TakeData takeData)
        {
            GroupName = groupName;
            ClipClustersInTake = clipClustersInTake;
            PlayableDirector = playableDirector;
            FbxDataWithHolders = fbxDataWithHolders;
            TakeData = takeData;
        }
    }
}