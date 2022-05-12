namespace Retinize.Editor.AnimotiveImporter
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class TransformInfoList
    {
        public List<TransformsByString> TransformsByStrings =
            new List<TransformsByString>();
    }
}