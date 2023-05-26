using System;
using System.Collections.Generic;
using UDT.Core;
using UDT.Core.Controllables;
using UnityEngine.InputSystem;

namespace AI.Core
{
    public class Brain : StandardComponent<BrainData>
    {
    }
    
    /// <summary>
    /// The brain of the AI, it uses it's <see cref="ISensor"/>s to determine the input into the Neural Network,
    /// and then uses the output of the Neural Network to control the <see cref="IRelay"/>s.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Brain<T> : Brain where T : BrainData
    {
        public new T Data => (T) base.Data;
        
        /// <summary>
        /// The <see cref="ISensor"/>s that the brain uses to determine it's Input
        /// </summary>
        public List<ISensor> Sensors = new List<ISensor>();
        /// <summary>
        /// The <see cref="IRelay"/>s that the brain controls
        /// </summary>
        public List<IRelay> Relays = new List<IRelay>();

        void Update()
        {
            
        }
        
        public override void OnInputAction(InputAction.CallbackContext context)
        {
            
        }

        public override void OnInputAction(SerializedInput input)
        {
            
        }

        //TODO: It might be beneficial to parallelize this somehow
        /// <summary>
        /// First runs through the Sensors of the Brain to determine the Inputs,
        /// Then Calls the Inference Function,
        /// And Finally applies the gathered Outputs from the Inference Function to the Signal Relays
        /// </summary>
        public void UpdateBrain()
        {
            List<float> inputs = new List<float>();
            
            // Compute Sensors
            foreach (var sensor in Sensors)
            {
                inputs.AddRange(sensor.GetInputs());
            }

            // Infer the Outputs
            var outputs = Inference(inputs.ToArray());

            // Compute Relays
            var startIndex = 0;
            var endIndex = 0;
            foreach (var relay in Relays)
            {
                //The End Index is the Start Index from the last iteration + the Relay's Output Count
                endIndex = startIndex + relay.OutputCount;

                //Create the List of Outputs to apply to this Relay
                List<float> appliedOutput = new List<float>();
                for (int i = startIndex; i < endIndex; i++)
                {
                    appliedOutput.Add(outputs[i]);
                }
                
                //Set the Outputs of the Relay
                relay.SetOutput(appliedOutput.ToArray());
                
                //Add the Relay's Output Count to the Start Index, to apply the appropriate Outputs to the correct Relay
                startIndex += relay.OutputCount;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract float[] Inference(float[] inputs);
    }
}