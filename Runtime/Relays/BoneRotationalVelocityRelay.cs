using UnityEngine;
using AI.Core;

namespace AI.Relays
{
    public class BoneRotationalVelocityRelay : MonoBehaviour, IRelay
    {
        public int OutputCount { get; set; }

        private Rigidbody[] boneRigidbodies;
        private float[] previousOutputs;

        private void Awake()
        {
            // Assuming the bones are represented by Rigidbody components attached to the GameObject
            boneRigidbodies = GetComponentsInChildren<Rigidbody>();
            OutputCount = boneRigidbodies.Length * 3; // 3 outputs per bone for rotational velocity in x, y, and z axes

            previousOutputs = new float[OutputCount];
        }

        public void SetOutput(float[] outputs)
        {
            if (outputs.Length != OutputCount)
            {
                Debug.LogError("Invalid number of outputs provided!");
                return;
            }

            int currentIndex = 0;

            // Iterate through the bone rigidbodies and set the rotational velocity based on the outputs
            for (int i = 0; i < boneRigidbodies.Length; i++)
            {
                Rigidbody boneRigidbody = boneRigidbodies[i];

                // Get the rotational velocity values from the outputs
                float angularVelocityX = outputs[currentIndex++];
                float angularVelocityY = outputs[currentIndex++];
                float angularVelocityZ = outputs[currentIndex++];

                // Create a new rotational velocity vector based on the outputs
                Vector3 angularVelocity = new Vector3(angularVelocityX, angularVelocityY, angularVelocityZ);

                // Set the rotational velocity of the bone rigidbody
                boneRigidbody.angularVelocity = angularVelocity;

                // Store the current outputs for future reference
                previousOutputs[i * 3] = angularVelocityX;
                previousOutputs[i * 3 + 1] = angularVelocityY;
                previousOutputs[i * 3 + 2] = angularVelocityZ;
            }
        }
    }
}