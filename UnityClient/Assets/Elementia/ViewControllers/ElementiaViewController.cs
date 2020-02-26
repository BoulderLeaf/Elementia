using PandeaGames;
using PandeaGames.Data;
using PandeaGames.Views.ViewControllers;
using Terra.ViewModels;
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
            public override void EnterState(ElementiaViewControllerStates @from)
            {
                base.EnterState(@from);
                Game.Instance.GetViewModel<TerraEntitiesViewModel>(0).TerraEntityPrefabConfig =
                    ElementiaGameResources.Instance.TerraEntityPrefabConfig;
            }

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