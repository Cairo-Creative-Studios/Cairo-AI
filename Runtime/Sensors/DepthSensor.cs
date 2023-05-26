using AI.Core;
using UnityEngine;

namespace AI.Sensors
{
    public class DepthSensor : MonoBehaviour, ISensor
    {
        public int inputCount = 64; // Number of depth samples
        public int textureWidth = 64; // Width of the depth texture
        public int textureHeight = 64; // Height of the depth texture
        public float maxDistance = 10f; // Maximum distance for depth measurement

        private Texture2D depthTexture;

        public int InputCount { get; set; }
        private void Start()
        {
            // Create the depth texture
            depthTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RFloat, false);
        }

        public float[] GetInputs()
        {
            // Render the depth texture using the camera
            RenderTexture renderTexture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.RFloat);
            Camera mainCamera = Camera.main;
            mainCamera.targetTexture = renderTexture;
            mainCamera.Render();
            RenderTexture.active = renderTexture;

            // Read the depth values from the texture
            depthTexture.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0);
            depthTexture.Apply();

            // Generate depth samples
            float[] inputs = new float[inputCount];
            int sampleStep = Mathf.FloorToInt(textureWidth / Mathf.Sqrt(inputCount));

            int index = 0;
            for (int y = 0; y < textureHeight; y += sampleStep)
            {
                for (int x = 0; x < textureWidth; x += sampleStep)
                {
                    float depthValue = depthTexture.GetPixel(x, y).r;
                    float distance = depthValue * maxDistance;
                    inputs[index] = distance;
                    index++;
                }
            }

            // Clean up
            RenderTexture.active = null;
            mainCamera.targetTexture = null;
            Destroy(renderTexture);

            return inputs;
        }
    }
}