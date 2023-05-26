
namespace AI.Core
{
    public interface ISensor
    {
        /// <summary>
        /// The number of inputs that the sensor provides
        /// </summary>
        public int InputCount { get; set;  }
        /// <summary>
        /// Get the inputs from the sensor
        /// </summary>
        /// <returns></returns>
        public float[] GetInputs();
    }
}