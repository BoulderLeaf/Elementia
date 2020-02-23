using System.Collections;
using System.Collections.Generic;
using Terra.SerializedData.Entities;

namespace Terra.SerializedData.World
{
    public class TerraWorld : IEnumerable<TerraEntity>
    {
        public HashSet<TerraEntity> Entities { get; set; }
        
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