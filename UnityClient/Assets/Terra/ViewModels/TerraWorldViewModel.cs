using System.Collections.Generic;
using PandeaGames;
using PandeaGames.ViewModels;
using Terra.SerializedData.Entities;
using Terra.SerializedData.World;

namespace Terra.ViewModels
{
    public class TerraWorldViewModel : IViewModel
    {
        public delegate void WorldSetDelegate(TerraWorld world);

        public event WorldSetDelegate OnWorldSet;
        
        private TerraWorld _world;

        public void SetWorld(TerraWorld world)
        {
            _world = world;
            OnWorldSet?.Invoke(world);
        }

        public IEnumerable<AssembledEntity> GetEntities()
        {
            if (_world != null)
            {
                foreach (AssembledEntity entity in _world.Entities)
                {
                    yield return entity;
                }
            }
            else
            {
                yield break;
            }
        }
        
        public IEnumerable<RuntimeTerraEntity> GetRuntimeEntities()
        {
            if (_world != null)
            {
                foreach (RuntimeTerraEntity entity in _world)
                {
                    yield return entity;
                }
            }
            else
            {
                yield break;
            }
        } 

        public void Reset()
        {
            
        }
    }
}