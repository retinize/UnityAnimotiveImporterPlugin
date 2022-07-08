using System;
using System.Collections.Generic;

namespace Retinize.Editor.AnimotiveImporter
{
    /// <summary>
    ///     The point of this class is solely based on serializing list of 'IT_TransformsByString' class to a json file
    /// </summary>
    [Serializable]
    public class IT_TransformInfoList
    {
        public List<IT_TransformsByString> TransformsByStrings =
            new List<IT_TransformsByString>();
    }
}