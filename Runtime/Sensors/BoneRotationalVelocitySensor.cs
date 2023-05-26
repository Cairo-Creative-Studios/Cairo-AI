using AI.Core;
using UnityEngine;

namespace AI.Sensors
{
    public class BoneRotationalVelocitySensor : MonoBehaviour, ISensor
    {
        public int inputCount = 64; // Number of bone rotational velocities to track
        public GameObject targetObject; // GameObject containing the bones to track
        public float maxAngularVelocity = 10f; // Maximum angular velocity for normalization

        private Rigidbody[] boneRigidbodies;

        public int InputCount { get; set; }

        private void Start()
        {
            // Get the rigidbodies of the bones
            boneRigidbodies = targetObject.GetComponentsInChildren<Rigidbody>();
        }

        public float[] GetInputs()
        {
            // Create an array to store the bone rotational velocities
            float[] inputs = new float[inputCount];

            int index = 0;
            for (int i = 0; i < boneRigidbodies.Length; i++)
            {
                Rigidbody boneRigidbody = boneRigidbodies[i];

                // Calculate the angular velocity magnitude of the bone
                float angularVelocityMagnitude = boneRigidbody.angularVelocity.magnitude;

                // Normalize the angular velocity magnitude
                float normalizedAngularVelocity = angularVelocityMagnitude / maxAngularVelocity;

                // Clamp the normalized angular velocity between 0 and 1
                normalizedAngularVelocity = Mathf.Clamp01(normalizedAngularVelocity);

                // Store the normalized angular velocity in the inputs array
                inputs[index] = normalizedAngularVelocity;
                index++;

                // Check if all bone rotational velocities have been tracked
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