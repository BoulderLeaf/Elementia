using System.Collections.Generic;
using System.Collections;

namespace Terra.SerializedData.Entities
{
    public partial class TerraEntities
    {
        public HashSet<TerraPosition3D> Positions { get; set; } = new HashSet<TerraPosition3D>();
        
        public IEnumerator<TerraPosition3D> GetPositionsEnumerator()
        {
            return Positions.GetEnumerator();
        }
    }
}