namespace AI.Core
{
    /// <summary>
    /// Relays are output communicators for the brain, they can be used to accomplish a variety of tasks
    /// </summary>
    public interface IRelay
    {
        /// <summary>
        /// The number of Outputs that the Relay has
        /// </summary>
        int OutputCount { get; set; }
        /// <summary>
        /// Set the output of the Relay
        /// </summary>
        /// <param name="outputs"></param>
        public void SetOutput(float[] outputs);
    }
}