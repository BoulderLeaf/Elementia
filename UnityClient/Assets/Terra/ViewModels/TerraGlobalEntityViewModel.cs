using PandeaGames.ViewModels;
using Terra.Entities;

namespace Terra.ViewModels
{
    public class TerraGlobalEntityViewModel : AbstractTerraEntityViewModel<TerraGlobalEntity>
    {
        private TerraEntitiesViewModel _terraEntitiesViewModel;
        
        public TerraGlobalEntityViewModel(TerraEntitiesViewModel terraEntitiesViewModel)
        {
            _terraEntitiesViewModel = terraEntitiesViewModel;
            
            _terraEntitiesViewModel.OnAddEntity += TerraEntitiesViewModelOnOnAddEntity;
            _terraEntitiesViewModel.OnRemoveEntity += TerraEntitiesViewModelOnOnRemoveEntity;
        }

        private void TerraEntitiesViewModelOnOnAddEntity(TerraEntity obj)
        {
            if (obj is TerraGlobalEntity)
            {
                AddEntity(obj as TerraGlobalEntity);
            }
        }
        
        private void TerraEntitiesViewModelOnOnRemoveEntity(TerraEntity obj)
        {
            if (obj is TerraGlobalEntity)
            {
                RemoveEntity(obj as TerraGlobalEntity);
            }
        }

        public void Reset()
        {
            throw new System.NotImplementedException();
        }
    }
}