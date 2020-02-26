using System;
using PandeaGames;
using Terra.SerializedData.World;
using Terra.Services;
using Terra.ViewModels;

namespace Terra.Views.ViewDataStreamers
{
    public class TerraWorldDataStreamer : IDataStreamer
    {
        private TerraWorldViewModel _terraWorldViewModel;
        private TerraWorldService _terraWorldService;
        
        public TerraWorldDataStreamer(TerraWorldViewModel terraWorldViewModel, TerraWorldService terraWorldService)
        {
            _terraWorldViewModel = terraWorldViewModel;
            _terraWorldService = terraWorldService;
        }
        
        public void Start()
        {
            _terraWorldService.LoadWorld(OnWorldLoaded, OnError);
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