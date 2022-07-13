using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    public class IT_AnimotiveImporterEditorGroupMemberInfo
    {
        public string BindedGroupName { get; }
        public int serializedId { get; }
        public string name { get; }
        public GameObject ObjectInScene { get; }

        public string BodyAnimationPath { get; }


        public IT_AnimotiveImporterEditorGroupMemberInfo(string bindedGroupName, int serializedId, string name,
            GameObject objectInScene, string bodyAnimationPath)
        {
            BindedGroupName = bindedGroupName;
            this.serializedId = serializedId;
            this.name = name;
            ObjectInScene = objectInScene;
            BodyAnimationPath = bodyAnimationPath;
        }
    }
}