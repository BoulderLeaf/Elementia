namespace Terra.SerializedData.Entities
{
    public struct TerraPosition3D
    {
        public int InstanceId { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public override int GetHashCode()
        {
            return InstanceId;
        }
    }
}