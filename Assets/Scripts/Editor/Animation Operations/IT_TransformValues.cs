using UnityEngine;

namespace Retinize.Editor.AnimotiveImporter
{
    public struct IT_TransformValues
    {
        public Quaternion Rotation { get; private set; }
        public Vector3 Position { get; private set; }

        public IT_TransformValues(Quaternion rotation, Vector3 position)
        {
            Rotation = rotation;
            Position = position;
        }
    }
}