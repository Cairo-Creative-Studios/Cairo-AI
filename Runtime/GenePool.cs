using System.Collections.Generic;
using UDT.Core;

namespace AI.Core
{
    /// <summary>
    /// The base class for all types of Gene Pools
    /// </summary>
    public class GenePool : StandardComponent<GenePoolData>
    {
        public List<Brain> brains = new List<Brain>();

        public virtual void UpdatePool()
        {
            
        }
    }
}