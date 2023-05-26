using System;
using System.Collections.Generic;
using AI.Core;
using CairoAI.Layers;
using UnityEngine;

namespace AI.Brains
{
    public class ArtificialHippocampus
    {
        public HyperdimensionalManifold Manifold;

        /// <summary>
        /// The Neurons of the Brain, their indexes corresponding to the index of the Points in the Manifold
        /// </summary>
        private List<DeepBoltzmannMachine> _neurons;

        /// <summary>
        /// The Network that constructs the Path that is followed through the Manifold
        /// </summary>
        public IratePerceptron PathContruct;

        /// <summary>
        /// The points within the Point 
        /// </summary>
        private List<DeepBoltzmannMachine> _currentCloudPoints;

        /// <summary>
        /// Controls the Radius at which Neurons are pooled from the Cloud based on the current distance traveled
        /// throughout the Manifold. The higher the value, the more Neurons are pooled from the Cloud and
        /// blended into the Output. Small values will cause the Brain to only use Neurons that are very close
        /// to the current Distance, and is better for convergence. 
        /// </summary>
        public float ManifoldFeather = 0.5f;

        /// <summary>
        /// When Inference is performed, the brain pushes through the manifold one step at a time.
        /// The Iterations are the total count of those steps. There are two "Latent Frames"
        /// Between each iteration, the brain will perform a step of the Cloud Construction Perceptron
        /// And a step of the Manifold Construction Perceptron
        /// </summary>
        public int Iterations = 10;

        /// <summary>
        /// The speed at which the brain travels through the manifold, adding up to the total Iterations
        /// </summary>
        private int _traveledDistance = 0;

        /// <summary>
        /// The amount of Training Steps that have been performed so far. This is used to determine when to stop training.
        /// </summary>
        private int _trainingSteps = 0;

        public List<byte> outputs = new List<byte>();

        public byte[] Infer(byte[] inputs)
        {
            if (_trainingSteps < 2)
            {

            }
            else
            {
                Iterations++;
            }

            switch (_trainingSteps)
            {
                case 0:

                    break;
                case 1:
                    break;
                default:
                    Iterations++;
                    break;
            }

            return outputs.ToArray();
        }
    }

    /// <summary>
    /// A Hyperdimensional Manifold is a collection of Data Points in space, that can be sliced with a plane into a line
    /// </summary>
    public class HyperdimensionalManifold
    {
        private List<List<float>> dataLists;
        private int numDimensions;
        private int numValues;

        public HyperdimensionalManifold(int numDimensions, int numValues)
        {
            this.numDimensions = numDimensions;
            this.numValues = numValues;
            dataLists = new List<List<float>>();

            for (int i = 0; i < numDimensions; i++)
            {
                dataLists.Add(new List<float>());
            }
        }

        /// <summary>
        /// Adds a Data Point to the Manifold
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="ArgumentException"></exception>
        public void AddDataPoint(List<float> data)
        {
            if (data.Count != numDimensions)
            {
                throw new ArgumentException("Data dimensions do not match the number of dimensions in the manifold.");
            }

            for (int i = 0; i < numDimensions; i++)
            {
                dataLists[i].Add(data[i]);
            }
        }
        
        public void SetDataPoint(int index, List<float> data)
        {
            if (data.Count != numDimensions)
            {
                throw new ArgumentException("Data dimensions do not match the number of dimensions in the manifold.");
            }

            for (int i = 0; i < numDimensions; i++)
            {
                dataLists[i][index] = data[i];
            }
        }

        public void SetDataPoint(int[] position, float value)
        {
            if (position.Length != numDimensions)
            {
                throw new ArgumentException("Position dimensions do not match the number of dimensions in the manifold.");
            }

            for (int i = 0; i < numDimensions; i++)
            {
                int index = position[i];

                if (index < 0 || index >= numValues)
                {
                    throw new ArgumentOutOfRangeException("Index out of range.");
                }

                dataLists[i][index] = value;
            }
        }

        /// <summary>
        /// Calculates the distance between two points in the manifold
        /// </summary>
        /// <param name="index1"></param>
        /// <param name="index2"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public float CalculateDistance(int index1, int index2)
        {
            if (index1 < 0 || index1 >= numValues || index2 < 0 || index2 >= numValues)
            {
                throw new ArgumentOutOfRangeException("Index out of range.");
            }

            float distance = 0f;
            for (int i = 0; i < numDimensions; i++)
            {
                float diff = dataLists[i][index1] - dataLists[i][index2];
                distance += diff * diff;
            }

            return (float)Math.Sqrt(distance);
        }

        /// <summary>
        /// Creates a loop around the manifold, slicing it with the given plane coefficients
        /// </summary>
        /// <param name="planeCoefficients"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public List<float> CreateLoopAroundManifold(List<float> planeCoefficients)
        {
            if (numDimensions < 3)
            {
                throw new InvalidOperationException("Manifold must have at least 3 dimensions to perform slicing.");
            }

            if (planeCoefficients.Count != numDimensions + 1)
            {
                throw new ArgumentException("Plane coefficients do not match the number of dimensions in the manifold.");
            }

            List<float> flattenedPoints = new List<float>();

            for (int i = 0; i < numValues; i++)
            {
                float distance = 0f;

                for (int j = 0; j < numDimensions; j++)
                {
                    float diff = dataLists[j][i] - planeCoefficients[j];
                    distance += diff * diff;
                }

                // If the point lies on the plane (within a tolerance), add its value to the flattened points
                if (Math.Abs(distance - planeCoefficients[numDimensions]) < 0.001f)
                {
                    flattenedPoints.Add(dataLists[0][i]);
                }
            }

            // Connect the last point with the first point to create a loop
            flattenedPoints.Add(flattenedPoints[0]);

            return flattenedPoints;
        }
    }
}