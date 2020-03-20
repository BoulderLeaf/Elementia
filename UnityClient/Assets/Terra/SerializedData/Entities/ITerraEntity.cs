using System;
using System.Collections.Generic;
using Terra.SerializedData.GameData;

namespace Terra.SerializedData.Entities
{
    public interface ITerraEntity : ITerraEntityComponent, ITerraEntityType
    {
        event Action<TerraEntity, string> OnLabelRemoved;
        event Action<TerraEntity, string> OnLabelAdded;
        
        HashSet<string> Labels { get; set; }
        string EntityID { get; set; }
        int InstanceId { get; set; }
    }
}