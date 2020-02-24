using UnityEngine;

namespace Terra.SerializedData.Entities
{
    public struct TerraPosition3D
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public static implicit operator TerraPosition3D(Vector3 unityVector)
        {
            return new TerraPosition3D()
            {
                x = unityVector.x, y = unityVector.y, z = unityVector.z
            };
        }
        
        public static implicit operator Vector3(TerraPosition3D unityVector)
        {
            return new Vector3()
            {
                x = unityVector.x, y = unityVector.y, z = unityVector.z
            };
        }
    }
}