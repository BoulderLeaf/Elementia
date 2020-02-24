using PandeaGames.Views;
using PandeaGames.Views.ViewControllers;
using Terra.Views;

namespace Terra.Controllers
{
    public class TerraController : AbstractViewController
    {
        public TerraController()
        {
            
        }

        protected override IView CreateView()
        {
            return new TerraView();
        }
    }
}