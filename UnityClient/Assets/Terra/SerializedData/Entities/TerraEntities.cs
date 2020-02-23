using System.Collections;
using System.Collections.Generic;

namespace Terra.SerializedData.Entities
{
    public partial class TerraEntities : IEnumerable<TerraEntity>
    {
        public HashSet<TerraEntity> Entities { get; set; } = new HashSet<TerraEntity>();
        
        public IEnumerator<TerraEntity> GetEnumerator()
        {
            return Entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}