using System;
using Terra.SerializedData.Entities;
using Terra.ViewModels;
using UnityEngine;

namespace Terra.MonoViews
{
    public class TerraEntityMonoView : MonoBehaviour
    {
        public event Action<TerraEntity> OnInitialize;
        private TerraEntitiesViewModel _viewModel;
        
        public TerraEntity Entity { private set; get; }
        
        public void Initilize(TerraEntity entity)
        {
            Entity = entity;
            OnInitialize?.Invoke(entity);
        }
    }
}