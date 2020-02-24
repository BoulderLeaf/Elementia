using Terra.SerializedData.Entities;
using UnityEngine;

namespace Terra.MonoViews
{
    public class AbstractTerraMonoComponent : MonoBehaviour
    {
        [SerializeField] 
        protected TerraEntityMonoView _entityMonoView;

        protected bool Initialized { private set; get; }
        protected RuntimeTerraEntity Entity
        {
            get { return _entityMonoView.Entity; }
        }

        private void Start()
        {
            if (!Initialized && _entityMonoView.Entity != null)
            {
                Initialize(Entity);
            }
            else
            {
                _entityMonoView.OnInitialize += Initialize;
            }
        }

        protected virtual void Initialize(RuntimeTerraEntity Entity)
        {
            Initialized = true;
            _entityMonoView.OnInitialize -= Initialize;
        }

        private void OnDestroy()
        {
            _entityMonoView.OnInitialize -= Initialize;
        }
    }
}