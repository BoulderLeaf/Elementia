using System;
using System.Collections;
using System.Collections.Generic;
using PandeaGames;
using PandeaGames.Data;
using PandeaGames.ViewModels;
using Terra.SerializedData.Entities;
using Terra.SerializedData.GameData;
using Terra.SerializedData.World;
using Terra.StaticData;
using UnityEngine;

namespace Terra.ViewModels
{
    public class TerraEntitiesViewModel : IParamaterizedViewModel<TerraEntitiesViewModel.Parameters>, ITerraEntityViewModel<RuntimeTerraEntity>
    {
        public struct Parameters
        {
            public string[] Labels;
        }
        
        public event Action<RuntimeTerraEntity> OnAddEntity;
        public event Action<RuntimeTerraEntity> OnRemoveEntity;

        private HashSet<RuntimeTerraEntity> _entities { get; } = new HashSet<RuntimeTerraEntity>();
        private Dictionary<string, HashSet<RuntimeTerraEntity>> _filteredEntities { get; } = new Dictionary<string, HashSet<RuntimeTerraEntity>>();
        
        private TerraChunksViewModel _chunksViewModel;
        private TerraWorldViewModel _worldViewModel;

        private List<ITerraEntityDataController> _entityDataControllers { get; } = new List<ITerraEntityDataController>();

        public TerraEntityPrefabConfig TerraEntityPrefabConfig;

        public RuntimeTerraEntity Player { get; private set; }

        public TerraEntitiesViewModel()
        {
            _chunksViewModel = Game.Instance.GetViewModel<TerraChunksViewModel>(0);
            _worldViewModel = Game.Instance.GetViewModel<TerraWorldViewModel>(0);
            
            _worldViewModel.OnWorldSet += WorldViewModelOnOnWorldSet;
            
            _chunksViewModel.OnChunkAdded += ChunksViewModelOnOnChunkAdded;
            _chunksViewModel.OnChunkRemoved += ChunksViewModelOnOnChunkRemoved;
            
           // AddEntities(_chunksViewModel.GetRuntimeEntities());
           // AddEntities(_worldViewModel.GetRuntimeEntities());

            //AddDataController(new RuntimeTerraEntity.TerraPosition3DDataController());
        }

        public void AddDataController(ITerraEntityDataController dataController)
        {
            _entityDataControllers.Add(dataController);
        }

        private void WorldViewModelOnOnWorldSet(TerraWorld world)
        {
            AddEntities(_worldViewModel.GetRuntimeEntities());
        }
        
        private void ChunksViewModelOnOnChunkRemoved(TerraWorldChunk chunk)
        {
            //RemoveEntities(chunk);
        }
        
        private void ChunksViewModelOnOnChunkAdded(TerraWorldChunk chunk)
        {
           // AddEntities(chunk);
        }

        public RuntimeTerraEntity GetEntity(ITerraEntityType type)
        {
            foreach (RuntimeTerraEntity entity in _entities)
            {
                if (entity.EntityTypeData.EntityID == type.EntityID)
                {
                    return entity;
                }
            }

            return null;
        }

        public IEnumerator<RuntimeTerraEntity> GetEntities(string label = "")
        {
            if (string.IsNullOrEmpty(label))
            {
                return _entities.GetEnumerator();
            }
            else
            {
                _filteredEntities.TryGetValue(label, out HashSet<RuntimeTerraEntity> filterSet);
            
                if (filterSet != null)
                {
                    return filterSet.GetEnumerator();
                }
            }

            return null;
        }

        public void AddEntities(IEnumerable<RuntimeTerraEntity> entitiesToAdd)
        {
            foreach (RuntimeTerraEntity entity in entitiesToAdd)
            {
                AddEntity(entity);
            }
        }

        public bool AddEntity(RuntimeTerraEntity entity)
        {
            if (_entities.Contains(entity))
            {
                return false;
            }
            else
            {
                _entities.Add(entity);

                foreach (string label in entity.Labels)
                {
                    EntityOnLabelAdded(entity, label);
                }

                if (entity.EntityID == TerraGameResources.Instance.TerraEntityPrefabConfig.PlayerConfig.Data.EntityID)
                {
                    Player = entity;
                }
                
                entity.OnLabelAdded += EntityOnLabelAdded;
                entity.OnLabelRemoved += EntityOnLabelRemoved;
                OnAddEntity?.Invoke(entity);
            }

            return true;
        }

        public void RemoveEntities(IEnumerable<RuntimeTerraEntity> entities)
        {
            foreach (RuntimeTerraEntity entity in entities)
            {
                RemoveEntity(entity);
            }
        }

        public bool RemoveEntity(RuntimeTerraEntity entity)
        {
            if (!_entities.Contains(entity))
            {
                return false;
            }
            else
            {
                _entities.Remove(entity);
                
                foreach (string label in entity.Labels)
                {
                    EntityOnLabelRemoved(entity, label);
                }
                
                entity.OnLabelAdded -= EntityOnLabelAdded;
                entity.OnLabelRemoved -= EntityOnLabelRemoved;
                OnRemoveEntity?.Invoke(entity);
            }

            return true;
        }

        private void EntityOnLabelRemoved(RuntimeTerraEntity entity, string label)
        {
            _filteredEntities.TryGetValue(label, out HashSet<RuntimeTerraEntity> filterSet);
            
            if (filterSet != null)
            {
                filterSet.Remove(entity);
            }
        }
        
        private void EntityOnLabelAdded(RuntimeTerraEntity entity, string label)
        {
            _filteredEntities.TryGetValue(label, out HashSet<RuntimeTerraEntity> filterSet);
            
            if (filterSet == null)
            {
                filterSet = new HashSet<RuntimeTerraEntity>();
                _filteredEntities.Add(label, filterSet);
            }

            filterSet.Add(entity);
        }


        public void SetParameters(Parameters parameters)
        {
            throw new NotImplementedException();
        }

        void IViewModel.Reset()
        {

        }

        public IEnumerator<RuntimeTerraEntity> GetEnumerator()
        {
            return _entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}