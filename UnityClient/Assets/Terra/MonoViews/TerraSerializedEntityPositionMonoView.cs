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
                Entity.SetTerraPosition3D(transform.position);
            }
        }

        protected override void Initialize(RuntimeTerraEntity entity)
        {
            base.Initialize(entity);

            transform.position = entity.GetTerraPosition3D();
        }
    }
}