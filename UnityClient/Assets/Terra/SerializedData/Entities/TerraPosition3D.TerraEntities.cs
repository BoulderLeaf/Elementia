using System.Collections.Generic;
using System.Collections;
using Terra.ViewModels;

namespace Terra.SerializedData.Entities
{
    public partial class TerraEntities
    {
        public Dictionary<int, TerraPosition3D> Positions { get; set; }

        public IEnumerator<TerraPosition3D> GetPositionsEnumerator()
        {
            foreach (KeyValuePair<int, TerraPosition3D> kvp in Positions)
            {
                yield return kvp.Value;
            }
        }
    }
    
    public partial class RuntimeTerraEntity
    {
        public TerraPosition3D GetTerraPosition3D()
        {
            if (Entities.Positions.ContainsKey(Entity.InstanceId))
            {
                return Entities.Positions[Entity.InstanceId];
            }
            
            return new TerraPosition3D();
        }
        
        public void SetTerraPosition3D(TerraPosition3D value)
        {
            if (Entities.Positions.ContainsKey(Entity.InstanceId))
            {
                Entities.Positions[Entity.InstanceId] = value;
            }
            else
            {
                Entities.Positions.Add(Entity.InstanceId, value);
            }
        }
        
        public class TerraPosition3DDataController : ITerraEntityDataController
        {
            public void Remove(RuntimeTerraEntity entity)
            {
                entity.Entities.Positions.Remove(entity.InstanceId);
            }
        }
    }
}