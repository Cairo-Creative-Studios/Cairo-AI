
namespace AI.Core
{
    public abstract class ISensor
    {
        /// <summary>
        /// The number of inputs that the sensor provides
        /// </summary>
        public int InputCount { get; set;  }
        /// <summary>
        /// Get the inputs from the sensor
        /// </summary>
        /// <returns></returns>
        public abstract byte[] GetInputs();
    }
}