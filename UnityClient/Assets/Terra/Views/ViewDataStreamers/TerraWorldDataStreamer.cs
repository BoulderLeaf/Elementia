using System;
using PandeaGames;
using PandeaGames.Data;
using Terra.SerializedData.Entities;
using Terra.SerializedData.World;
using Terra.Services;
using Terra.ViewModels;
using UnityEngine;

namespace Terra.Views.ViewDataStreamers
{
    public class TerraWorldDataStreamer : IDataStreamer
    {
        private TerraWorldViewModel _terraWorldViewModel;
        private TerraEntitesService _terraEntitiesService;
        private TerraWorldService _terraWorldService;
        private TerraEntitiesViewModel _terraEntitiesViewModel;
        private TerraChunkService _terraChunkService;
        private TerraDBService _db;
        private RuntimeTerraEntity _playerEntity;
        private float _lastSaveSeconds;
        private Vector3 _lastChunkLoadPosition;
        
        public TerraWorldDataStreamer(
            TerraWorldViewModel terraWorldViewModel, 
            TerraWorldService terraWorldService,
                TerraChunkService terraChunkService,
            TerraEntitiesViewModel terraEntitiesViewModel,
            TerraDBService db)
        {
            _terraWorldViewModel = terraWorldViewModel;
            _terraWorldService = terraWorldService;
            _terraChunkService = terraChunkService;
            _terraEntitiesViewModel = terraEntitiesViewModel;
            _db = db;
        }
        
        public void Start()
        {
           // _terraWorldService.LoadWorld(OnWorldLoaded, OnError);
           _terraWorldViewModel.SetWorld(new TerraWorld());

           _terraEntitiesService = Game.Instance.GetService<TerraEntitesService>();
           //_terraEntitiesService.LoadEntitesOfType(TerraGameResources.Instance.TerraEntityPrefabConfig.PlayerConfig.Data, OnPlayerLoaded, null);
           _terraEntitiesService.LoadEntites(OnPlayerLoaded, null);
        }

        public void Update(float time)
        {
            if (time - _lastSaveSeconds > 10)
            {
                _db.Save();
                _lastSaveSeconds = time;
            }

            if (_terraEntitiesViewModel.Player != null)
            {
                if (Vector3.Distance(_terraEntitiesViewModel.Player.Position.Data, _lastChunkLoadPosition) > 10)
                {
                    _db.Save();
                    _lastSaveSeconds = time;
                    _lastChunkLoadPosition = _terraEntitiesViewModel.Player.Position.Data;
                    _terraChunkService.GetChunk(new TerraArea()
                    {
                        height = 100, width = 100, x = (int)_lastChunkLoadPosition.x - 50, y = (int)_lastChunkLoadPosition.z - 50
                    }, chunk =>
                    {
                        OnChunkLoaded(chunk);
                    }, null);
                }
            }
        }

        private void OnPlayerLoaded(RuntimeTerraEntity[] entities)
        {
            for (int i = 0; i < entities.Length; i++)
           {
               if (entities[i].Entity.EntityID == TerraGameResources.Instance.TerraEntityPrefabConfig.PlayerConfig
                       .Data.EntityID)
               {
                   _playerEntity = entities[i];
               }
           }

           if (_playerEntity == null)
            {
                _playerEntity = _terraEntitiesService.CreateEntity(
                    TerraGameResources.Instance.TerraEntityPrefabConfig.PlayerConfig.Data);
                RuntimeTerraEntity[] tmpEntities = new RuntimeTerraEntity[entities.Length+1];
                Array.Copy(entities, tmpEntities, 0);
                tmpEntities[tmpEntities.Length - 1] = _playerEntity;
                entities = tmpEntities;
            }

           _lastChunkLoadPosition = _playerEntity.Position.Data;
            _terraChunkService.GetChunk(new TerraArea()
            {
                height = 100, width = 100, x = (int)_playerEntity.Position.Data.x - 50, y = (int)_playerEntity.Position.Data.z - 50
            }, chunk =>
            {
                OnChunkLoaded(chunk);
                _terraEntitiesViewModel.AddEntities(entities);
            }, null);
        }

        private void OnChunkLoaded(TerraWorldChunk chunk)
        {
            Game.Instance.GetViewModel<TerraChunksViewModel>(0).AddChunk(chunk);
            Game.Instance.GetViewModel<TerraViewModel>(0).SetChunk(chunk);
        }

        public void Stop()
        {
            
        }

        private void OnWorldLoaded(TerraWorld world)
        {
            _terraWorldViewModel.SetWorld(world);
        }

        private void OnError(Exception exception)
        {
            
        }
    }
}