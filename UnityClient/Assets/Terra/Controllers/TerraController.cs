using PandeaGames;
using PandeaGames.Views;
using PandeaGames.Views.ViewControllers;
using Terra.SerializedData.Entities;
using Terra.Services;
using Terra.Views;

namespace Terra.Controllers
{
    public class TerraController : AbstractViewController
    {
        public TerraController()
        {
            Game.Instance.GetService<TerraDBService>().Setup(
                new IDBSchema[]
                {
                    TerraEntity.Serializer,
                    TerraPosition3D.Serializer,
                    TerraPoint.Serializer
                }
                );
        }

        protected override IView CreateView()
        {
            return new TerraView();
        }
    }
}