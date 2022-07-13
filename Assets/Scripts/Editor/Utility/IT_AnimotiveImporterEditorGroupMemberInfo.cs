using System.Collections.Generic;
using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    public class IT_AnimotiveImporterEditorGroupMemberInfo
    {
        public int serializedId { get; }
        public string name { get; }
        public GameObject ObjectInScene { get; }

        public string BodyAnimationPath { get; }

        public IT_AnimotiveImporterEditorGroupMemberInfo(int serializedId, string name, GameObject objectInScene,
            string bodyAnimationPath)
        {
            this.serializedId = serializedId;
            this.name = name;
            ObjectInScene = objectInScene;
            BodyAnimationPath = bodyAnimationPath;
        }
    }


    public class IT_Groupdata
    {
        public List<IT_ClipData> ClipDatas;
        public int serializedId { get; }

        public IT_Groupdata(int serializedId)
        {
            this.serializedId = serializedId;
            ClipDatas = new List<IT_ClipData>();
        }
    }
}