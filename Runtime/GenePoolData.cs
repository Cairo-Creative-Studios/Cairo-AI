using System.Collections.Generic;
using UDT.Core;

namespace AI.Core
{
    public class GenePoolData : ComponentData<GenePool>
    {
        /// <summary>
        /// The class to use to manage the Gene Pool
        /// </summary>
        [MonoScript] public string GenePoolClass = "";
        /// <summary>
        /// Whether to Time Out Gene Pool Chromosomes
        /// </summary>
        public bool Timeout = false;
        /// <summary>
        /// The Kill Switch Event of the Instantiated Gene Pool specimens
        /// </summary>
        public StandardEvent KillSwitch;

    }
}