using System;
using System.Linq;
using UnityEngine;
using AI.Core;
using UDT.Materials;

namespace AI.Sensors
{
    /// <summary>
    /// A sensor that detects the material type of objects in the environment.
    /// </summary>
    public class MaterialSensor : MonoBehaviour, ISensor
    {
        [SerializeField] private float maxDistance = 10f; // Maximum distance for raycasting
        [SerializeField] private LayerMask layerMask; // Layer mask to filter objects
        [SerializeField] private int inputCount = 10; // Number of inputs to provide

        public int InputCount { get; set; }

        public float[] GetInputs()
        {
            float[] inputs = new float[inputCount];

            for (int i = 0; i < inputCount; i++)
            {
                // Calculate the step angle for each input
                float stepAngle = 360f / inputCount;

                // Calculate the ray direction based on the step angle
                Vector3 direction = Quaternion.Euler(0f, stepAngle * i, 0f) * transform.forward;

                RaycastHit hit;
                if (Physics.Raycast(transform.position, direction, out hit, maxDistance, layerMask))
                {
                    // Retrieve the material type of the hit object
                    string materialType = GetMaterialType(hit.collider.gameObject);

                    // Assign a value to the input based on the material type
                    inputs[i] = GetMaterialValue(materialType);
                }
            }

            return inputs;
        }

        /// <summary>
        /// Returns a value based on the material type.
        /// </summary>
        /// <param name="materialType"></param>
        /// <returns></returns>
        private float GetMaterialValue(string materialType)
        {
            var keyArray = MaterialsModule.Data.MaterialTypes.Keys.ToArray();
            var value = (float) MaterialsModule.Data.MaterialTypes.Count / Array.IndexOf(keyArray, materialType);
            return value;
        }

        /// <summary>
        /// Returns the material type of the given object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private string GetMaterialType(GameObject obj)
        {
            var material = FindObjectMaterial(obj);
            string matType = "Unknown";
            if (material != null)
            {
                // Find the material type of the given material
                matType = MaterialsModule.FindMaterialType(material);
            }
            return matType;
        }
        
        /// <summary>
        /// Returns the material of the given object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Material FindObjectMaterial(GameObject obj)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material[] materials = renderer.materials;
                if (materials.Length > 0)
                {
                    // Assuming the object has a single material, you can return the first material
                    return materials[0];
                }
            }

            // If no material found, return null or handle the case as needed
            return null;
        }
    }
}