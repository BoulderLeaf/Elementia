using System.Collections;
using System.IO;
using PandeaGames;
using PandeaGames.Data;
using PandeaGames.Views;
using Terra.MonoViews;
using Terra.SerializedData.Entities;
using Terra.SerializedData.World;
using Terra.Services;
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
        private TerraChunkService _terraChunkService;
        private TerraEntitesService _terraEntitiesService;
        private TerraDBService _db;
        private TerraEntitiesViewModel _terraEntitiesViewModel;
        private Coroutine _updateCoroutine;

        public TerraView()
        {
            _terraWorldViewModel = Game.Instance.GetViewModel<TerraWorldViewModel>(0);
            _terraEntitiesViewModel = Game.Instance.GetViewModel<TerraEntitiesViewModel>(0);
            _terraWorldService = Game.Instance.GetService<TerraWorldService>();
            _terraChunkService = Game.Instance.GetService<TerraChunkService>();
            _db = Game.Instance.GetService<TerraDBService>();
            
            _dataStreamers = new ViewDataStreamerGroup(new IDataStreamer[]
            {
                new TerraWorldDataStreamer(
                    _terraWorldViewModel, 
                    _terraWorldService,
                    _terraChunkService,
                    _terraEntitiesViewModel,
                    _db
                    ) 
            });
            
            _terraWorldViewModel.OnWorldSet += TerraWorldViewModelOnWorldSet;
            _terraEntitiesViewModel.OnAddEntity+= TerraEntitiesViewModelOnAddEntity;
        }

        private void TerraEntitiesViewModelOnAddEntity(RuntimeTerraEntity obj)
        {
            if (obj.EntityTypeData.EntityID ==
                TerraGameResources.Instance.TerraEntityPrefabConfig.PlayerConfig.Data.EntityID)
            {
                
            }
        }

        private void TerraWorldViewModelOnWorldSet(TerraWorld world)
        {
           /* _terraEntitiesViewModel.AddEntity(new RuntimeTerraEntity(
                new TerraEntity("Player"), world
            ));*/
        }

        private IEnumerator Update()
        {
            while (true)
            {
                yield return null;
                
                if (_dataStreamers != null)
                {
                    _dataStreamers.Update(Time.time);
                }
            }
        }

        public override void Show()
        {
            _view = new GameObject("TerraView",
            new []{typeof(TerraEntitiesMonoView),typeof(TerraTerrainMonoView)}
            );
            
            TaskProvider.Instance.DelayedAction(() => _dataStreamers.Start());
            _updateCoroutine = TaskProvider.Instance.RunTask(Update());
        }

        public override void Destroy()
        {
            base.Destroy();
            _dataStreamers.Stop();
            TaskProvider.Instance.EndTask(_updateCoroutine);
        }

        public override Transform GetTransform()
        {
            return _view.transform;
        }
    }
}