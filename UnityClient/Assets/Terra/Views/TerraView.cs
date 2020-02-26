using System.IO;
using PandeaGames;
using PandeaGames.Views;
using Terra.MonoViews;
using Terra.SerializedData.Entities;
using Terra.SerializedData.World;
using Terra.Services;
using Terra.StaticData;
using Terra.ViewModels;
using Terra.Views.ViewDataStreamers;
using UnityEngine;

namespace Terra.Views
{
    public class TerraView : AbstractUnityView
    {
        private GameObject _view;
        private Controllers.TerraController _controller;
        private ViewDataStreamerGroup _dataStreamers;
        private TerraWorldViewModel _terraWorldViewModel;
        private TerraWorldService _terraWorldService;
        private TerraEntitiesViewModel _terraEntitiesViewModel;

        public TerraView()
        {
            _terraWorldViewModel = Game.Instance.GetViewModel<TerraWorldViewModel>(0);
            _terraEntitiesViewModel = Game.Instance.GetViewModel<TerraEntitiesViewModel>(0);
            _terraWorldService = Game.Instance.GetService<TerraWorldService>();
            
            _dataStreamers = new ViewDataStreamerGroup(new IDataStreamer[]
            {
                new TerraWorldDataStreamer(_terraWorldViewModel, _terraWorldService) 
            });
            
            _terraWorldViewModel.OnWorldSet += TerraWorldViewModelOnWorldSet;
        }

        private void TerraWorldViewModelOnWorldSet(TerraWorld world)
        {
            _terraEntitiesViewModel.AddEntity(new RuntimeTerraEntity(
                new TerraEntity("Player"), world
            ));
        }

        public override void Show()
        {
            _view = new GameObject("TerraView",
            new []{typeof(TerraEntitiesMonoView)}
            );
            
            TaskProvider.Instance.DelayedAction(() => _dataStreamers.Start());
        }

        public override void Destroy()
        {
            base.Destroy();
            _dataStreamers.Stop();
        }

        public override Transform GetTransform()
        {
            return _view.transform;
        }
    }
}