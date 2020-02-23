using System;
using System.Collections;
using System.Collections.Generic;
using ICSharpCode.NRefactory.PrettyPrinter;
using PandeaGames;
using PandeaGames.ViewModels;
using Terra.SerializedData.Entities;
using Terra.SerializedData.World;
using UnityEngine;

namespace Terra.ViewModels
{
    public class TerraEntitiesViewModel : IParamaterizedViewModel<TerraEntitiesViewModel.Parameters>, ITerraEntityViewModel<TerraEntity>
    {
        public struct Parameters
        {
            public string[] Labels;
        }
        
        public event Action<TerraEntity> OnAddEntity;
        public event Action<TerraEntity> OnRemoveEntity;

        private HashSet<TerraEntity> _entities { get; } = new HashSet<TerraEntity>();
        private Dictionary<string, HashSet<TerraEntity>> _filteredEntities { get; } = new Dictionary<string, HashSet<TerraEntity>>();
        
        private TerraChunksViewModel _chunksViewModel;
        private TerraWorldViewModel _worldViewModel;

        public TerraEntitiesViewModel()
        {
            _chunksViewModel = Game.Instance.GetViewModel<TerraChunksViewModel>(0);
            _worldViewModel = Game.Instance.GetViewModel<TerraWorldViewModel>(0);
            
            _worldViewModel.OnWorldSet += WorldViewModelOnOnWorldSet;
            
            _chunksViewModel.OnChunkAdded += ChunksViewModelOnOnChunkAdded;
            _chunksViewModel.OnChunkRemoved += ChunksViewModelOnOnChunkRemoved;
            
            AddEntities(_chunksViewModel.GetEntities());
            AddEntities(_worldViewModel.GetEntities());
        }

        private void WorldViewModelOnOnWorldSet(TerraWorld world)
        {
            throw new NotImplementedException();
        }
        
        private void ChunksViewModelOnOnChunkRemoved(TerraVector position, TerraWorldChunk chunk)
        {
            RemoveEntities(chunk.Entities);
        }
        
        private void ChunksViewModelOnOnChunkAdded(TerraVector position, TerraWorldChunk chunk)
        {
            AddEntities(chunk.Entities);
        }

        public IEnumerator<TerraEntity> GetEntities(string label = "")
        {
            if (string.IsNullOrEmpty(label))
            {
                return _entities.GetEnumerator();
            }
            else
            {
                _filteredEntities.TryGetValue(label, out HashSet<TerraEntity> filterSet);
            
                if (filterSet != null)
                {
                    return filterSet.GetEnumerator();
                }
            }

            return null;
        }

        public void AddEntities(IEnumerable<TerraEntity> entities)
        {
            foreach (TerraEntity entity in entities)
            {
                AddEntity(entity);
            }
        }

        public bool AddEntity(TerraEntity entity)
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
                
                entity.OnLabelAdded += EntityOnLabelAdded;
                entity.OnLabelRemoved += EntityOnLabelRemoved;
                OnAddEntity?.Invoke(entity);
            }

            return true;
        }

        public void RemoveEntities(IEnumerable<TerraEntity> entities)
        {
            foreach (TerraEntity entity in entities)
            {
                RemoveEntity(entity);
            }
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

        private void EntityOnLabelRemoved(TerraEntity entity, string label)
        {
            _filteredEntities.TryGetValue(label, out HashSet<TerraEntity> filterSet);
            
            if (filterSet != null)
            {
                filterSet.Remove(entity);
            }
        }
        
        private void EntityOnLabelAdded(TerraEntity entity, string label)
        {
            _filteredEntities.TryGetValue(label, out HashSet<TerraEntity> filterSet);
            
            if (filterSet == null)
            {
                filterSet = new HashSet<TerraEntity>();
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

        public IEnumerator<TerraEntity> GetEnumerator()
        {
            return _entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}