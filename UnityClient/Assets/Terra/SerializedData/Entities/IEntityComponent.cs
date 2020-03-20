using Terra.Services;

namespace Terra.SerializedData.Entities
{
    public interface IEntityComponent : IDBSerializable
    {
        EntityComponent Type { get; }
    }
}