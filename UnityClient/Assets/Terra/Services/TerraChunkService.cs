using System;
using PandeaGames;
using PandeaGames.Services;
using Terra.SerializedData.World;
using Terra.ViewModels;

namespace Terra.Services
{
    public class TerraChunkService : AbstractService<TerraChunkService>
    {
        public delegate void TerraWorldChunkDelegate(TerraWorldChunk chunk);
        public delegate void TerraWorldChunkErrorDelegate(Exception exception);

        private TerraChunksViewModel _viewModel;
        
        public TerraChunkService()
        {
            _viewModel = Game.Instance.GetViewModel<TerraChunksViewModel>(0);
        }
        
        public void GetChunk(
            TerraVector vector,
            TerraWorldChunkDelegate onComplete,
            TerraWorldChunkErrorDelegate onError)
        {
            if (_viewModel[vector] != null)
            {
                onComplete(_viewModel[vector]);
            }
            else
            {
                //TODO: ASYNC get chunk
            }
        }

        public void Save(Action onComplete, TerraWorldChunkErrorDelegate onError)
        {
            //TODO: save everything in _viewModel
        }
    }
}