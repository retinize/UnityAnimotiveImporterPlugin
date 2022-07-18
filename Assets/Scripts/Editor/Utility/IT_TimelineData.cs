using System.Collections.Generic;
using Retinize.Editor.AnimotiveImporter;
using UnityEngine.Playables;

public class IT_TimelineData
{
    public string GroupName { get; }
    public List<IT_ClipData> ClipDatasInTake { get; }
    public PlayableDirector PlayableDirector { get; }
    public Dictionary<string, IT_FbxDatasAndHoldersTuple> FbxDataWithHolders { get; }

    public IT_TimelineData(string groupName, List<IT_ClipData> clipDatasInTake, PlayableDirector playableDirector,
        Dictionary<string, IT_FbxDatasAndHoldersTuple> fbxDataWithHolders)
    {
        GroupName = groupName;
        ClipDatasInTake = clipDatasInTake;
        PlayableDirector = playableDirector;
        FbxDataWithHolders = fbxDataWithHolders;
    }
}