namespace Retinize.Editor.AnimotiveImporter
{
    using UnityEngine;

    public class IT_AnimotiveImporterEditorGroupInfo
    {
        public string name { get; }
        public GameObject ObjectInScene { get; }

        public IT_AnimotiveImporterEditorGroupInfo(string name, GameObject objectInScene)
        {
            this.name = name;
            this.ObjectInScene = objectInScene;
        }
    }
}