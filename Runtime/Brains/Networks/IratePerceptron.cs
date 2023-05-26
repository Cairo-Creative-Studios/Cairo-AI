using System;
using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

namespace CairoAI.Layers
{
    /// <summary>
    /// A Multi-Layer Perceptron that uses the Mad Elu Swish Activation Function. This Perceptron is specially designed for use within
    /// Bubble Brains
    /// </summary>
    [Serializable]
    public class IratePerceptron
    {
        [Serializable]
        public struct Network
        {
            public Vector4[] nodes;
            public Vector4[] biases;
            public Vector4[] weights;

            //TODO: Add condition to check if layer index is 0, and if it is, continue
            public uint[] layers;
            public uint layerCount;
            
            public float learning_rate;
            public float error;

            public Vector4[] input;
            public Vector4[] output;
        }

        /// <summary>
        /// The network structure of the Multi-Layer Perceptron
        /// </summary>
        public Network network;

        // Buffers
        ComputeBuffer _networkBuffer;
        ComputeShader _computeShader;
        private int feedForwardKernelIndex;
        private int backpropKernelIndex;

        private int _totalSize;

        /// <summary>
        /// Creates a new Multi-Layer Perceptron
        /// </summary>
        /// <param name="networkStructure"></param>
        /// <param name="activationFunction"></param>
        /// <param name="learningRate"></param>
        /// <param name="multilayerPerceptron"></param>
        public IratePerceptron(int[] networkStructure, float learningRate)
        {
            network = new Network();
            network.layerCount = (uint)networkStructure.Length;
            network.learning_rate = learningRate;
            network.error = 0.0f;

            var layerStart = 0;

            List<int> layerStructure = new List<int>();

            for (int i = 0; i < networkStructure.Length; i++)
            {
                if (i == 0)
                    network.input = new Vector4[networkStructure[i]];
                if (i == networkStructure.Length - 1)
                {
                    network.output = new Vector4[networkStructure[i]];
                }

                layerStructure.Add(layerStart);
                layerStart += networkStructure[i];

                _totalSize += networkStructure[i];
            }

            network.nodes = new Vector4[_totalSize];
            network.biases = new Vector4[_totalSize];
            network.weights = new Vector4[_totalSize];

            network.layers = new uint[layerStructure.Count];

            this._computeShader = Resources.Load<ComputeShader>("IratePerceptron");

            feedForwardKernelIndex = _computeShader.FindKernel("infer");
        }

        /// <summary>
        /// Updates the network and returns the output
        /// </summary>
        /// <returns></returns>
        public float[] Infer(float[] input)
        {
            //Set the Input
            for (int i = 0; i < input.Length / 4; i++)
                network.input[i] = new Vector4(input[i * 4], input[i * 4 + 1], input[i * 4 + 2], input[i * 4 + 3]);

            _networkBuffer = new ComputeBuffer(1, sizeof(float) * 4 * _totalSize);
            _computeShader.SetBuffer(0, "network", _networkBuffer);

            int feedForwardKernel = _computeShader.FindKernel("feedforward");
            int backPropagateKernel = _computeShader.FindKernel("backpopagate");

            _computeShader.Dispatch(feedForwardKernel, 4, 4, 1);
            
            var output = new List<float>();
            for (int i = 0; i < network.output.Length; i++)
            {
                output.Add(network.output[i].x);
                output.Add(network.output[i].y);
                output.Add(network.output[i].z);
                output.Add(network.output[i].w);
            }

            return output.ToArray();
        }
        
        /// <summary>
        /// Backpropagates the error through the network
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public float[] backpropagate()
        {
            _networkBuffer = new ComputeBuffer(1, sizeof(float) * 4 * _totalSize);
            _computeShader.SetBuffer(0, "network", _networkBuffer);

            _computeShader.Dispatch(feedForwardKernelIndex, 4, 4, 1);
            _computeShader.Dispatch(backpropKernelIndex, 4, 4, 1);

            var output = new List<float>();
            for (int i = 0; i < network.output.Length; i++)
            {
                output.Add(network.output[i].x);
                output.Add(network.output[i].y);
                output.Add(network.output[i].z);
                output.Add(network.output[i].w);
            }

            return output.ToArray();
        }
    }
}