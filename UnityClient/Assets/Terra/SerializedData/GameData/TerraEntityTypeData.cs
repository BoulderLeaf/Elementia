using System;
using Terra.SerializedData.Entities;
using UnityEngine;

namespace Terra.SerializedData.GameData
{
    [Serializable]
    public class TerraEntityTypeData : ITerraEntityType
    {
        [SerializeField]
        private EntityComponent component;
        public EntityComponent Component => component;
        
        [SerializeField]
        private string _entityId;
        public string EntityID => _entityId;
        
    }
}