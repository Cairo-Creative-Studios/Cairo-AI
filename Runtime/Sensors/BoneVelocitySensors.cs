using AI.Core;
using UnityEngine;

namespace AI.Sensors
{
    public class BoneVelocitySensor : MonoBehaviour, ISensor
    {
        public int inputCount = 64; // Number of bone velocities to track
        public GameObject targetObject; // GameObject containing the bones to track
        public float maxVelocity = 10f; // Maximum velocity for normalization

        private Rigidbody[] boneRigidbodies;

        public int InputCount { get; set; }

        private void Start()
        {
            // Get the rigidbodies of the bones
            boneRigidbodies = targetObject.GetComponentsInChildren<Rigidbody>();
        }

        public float[] GetInputs()
        {
            // Create an array to store the bone velocities
            float[] inputs = new float[inputCount];

            int index = 0;
            for (int i = 0; i < boneRigidbodies.Length; i++)
            {
                Rigidbody boneRigidbody = boneRigidbodies[i];

                // Calculate the velocity magnitude of the bone
                float velocityMagnitude = boneRigidbody.velocity.magnitude;

                // Normalize the velocity magnitude
                float normalizedVelocity = velocityMagnitude / maxVelocity;

                // Clamp the normalized velocity between 0 and 1
                normalizedVelocity = Mathf.Clamp01(normalizedVelocity);

                // Store the normalized velocity in the inputs array
                inputs[index] = normalizedVelocity;
                index++;

                // Check if all bone velocities have been tracked
                if (index >= inputCount)
                    break;
            }

            // Fill remaining inputs with zeros if necessary
            while (index < inputCount)
            {
                inputs[index] = 0f;
                index++;
            }

            return inputs;
        }
    }
}