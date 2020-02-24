using System;
using System.Collections.Generic;

namespace Terra.SerializedData.Entities
{
    public interface ITerraEntity : ITerraEntityComponent
    {
        event Action<TerraEntity, string> OnLabelRemoved;
        event Action<TerraEntity, string> OnLabelAdded;
        
        HashSet<string> Labels { get; set; }
        string Type { get; set; }
    }
}