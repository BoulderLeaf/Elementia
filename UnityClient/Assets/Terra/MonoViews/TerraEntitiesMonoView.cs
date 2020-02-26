using PandeaGames;
using Terra.SerializedData.Entities;
using Terra.StaticData;
using Terra.ViewModels;
using UnityEngine;

namespace Terra.MonoViews
{
    public class TerraEntitiesMonoView : MonoBehaviour
    {
        private TerraEntityPrefabConfig _terraEntityPrefabConfig;
        private TerraEntitiesViewModel _terraEntitiesViewModel;
        private void Start()
        {
            _terraEntitiesViewModel = Game.Instance.GetViewModel<TerraEntitiesViewModel>(0);
            _terraEntitiesViewModel.OnAddEntity += OnAddEntity;
            _terraEntityPrefabConfig = _terraEntitiesViewModel.TerraEntityPrefabConfig;
        }

        private void OnDestroy()
        {
            _terraEntitiesViewModel.OnAddEntity -= OnAddEntity;
        }

        private void OnAddEntity(RuntimeTerraEntity obj)
        {
            GameObject entity = Instantiate(_terraEntityPrefabConfig.GetGameObject(obj), transform);
            entity.GetComponent<TerraEntityMonoView>().Initilize(obj);
        }
    }
}