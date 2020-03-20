using Terra.SerializedData.Entities;
using UnityEngine;

namespace Terra.MonoViews
{
    public class TerraSerializedEntityPositionMonoView : AbstractTerraMonoComponent
    {
        private void Update()
        {
            if (Initialized)
            {
                Entity.Position.Set(transform.position);
            }
        }

        protected override void Initialize(RuntimeTerraEntity entity)
        {
            base.Initialize(entity);
            transform.position = new Vector3(entity.Position.Data.x, entity.Position.Data.y + 0.1f, entity.Position.Data.z);
        }
    }
}