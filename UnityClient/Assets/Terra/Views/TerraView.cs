using System.IO;
using PandeaGames.Views;
using Terra.StaticData;
using UnityEngine;

namespace Terra.Views
{
    public class TerraView : AbstractUnityView
    {
        private GameObject _view;
        
        public override void Show()
        {
            _view = new GameObject("TerraView");
        }

        public override Transform GetTransform()
        {
            return _view.transform;
        }
    }
}