using System;
using System.Collections.Generic;

namespace Terra.SerializedData.Entities
{
    public partial class RuntimeTerraEntity : ITerraEntity
    {
        public event Action<RuntimeTerraEntity, string> OnLabelAdded;
        public event Action<RuntimeTerraEntity, string> OnLabelRemoved;
            
        event Action<TerraEntity, string> ITerraEntity.OnLabelRemoved
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        event Action<TerraEntity, string> ITerraEntity.OnLabelAdded
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }
        
        public TerraEntity Entity { private set; get; }
        public TerraEntities Entities { private set; get; }

        public override int GetHashCode()
        {
            return Entity.InstanceId;
        }

        public RuntimeTerraEntity(TerraEntity entity, TerraEntities entities)
        {
            Entity = entity;
            Entities = entities;

            Entity.OnLabelAdded += (labelEntity, label) => OnLabelAdded?.Invoke(this, label);
            Entity.OnLabelRemoved += (labelEntity, label) => OnLabelRemoved?.Invoke(this, label);
        }

        public int InstanceId
        {
            get => Entity.InstanceId;
            set => Entity.InstanceId = value;
        }
        public HashSet<string> Labels
        {
            get => Entity.Labels;
            set => Entity.Labels = value;
        }
        public string Type
        {
            get => Entity.Type;
            set => Entity.Type = value;
        }
    }
}