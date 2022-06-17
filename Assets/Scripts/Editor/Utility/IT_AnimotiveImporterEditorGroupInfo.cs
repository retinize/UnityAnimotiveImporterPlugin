namespace Retinize.Editor.AnimotiveImporter
{
    using UnityEngine;

    public class IT_AnimotiveImporterEditorGroupInfo
    {
        public string name { get; }
        public GameObject fbx { get; }

        public IT_AnimotiveImporterEditorGroupInfo(string name, GameObject fbx)
        {
            this.name = name;
            this.fbx = fbx;
        }
    }
}