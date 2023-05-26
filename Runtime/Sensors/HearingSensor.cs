using AI.Core;
using UDT.Audio;
using UnityEngine;

namespace AI.Sensors
{
    public class HearingSensor : MonoBehaviour, ISensor
    {
        public float maxDistance = 10f; // Maximum distance for sound detection
        public float angleRange = 180f; // Range of angles for sound detection
        public string[] tagList; // List of tags for mapping

        private int inputCount; // Number of inputs based on tag, distance, and angle

        public int InputCount { get; set; }

        private void Start()
        {
            // Calculate the input count based on the tag list size
            inputCount = tagList.Length * 3;
        }

        public float[] GetInputs()
        {
            // Create an array to store the inputs
            float[] inputs = new float[inputCount];

            // Get the recently played sounds from the audio module
            SoundInfo[] recentSounds = AudioModule.GetRecentSounds();

            // Loop through the recent sounds and generate inputs
            for (int i = 0; i < recentSounds.Length; i++)
            {
                SoundInfo sound = recentSounds[i];

                // Calculate the direction and distance to the sound
                Vector3 soundDirection = sound.Position - transform.position;
                float soundDistance = soundDirection.magnitude;

                // Check if the sound is within the maximum distance and angle range
                if (soundDistance <= maxDistance && Vector3.Angle(transform.forward, soundDirection) <= angleRange)
                {
                    // Find the tag index in the tag list
                    int tagIndex = -1;
                    for (int j = 0; j < tagList.Length; j++)
                    {
                        if (tagList[j] == sound.Tag)
                        {
                            tagIndex = j;
                            break;
                        }
                    }

                    if (tagIndex != -1)
                    {
                        // Calculate the normalized tag value
                        float normalizedTag = (float)tagIndex / (tagList.Length - 1);

                        // Calculate the normalized distance
                        float normalizedDistance = soundDistance / maxDistance;

                        // Calculate the normalized angle
                        float normalizedAngle = Vector3.Angle(transform.forward, soundDirection) / angleRange;

                        // Calculate the input index for this sound
                        int inputIndex = tagIndex * 3;

                        // Store the inputs in the array
                        inputs[inputIndex] = normalizedTag;
                        inputs[inputIndex + 1] = normalizedDistance;
                        inputs[inputIndex + 2] = normalizedAngle;
                    }
                }
            }

            return inputs;
        }
    }
}