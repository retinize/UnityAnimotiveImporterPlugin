using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    public class IT_AnimotiveImporterEditorGroupInfo
    {
        public string name { get; }
        public GameObject ObjectInScene { get; }

        public IT_AnimotiveImporterEditorGroupInfo(string name, GameObject objectInScene)
        {
            this.name = name;
            ObjectInScene = objectInScene;
        }
    }
}