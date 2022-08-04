using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    /// <summary>
    ///     Simple class to tuple values
    /// </summary>
    public struct IT_TransformValues
    {
        public Quaternion Rotation { get; }
        public Vector3 Position { get; }

        public IT_TransformValues(Quaternion rotation, Vector3 position)
        {
            Rotation = rotation;
            Position = position;
        }
    }
}