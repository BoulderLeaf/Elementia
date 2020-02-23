using System;
using System.Collections;
using System.Collections.Generic;
using ICSharpCode.NRefactory.PrettyPrinter;
using PandeaGames.ViewModels;
using Terra.Entities;
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

        public TerraEntitiesViewModel()
        {
            
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