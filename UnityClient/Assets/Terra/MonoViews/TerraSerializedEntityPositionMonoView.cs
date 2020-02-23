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
                
            }
        }

        protected override void Initialize(TerraEntity entity)
        {
            base.Initialize(entity);
            
            //transform.position = new Vector3();
        }
    }
}