using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CGPT
{
    public class GeneticPool : MonoBehaviour
    {
        [SerializeField] private ComputeShader neuralNetworkShader;
        [SerializeField] private int populationSize = 10;
        [SerializeField] private int maxGenerations = 100;
        [SerializeField] private float mutationRate = 0.1f;

        private List<Instance> Instances;
        private ComputeBuffer InstanceBuffer;
        private int kernelID;

        private void CreateInstance(ComputeShader computeShader, int inputs, int layers, int outputCount)
        {
            Instance instance = new Instance(computeShader, inputs, layers, 5, outputCount);
            Instances.Add(instance);
        }

        private void OnDestroy()
        {
            // Release the compute buffer
            InstanceBuffer.Release();
        }

        /// <summary>
        /// Iterate the genetic pool
        /// </summary>
        private void Iterate()
        {
            // Get the results from the compute buffer
            Instance[] results = new Instance[populationSize];
            InstanceBuffer.GetData(results);

            // Sort the results by fitness
            results = SortByFitness(results);

            // Create a new population of  instances
            List<Instance> newInstances = new List<Instance>();

            // Add the fittest instances to the new population
            for (int i = 0; i < populationSize / 2; i++)
            {
                newInstances.Add(results[i]);
            }

            // Create new instances by crossing over the fittest instances
            while (newInstances.Count < populationSize)
            {
                int parentIndex1 = Random.Range(0, populationSize / 2);
                int parentIndex2 = Random.Range(0, populationSize / 2);
                Instance parent1 = results[parentIndex1];
                Instance parent2 = results[parentIndex2];
                Instance child = parent1.Crossover(parent2);
                child.Mutate(mutationRate);
                newInstances.Add(child);
            }

            // Update the terrain instances list and buffer
            Instances = newInstances;
            InstanceBuffer.SetData(Instances.ToArray());
        }

        /// <summary>
        /// Sort the instances by fitness
        /// </summary>
        /// <param name="instances"></param>
        /// <returns></returns>
        private Instance[] SortByFitness(Instance[] instances)
        {
            // Sort the instances by fitness
            List<Instance> sorted = new List<Instance>(instances);
            sorted.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));
            return sorted.ToArray();
        }

        [Serializable]
        public class Instance
        {
            public Network Network;
            public ComputeShader NeuralNetwork;
            public ComputeBuffer InputBuffer;
            public ComputeBuffer OutputBuffer;
            //
            public ComputeBuffer WeightsBuffer;
            public ComputeBuffer BiasBuffer;
            public ComputeBuffer LayersBuffer;
            
            //
            int inferenceKernelID;
            int backpropagationKernelID;

            public float Fitness { get; set; }
            public int Size = 16;

            /// <summary>
            /// Creates an original instance of the neural network
            /// </summary>
            /// <param name="neuralNetwork"></param>
            /// <param name="inputs"></param>
            /// <param name="hiddenLayers"></param>
            /// <param name="outputs"></param>
            /// <param name="size"></param>
            public Instance(ComputeShader neuralNetwork, int inputs, int hiddenLayers, int hiddenNeuronLimit, int outputs, int size = 16)
            {
                this.NeuralNetwork = neuralNetwork;
                
                // Initialize the network structure
                Network = new Network();
                Network.input_layer = new Neuron[inputs];
                Network.hidden_layer = new Neuron[hiddenLayers][];
                for (int i = 0; i < Network.hidden_layer.Length; i++)
                {
                    Network.hidden_layer[i] = new Neuron[Random.Range(0, hiddenNeuronLimit)];
                }
                
                Network.output_layer = new Neuron[outputs];
                
                // Initialize the network neurons
                // Input layer
                for (int i = 0; i < inputs; i++)
                {
                    Network.input_layer[i] = new Neuron(hiddenLayers);
                }
                // Hidden layer
                for (int i = 0; i < hiddenLayers; i++)
                {
                    for (int j = 0; j < Network.hidden_layer[i].Length; j++)
                    {
                        Network.hidden_layer[i][j] = new Neuron(outputs);
                    }
                }
                // Output layer
                for (int i = 0; i < outputs; i++)
                {
                    Network.output_layer[i] = new Neuron(0);
                }
                
                // Initialize the compute buffers
                InputBuffer = new ComputeBuffer(inputs, sizeof(float));
                OutputBuffer = new ComputeBuffer(outputs, sizeof(float));
                
                inferenceKernelID = NeuralNetwork.FindKernel("Inference");
                backpropagationKernelID = NeuralNetwork.FindKernel("Backpropagation");
            }
            
            /// <summary>
            /// Creates a new instance of the neural network from an existing instance
            /// </summary>
            /// <param name="instance"></param>
            public Instance(Instance instance)
            {
                this.NeuralNetwork = instance.NeuralNetwork;
                this.Network = instance.Network;
                this.InputBuffer = instance.InputBuffer;
                this.OutputBuffer = instance.OutputBuffer;
                this.WeightsBuffer = instance.WeightsBuffer;
                this.BiasBuffer = instance.BiasBuffer;
                this.LayersBuffer = instance.LayersBuffer;
                this.inferenceKernelID = instance.inferenceKernelID;
                this.backpropagationKernelID = instance.backpropagationKernelID;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="inputs"></param>
            /// <returns></returns>
            public half[] Compute(float[] inputs)
            {
                // Set the input buffer
                InputBuffer.SetData(inputs);
                
                // Set the compute shader buffers
                NeuralNetwork.SetBuffer(inferenceKernelID, "InputBuffer", InputBuffer);
                NeuralNetwork.SetBuffer(inferenceKernelID, "OutputBuffer", OutputBuffer);
                NeuralNetwork.SetBuffer(inferenceKernelID, "WeightsBuffer", WeightsBuffer);
                NeuralNetwork.SetBuffer(inferenceKernelID, "BiasBuffer", BiasBuffer);
                NeuralNetwork.SetBuffer(inferenceKernelID, "LayersBuffer", LayersBuffer);
                
                // Dispatch the compute shader
                NeuralNetwork.Dispatch(inferenceKernelID, 1, 1, 1);
                
                // Get the output buffer
                half[] outputs = new half[OutputBuffer.count];
                OutputBuffer.GetData(outputs);
                
                // Return the output buffer
                return outputs;
            }
            
            public Instance Crossover(Instance parent)
            {
                // Create a new instance
                Instance child = new Instance(this);
                
                // Crossover the weights
                int crossoverPoint = Random.Range(1, this.GetNumWeights() - 1);
                for (int i = 0; i < this.Network.hidden_layer.Length+2; i++)
                {
                    if (i == 0)
                    {
                        for (int j = 0; j < this.Network.input_layer.Length; j++)
                        {
                            if (i < crossoverPoint)
                            {
                                child.Network.input_layer[j].weights[i] = this.Network.input_layer[j].weights[i];
                            }
                            else
                            {
                                child.Network.input_layer[j].weights[i] = this.Network.input_layer[j].weights[i];
                            }
                        }
                    }
                    else if (i == this.Network.hidden_layer.Length+1)
                    {
                        for (int j = 0; j < this.Network.output_layer.Length; j++)
                        {
                            if (i < crossoverPoint)
                            {
                                child.Network.output_layer[j].weights[i] = this.Network.output_layer[j].weights[i];
                            }
                            else
                            {
                                child.Network.output_layer[j].weights[i] = this.Network.output_layer[j].weights[i];
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < this.Network.hidden_layer[i].Length; j++)
                        {
                            if (i < crossoverPoint)
                            {
                                child.Network.hidden_layer[i][j].weights[j] = this.Network.hidden_layer[i][j].weights[j];
                            }
                            else
                            {
                                child.Network.hidden_layer[i][j].weights[j] = this.Network.hidden_layer[i][j].weights[j];
                            }
                        }
                    }
                }

                
                // Set the compute shader buffers
                NeuralNetwork.SetBuffer(backpropagationKernelID, "WeightsBuffer", WeightsBuffer);
                NeuralNetwork.SetBuffer(backpropagationKernelID, "BiasBuffer", BiasBuffer);
                NeuralNetwork.SetBuffer(backpropagationKernelID, "LayersBuffer", LayersBuffer);
                
                // Dispatch the compute shader
                NeuralNetwork.Dispatch(backpropagationKernelID, 1, 1, 1);
                
                // Return the child instance
                return child;
            }

            private int GetNumWeights()
            {
                int numWeights = 0;
                for (int i = 0; i < Network.input_layer.Length; i++)
                {
                    numWeights += Network.input_layer[i].weights.Length;
                }
                for (int i = 0; i < Network.hidden_layer.Length; i++)
                {
                    for (int j = 0; j < Network.hidden_layer[i].Length; j++)
                    {
                        numWeights += Network.hidden_layer[i][j].weights.Length;
                    }
                }
                return numWeights;
            }

            public void Mutate(float mutationRate)
            {
                // Mutate the neural network weights
                NeuralNetwork.SetFloat("MutationRate", mutationRate);
                NeuralNetwork.Dispatch(NeuralNetwork.FindKernel("Mutate"), 1, 1, 1);
            }
            
            
        }
        
        

        // Define the neuron structure
        public struct Neuron {
            public half bias;
            public half[] weights;
            
            public Neuron(int numWeights) {
                bias = (half)0;
                weights = new half[numWeights];
            }
        };
        
        // Define the network structure
        public struct Network {
            public Neuron[] input_layer;
            public Neuron[][] hidden_layer;
            public Neuron[] output_layer;
        };
    }
}