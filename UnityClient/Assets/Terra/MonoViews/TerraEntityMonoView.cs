using System;
using Terra.SerializedData.Entities;
using Terra.ViewModels;
using UnityEngine;

namespace Terra.MonoViews
{
    public class TerraEntityMonoView : MonoBehaviour
    {
        public event Action<RuntimeTerraEntity> OnInitialize;
        private TerraEntitiesViewModel _viewModel;
        
        public RuntimeTerraEntity Entity { private set; get; }
        
        public void Initilize(RuntimeTerraEntity entity)
        {
            Entity = entity;
            OnInitialize?.Invoke(entity);
        }
    }
}