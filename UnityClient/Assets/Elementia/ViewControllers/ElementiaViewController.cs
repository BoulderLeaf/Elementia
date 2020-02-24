using PandeaGames.Views.ViewControllers;
using TerraController = Terra.Controllers.TerraController;

namespace ViewControllers
{
    public enum ElementiaViewControllerStates
    {
        Preloading, 
        Terra
    }
    
    public class ElementiaViewController : AbstractViewControllerFsm<ElementiaViewControllerStates>
    {
        private class TerraState : AbstractViewControllerState<ElementiaViewControllerStates>
        {
            protected override IViewController GetViewController()
            {
                return new Terra.Controllers.TerraController();
            }
        }
        
        public ElementiaViewController()
        {
            SetViewStateController<TerraState>(ElementiaViewControllerStates.Terra);
            SetInitialState(ElementiaViewControllerStates.Terra);
        }
    }
}