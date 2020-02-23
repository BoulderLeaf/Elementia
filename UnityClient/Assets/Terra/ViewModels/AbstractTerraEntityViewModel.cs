using System;
using System.Collections.Generic;
using PandeaGames.ViewModels;
using Terra.SerializedData.Entities;

namespace Terra.ViewModels
{
    public abstract class AbstractTerraEntityViewModel<TTerraEntity>:IViewModel where TTerraEntity : TerraEntity
    {
        public event Action<TerraEntity> OnAddEntity;
        public event Action<TerraEntity> OnRemoveEntity;

        private HashSet<TerraEntity> _entities { get; } = new HashSet<TerraEntity>();

        public bool AddEntity(TerraEntity entity)
        {
            if (_entities.Contains(entity))
            {
                return false;
            }
            else
            {
                _entities.Add(entity);
                OnAddEntity?.Invoke(entity);
            }

            return true;
        }

        public bool RemoveEntity(TerraEntity entity)
        {
            if (!_entities.Contains(entity))
            {
                return false;
            }
            else
            {
                _entities.Remove(entity);
                OnRemoveEntity?.Invoke(entity);
            }

            return true;
        }

        void IViewModel.Reset()
        {

        }
    }
}