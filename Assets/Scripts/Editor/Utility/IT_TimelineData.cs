using System.Collections.Generic;
using Retinize.Editor.AnimotiveImporter;
using UnityEngine.Playables;

public class IT_TimelineData
{
    public string GroupName { get; }
    public List<IT_ClipCluster> ClipClustersInTake { get; }
    public PlayableDirector PlayableDirector { get; }
    public Dictionary<string, IT_FbxDatasAndHoldersTuple> FbxDataWithHolders { get; }

    public IT_TakeData TakeData { get; }

    public IT_TimelineData(string groupName, List<IT_ClipCluster> clipClustersInTake, PlayableDirector playableDirector,
        Dictionary<string, IT_FbxDatasAndHoldersTuple> fbxDataWithHolders, IT_TakeData takeData)
    {
        GroupName = groupName;
        ClipClustersInTake = clipClustersInTake;
        PlayableDirector = playableDirector;
        FbxDataWithHolders = fbxDataWithHolders;
        TakeData = takeData;
    }
}