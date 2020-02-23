using PandeaGames;
using PandeaGames.ViewModels;
using Terra.SerializedData.World;

namespace Terra.ViewModels
{
    public class TerraViewModel : IViewModel
    {
        private TerraWorldViewModel _worldViewModel;
        
        public TerraViewModel()
        {
            _worldViewModel = Game.Instance.GetViewModel<TerraWorldViewModel>(0);
        }
        
        public void Reset()
        {
            
        }
    }
}