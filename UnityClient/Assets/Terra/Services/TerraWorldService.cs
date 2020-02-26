using System;
using PandeaGames.Services;
using Terra.SerializedData.World;

namespace Terra.Services
{
    public class TerraWorldService : IService
    {
        public void LoadWorld(Action<TerraWorld> onComplete, Action<Exception> onError)
        {
            onComplete(new TerraWorld());
            //TODO: Load Terra World ASYNC
        }
    }
}