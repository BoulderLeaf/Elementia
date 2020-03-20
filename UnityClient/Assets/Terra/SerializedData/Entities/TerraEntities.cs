using System;
using System.Collections;
using System.Collections.Generic;
using PandeaGames;
using Terra.Services;

namespace Terra.SerializedData.Entities
{
    public partial class TerraEntities : IEnumerable<RuntimeTerraEntity>
    {
        protected event Action<RuntimeTerraEntity> OnAddEntity;
        protected event Action<RuntimeTerraEntity> OnRemoveEntity;
        
        public HashSet<AssembledEntity> Entities { get; set; } = new HashSet<AssembledEntity>();

        public RuntimeTerraEntity AddEntity(AssembledEntity entity)
        {
            RuntimeTerraEntity newRuntimeEntity = new RuntimeTerraEntity(entity, Game.Instance.GetService<TerraDBService>());
            OnAddEntity?.Invoke(newRuntimeEntity);
            return newRuntimeEntity;
        }

        public void RemoveEntity(AssembledEntity entity)
        {
            TerraDBService db = Game.Instance.GetService<TerraDBService>();
            Entities.Remove(entity);
            OnRemoveEntity?.Invoke(new RuntimeTerraEntity(entity, db));
        }
        
        public IEnumerator<RuntimeTerraEntity> GetEnumerator()
        {
            TerraDBService db = Game.Instance.GetService<TerraDBService>();
            foreach (AssembledEntity entity in Entities)
            {
                yield return new RuntimeTerraEntity(entity, db);
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