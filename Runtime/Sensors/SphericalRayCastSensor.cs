using System;
using UnityEngine;

namespace AI.Core
{
    [Serializable]
    public class SphericalRayCastSensor : ISensor
    {
        public int InputCount { get; set; }
        public float MaxDistance { get; set; }
        public int CastCount { get; set; }

        public SphericalRayCastSensor(int inputCount, float maxDistance, int castCount)
        {
            InputCount = inputCount;
            MaxDistance = maxDistance;
            CastCount = castCount;
        }

        public float[] GetInputs()
        {
            float[] inputs = new float[InputCount];
            float verticalAngleIncrement = 180f / (CastCount + 1);
            float horizontalAngleIncrement = 360f / InputCount;

            Vector3 origin = Vector3.zero;

            for (int i = 0; i < CastCount; i++)
            {
                float verticalAngle = (i + 1) * verticalAngleIncrement;
                float radius = Mathf.Sin(Mathf.Deg2Rad * verticalAngle) * MaxDistance;
                float circleHeight = Mathf.Cos(Mathf.Deg2Rad * verticalAngle) * MaxDistance;

                for (int j = 0; j < InputCount; j++)
                {
                    float horizontalAngle = j * horizontalAngleIncrement;
                    Vector3 direction = Quaternion.Euler(verticalAngle, horizontalAngle, 0f) * Vector3.forward;

                    RaycastHit hit;
                    if (Physics.Raycast(origin + direction * circleHeight, direction, out hit, radius))
                    {
                        // Normalize the distance between 0 and 1
                        float normalizedDistance = hit.distance / MaxDistance;
                        inputs[j] = normalizedDistance;
                    }
                    else
                    {
                        inputs[j] = 0f;
                    }
                }
            }

            return inputs;
        }
    }
}