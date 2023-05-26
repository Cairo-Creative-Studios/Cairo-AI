using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AI.Core;
using CairoAI.Layers;
using CairoAI.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CairoAI.EvolutionSystems
{
    [Serializable]
    public class Bubble : GenePool
    {
        DumbVoronoi diagram;
        
        /// <summary>
        /// The population size of the pool
        /// </summary>
        public int populationSize;
        private List<Head> _population = new List<Head>();
        /// <summary>
        /// The Subordinate Heads in the pool, they move around the Voronoi Diagram, and observe and inherit weights from the dominant heads
        /// </summary>
        public List<Head> subHeads = new List<Head>();
        /// <summary>
        /// The Dominant Heads in the pool, they are stationary and are trained with a Multilayer Perceptron
        /// </summary>
        [SerializeField]
        private SerializableDictionary<VoronoiPoint, Head> DomHeads = new SerializableDictionary<VoronoiPoint, Head>();
        /// <summary>
        /// The maximum lifetime of the heads in the pool
        /// </summary>
        public float MaxLifeTime;
        
        /// <summary>
        /// The mutation curve of the pool, determining the rate at which mutation occurs over time
        /// </summary>
        public AnimationCurve MutationCurve;
        /// <summary>
        /// The curve of the observance of Subheads inheriting weights from dominant heads over time
        /// </summary>
        public AnimationCurve ObservanceCurve;
        /// <summary>
        /// The curve of the error applied to MLP Networks of the Pool over time
        /// </summary>
        public AnimationCurve ErrorCurve;
        /// <summary>
        /// The curve of the randomness applied to the error of the MLP Networks of the Pool over time
        /// </summary>
        public AnimationCurve ErrorRandomnessCurve;
        
        /// <summary>
        /// The rate at which the mutation occurs
        /// </summary>
        public float mutationStrength = 0.1f;
        /// <summary>
        /// An amount of randomness to add to final error value of the Trains Networks
        /// </summary>
        public float errorRandomness = 0.1f;
        /// <summary>
        /// The amount of influence the Diversity from the Mean Magnitude and Head Magnitude has on the fitness of the heads
        /// </summary>
        public float nicheStrength = 0.1f;
        
        /// <summary>
        /// The highest amount of fitness in the pool.
        /// </summary>
        private float leadingFitness = 0;
        /// <summary>
        /// A function that determines the fitness of the heads in the pool
        /// </summary>
        public Func<Head, float> DetermineFitness;
        /// <summary>
        /// The cells whose heads have been dropped
        /// </summary>
        private List<float[]> _droppedPoints = new List<float[]>();
        /// <summary>
        /// The mean of Magnitude of all Heads
        /// </summary>
        private float _meanMagnitude = 0;

        private int step = 0;
        private int resolution;
        
        // Update Variables
        int i = 0;
        int j = 0;
        int k = 0;
        Head subhead;
        Head domhead;
        int weightCount;
        int start = 0;
        List<Vector4> newWeights = new List<Vector4>();

        public Bubble(int subCount, int resolution, Func<Head, float> fitness = null)
        {
            this.resolution = resolution;
            
            // Set Dom and Sub Population Size
            populationSize = subCount + resolution*3;

            // Generate Voronoi Diagram for dominant heads
            diagram = new DumbVoronoi(new int[]{resolution, resolution, resolution});
            
            // Add dominant heads
            for (int i = 0; i < diagram.points.Count; i++)
            {
                var head = new Head(){floatingPosition = new Vector3(diagram.points[i].position.x, diagram.points[i].position.y, diagram.points[i].position.z)};
                head.Fitness = 0;
                head.lifeTime = 0;
                head.isDominant = true;
                head.Point = diagram.points[i];
                DomHeads.Add(diagram.points[i], head);
                _population.Add(head);
            }
            
            // Add sub heads
            for (int i = 0; i < subCount; i++)
            {
                var random = Random.Range(0, resolution);
                var head = new Head()
                {
                    floatingPosition = new Vector3(random/(resolution), (random % resolution)/(resolution/2), (random % resolution)%resolution)
                };
                head.Fitness = 0;
                head.lifeTime = 0;
                subHeads.Add(head);
                subHeads[i].isDominant = false;
                _population.Add(head);
            }

            DetermineFitness = fitness;
            MaxLifeTime = 10;
            mutationStrength = 0.1f;
            
            weightCount = subHeads[0].Brain.network.weights.Length;
        }
        
        public void Start()
        {
            if (MutationCurve == null)
            {
                MutationCurve = AnimationCurve.Linear(0, 0, 1, 1);
            }

            if (ObservanceCurve == null)
            {
                ObservanceCurve = AnimationCurve.Linear(0, 0, 1, 1);
            }
            
            if (ErrorCurve == null)
            {
                ErrorCurve = AnimationCurve.Linear(0, 0, 1, 1);
            }
            
            if (ErrorRandomnessCurve == null)
            {
                ErrorRandomnessCurve = AnimationCurve.Linear(0, 0, 1, 1);
            }
        }

        public override void UpdatePool()
        {
            // Steps:
            // Orbit and Train 
            // Observe and Mutate //TODO: Add Cultural Gradient Mutation, Pushing Mutations in different directions to reduce the chance of local minima
            // Age, Die, and Inherit TODO: Ensure Heads fill the pool as needed

            step = (step + 1)%3;
            switch (step)
            {
                // Orbit and Train
                case 0:
                    Parallel.For(start, subHeads.Count, i =>
                    {
                        var subHead = subHeads[i];
                        subHeads[i].Fitness = DetermineFitness(subHeads[i]);
                        
                        var cell = diagram.GetClosestPoint(new Vector3(subHead.floatingPosition.x, subHead.floatingPosition.y,
                            subHead.floatingPosition.z));
                        domhead = DomHeads[cell];

                        var direction = domhead.floatingPosition - subHead.floatingPosition;
                        direction.Normalize();
                        direction *= 0.1f;

                        subHead.floatingPosition.x += direction.x;
                        subHead.floatingPosition.y += direction.y;
                        subHead.floatingPosition.z += direction.z;
                    });
                    Parallel.For(start, DomHeads.Count, i =>
                    {
                        domhead = DomHeads.ValueAt(i);
                        domhead.Brain.network.error = (leadingFitness - domhead.Fitness + Random.Range(-errorRandomness, errorRandomness) * ErrorRandomnessCurve.Evaluate(domhead.lifeTime/MaxLifeTime))*ErrorCurve.Evaluate(domhead.lifeTime/MaxLifeTime);
                        domhead.Brain.backpropagate();
                    });
                    break;
                // Observe and Mutate
                case 1:
                    start = 0;
                    VoronoiPoint cell;
                    Parallel.For(start, subHeads.Count, i =>
                    {
                        subhead = subHeads[i];
                        subhead.Magnitude = 0;

                        newWeights.Clear();
                        newWeights.AddRange(subhead.Brain.network.weights);

                        cell = diagram.GetClosestPoint(new Vector3(subhead.floatingPosition.x, subhead.floatingPosition.y,
                            subhead.floatingPosition.z));
                        domhead = DomHeads[cell];
                        
                        if (domhead != null)
                        {
                            for (int j = 0; j < weightCount; j++)
                            {
                                newWeights[i] += new Vector4()
                                {
                                    x = domhead.Brain.network.weights[i].x +
                                        (Random.Range(-mutationStrength * Mathf.Sin(k), mutationStrength) * Mathf.Sin(k)) * MutationCurve.Evaluate(domhead.lifeTime/MaxLifeTime),
                                    y = domhead.Brain.network.weights[i].y +
                                        (Random.Range(-mutationStrength * Mathf.Sin(k), mutationStrength) * Mathf.Cos(k)) * MutationCurve.Evaluate(domhead.lifeTime/MaxLifeTime),
                                    z = domhead.Brain.network.weights[i].z +
                                        (Random.Range(-mutationStrength * Mathf.Sin(i), mutationStrength) * Mathf.Cos(i)) * MutationCurve.Evaluate(domhead.lifeTime/MaxLifeTime),
                                    w = domhead.Brain.network.weights[i].w + 
                                        (Random.Range(-mutationStrength * Mathf.Cos(i), mutationStrength * Mathf.Sin(i))) * MutationCurve.Evaluate(domhead.lifeTime/MaxLifeTime)
                                };
                                subhead.Magnitude += newWeights[i].magnitude;
                            }
                        }

                        subhead.Brain.network.weights = newWeights.ToArray();
                    });
                    break;

                // Age, Kill, and Inherit
                // TODO: Add a "Dropped Points" Array to keep track of Dom Heads that have been Killed
                // TODO: The Elements of the Array would have the Point, Dropped Duration, and the fittest and second fittest subheads that are closest to the point
                // TODO: Remove the Subhead Search from the Kill Loop and instead use the Dropped Points Array
                case 2:
                    Parallel.For(start, _population.Count, index =>
                    {
                        subhead = _population[index];
                        subhead.lifeTime += Time.deltaTime;
                        
                        subhead.Fitness = DetermineFitness(subhead) + subhead.Magnitude * nicheStrength;

                        if (subhead.lifeTime > MaxLifeTime)
                        {
                            if (subhead.isDominant)
                            {
                                //Search for an Heir 
                                var nearest = subHeads.FindAll(x => x.Point == subhead.Point);

                                Head fittest = null;
                                Head secondFittest = null;

                                float greatestFitness = 0;

                                foreach (var candidate in nearest)
                                {
                                    if (candidate.Fitness > greatestFitness)
                                    {
                                        greatestFitness = candidate.Fitness;
                                        secondFittest = fittest;
                                        fittest = candidate;
                                    }
                                }

                                if (secondFittest == null)
                                {
                                    if (fittest == null)
                                    {
                                        secondFittest = new Head()
                                        {
                                            floatingPosition = new  Vector3(subhead.Point.position.x, subhead.Point.position.y, subhead.Point.position.z),
                                            Point = subhead.Point,
                                            isDominant = true
                                        };
                                        secondFittest.Fitness = 0;
                                        secondFittest.lifeTime = 0;
                                    }
                                    else
                                        secondFittest = fittest;
                                }
                                
                                secondFittest.isDominant = true;
                                secondFittest.Point = subhead.Point;
                                DomHeads.Add(secondFittest.Point, secondFittest);
                                
                                //Remove the Head from DomHeads
                                DomHeads.Remove(subhead.Point);
                                HeadCreated?.Invoke(subhead);
                                
                                //Replace the Head
                                var random = Random.Range(0, resolution);
                                var head = new Head()
                                {
                                    floatingPosition = new Vector3(random/(resolution), (random % resolution)/(resolution/2), (random % resolution)%resolution)
                                };
                                head.Fitness = 0;
                                head.lifeTime = 0;
                                subHeads.Add(head);
                                subHeads[i].isDominant = false;
                                _population.Add(head);
                            }
                            else
                            {
                                //Remove the Head from SubHeads
                                subHeads.Remove(subhead);
                                HeadDestroyed?.Invoke(subhead);
                                
                                //Replace the Head
                                var random = Random.Range(0, resolution);
                                var head = new Head()
                                {
                                    floatingPosition = new Vector3(random/(resolution), (random % resolution)/(resolution/2), (random % resolution)%resolution)
                                };
                                head.Fitness = 0;
                                head.lifeTime = 0;
                                subHeads.Add(head);
                                subHeads[i].isDominant = false;
                                _population.Add(head);
                            }

                            _population.RemoveAt(i);
                        }
                    });
                    break;
            }
        }
        
        /// <summary>
        /// Send Inputs into the Network and return the Outputs
        /// </summary>
        /// <param name="head">The Head in the Population to use for Inference</param>
        /// <param name="inputs">The Inputs to pass into the Head</param>
        /// <returns></returns>
        public float[] Infer(int head, float[] inputs)
        {
            return _population[head].Brain.Infer(inputs);
        }
        
        /// <summary>
        /// Kills the Head at the given Index in the Population
        /// </summary>
        /// <param name="headIndex"></param>
        public void Kill(int headIndex)
        {
            var head = _population[headIndex];
            Kill(head);
        }

        /// <summary>
        /// Kills the given Head
        /// </summary>
        /// <param name="head"></param>
        public void Kill(Head head)
        {
            if (head.isDominant)
            {
                var index = DomHeads.Values.ToArray().ToList().IndexOf(head);
                DomHeads.Remove(DomHeads.KeyAt(index));
                
            }
            else
            {
                subHeads.Remove(head);
            }
            
            _population.Remove(head);
            RecalculateDiagram();
        }
        
        private void FillGenePool()
        {
            int createCount = populationSize - subHeads.Count;
            for (int i = 0; i < createCount; i++)
            {
                var random = Random.Range(0, resolution ^ 3);
                var head = new Head()
                {
                    floatingPosition = new Vector3(random/(resolution), (random % resolution)/(resolution/2), (random % resolution)%resolution),
                };
                head.Fitness = 0;
                head.lifeTime = 0;
                subHeads.Add(head);
                subHeads[^1].isDominant = false;
            }
        }
        
        private void RecalculateDiagram()
        {
            diagram.Clear();
            diagram.Create3D(new int[]{resolution, resolution, resolution});
            foreach (var sub in subHeads)
            {
                sub.Point = diagram.GetClosestPoint(sub.floatingPosition);
            }
        }

        /// <summary>
        /// A head is a single entity in the genetic pool
        /// </summary>
        [Serializable]
        public class Head : Brain
        {
            public Vector3 floatingPosition;
            /// <summary>
            /// The fitness of the head
            /// </summary>
            public float Fitness;
            /// <summary>
            /// The magnitude of the head, all weights are added together to get the magnitude
            /// </summary>
            public float Magnitude;
            /// <summary>
            /// The lifetime of the head
            /// </summary>
            public float lifeTime;
            /// <summary>
            /// Is the head dominant or not
            /// </summary>
            public bool isDominant;
            /// <summary>
            /// The index of the cell the head is in
            /// </summary>
            public int cellIndex;
            public VoronoiPoint Point;
            /// <summary>
            /// The brain of the head
            /// </summary>
            public IratePerceptron Brain;
        }
    }
}