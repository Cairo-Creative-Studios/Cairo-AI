using AI.Core;
using UnityEngine;

namespace AI.Sensors
{
    public class IdentifyingRayCastSensor : MonoBehaviour, ISensor
    {
        public int inputCount = 8; // Number of raycasts to perform
        public float maxDistance = 10f; // Maximum distance for normalization
        public string targetTag = "Target"; // Tag of the GameObjects to search for

        public int InputCount { get; set; }

        public float[] GetInputs()
        {
            // Create an array to store the distances
            float[] inputs = new float[inputCount];

            // Calculate the angle between each raycast
            float angleIncrement = 360f / inputCount;

            // Perform raycasts in a circle around the sensor's position
            for (int i = 0; i < inputCount; i++)
            {
                // Calculate the direction of the raycast based on the current angle
                Vector3 direction = Quaternion.Euler(0f, angleIncrement * i, 0f) * transform.forward;

                RaycastHit hit;
                if (Physics.Raycast(transform.position, direction, out hit, maxDistance))
                {
                    // Check if the hit object has the target tag
                    if (hit.collider.CompareTag(targetTag))
                    {
                        // Calculate the normalized distance
                        float normalizedDistance = hit.distance / maxDistance;

                        // Clamp the normalized distance between 0 and 1
                        normalizedDistance = Mathf.Clamp01(normalizedDistance);

                        // Store the normalized distance in the inputs array
                        inputs[i] = normalizedDistance;
                    }
                    else
                    {
                        // If the hit object doesn't have the target tag, set the input to 0
                        inputs[i] = 0f;
                    }
                }
                else
                {
                    // If no object is hit, set the input to 0
                    inputs[i] = 0f;
                }
            }

            return inputs;
        }
    }
}
