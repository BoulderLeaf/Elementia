using System;
using System.Collections;
using System.Collections.Generic;

namespace Terra.SerializedData.Entities
{
    public partial class TerraEntities : IEnumerable<RuntimeTerraEntity>
    {
        protected event Action<RuntimeTerraEntity> OnAddEntity;
        protected event Action<RuntimeTerraEntity> OnRemoveEntity;
        
        public HashSet<TerraEntity> Entities { get; set; } = new HashSet<TerraEntity>();
        
        public void AddEntity(TerraEntity entity)
        {
            Entities.Add(entity);
            OnAddEntity?.Invoke(new RuntimeTerraEntity(entity, this));
        }

        public void RemoveEntity(TerraEntity entity)
        {
            Entities.Remove(entity);
            OnRemoveEntity?.Invoke(new RuntimeTerraEntity(entity, this));
        }
        
        public IEnumerator<RuntimeTerraEntity> GetEnumerator()
        {
            foreach (TerraEntity entity in Entities)
            {
                yield return new RuntimeTerraEntity(entity, this);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected void RegisterData<TDataSet>(HashSet<TDataSet> dataSet)
        {
            
        }
    }
}