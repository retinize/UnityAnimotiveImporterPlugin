using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    public struct IT_AnimotiveImporterEditorGroupMemberInfo
    {
        public string BindedGroupName { get; }
        public int serializedId { get; }
        public string BodyAnimationName { get; }
        public GameObject ObjectInScene { get; }

        public string BodyAnimationPath { get; }

        public IT_ClipData ClipData { get; }

        public IT_AnimotiveImporterEditorGroupMemberInfo(string bindedGroupName, int serializedId,
            string bodyAnimationName,
            GameObject objectInScene, string bodyAnimationPath, IT_ClipData clipData)
        {
            BindedGroupName = bindedGroupName;
            this.serializedId = serializedId;
            BodyAnimationName = bodyAnimationName;
            ObjectInScene = objectInScene;
            BodyAnimationPath = bodyAnimationPath;
            ClipData = clipData;
        }
    }
}