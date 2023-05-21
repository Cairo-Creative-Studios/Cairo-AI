using System.Collections.Generic;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Bubble.Voronoi
{
    /// <summary>
    /// A class that represents a Voronoi Diagram in 1D, 2D or 3D space
    /// </summary>
    public class DumbVoronoi
    {
        GameObject voronoiDiagram;
        
        /// <summary>
        /// The list of cells in the diagram
        /// </summary>
        public List<VoronoiPoint> points = new List<VoronoiPoint>();
        
        /// <summary>
        /// Creates a new Voronoi Diagram, with a number of cells in each axis
        /// </summary>
        /// <param name="dimensions"></param>
        public DumbVoronoi(int[] dimensions)
        {
            switch (dimensions.Length)
            {
                case 1:
                    Create1D(dimensions);
                    break;
                case 2:
                    Create2D(dimensions);
                    break;
                case 3:
                    Create3D(dimensions);
                    break;
            }

            UpdateNeighbors();
        }
        
        public void Create1D(int[] dimensions)
        {
            for (int x = 0; x < dimensions[0]; x++)
            {
                VoronoiPoint point = new VoronoiPoint();
                point.cell.x = x;
                point.position.x = x + Random.Range(-0.5f, 0.5f);
                points.Add(point);
            }
        }
        
        public void Create2D(int[] dimensions)
        {
            for (int x = 0; x < dimensions[0]; x++)
            {
                for (int y = 0; y < dimensions[1]; y++)
                {
                    VoronoiPoint point = new VoronoiPoint();
                    point.cell.x = x;
                    point.cell.y = y;
                    point.position.x = x + Random.Range(-0.5f, 0.5f);
                    point.position.y = y + Random.Range(-0.5f, 0.5f);
                    points.Add(point);
                }
            }
        }
        
        public void Create3D(int[] dimensions)
        {
            for (int x = 0; x < dimensions[0]; x++)
            {
                for (int y = 0; y < dimensions[1]; y++)
                {
                    for (int z = 0; z < dimensions[2]; z++)
                    {
                        VoronoiPoint point = new VoronoiPoint();
                        point.cell.x = x;
                        point.cell.y = y;
                        point.cell.z = z;
                        point.position.x = x + Random.Range(-0.5f, 0.5f);
                        point.position.y = y + Random.Range(-0.5f, 0.5f);
                        point.position.z = z + Random.Range(-0.5f, 0.5f);
                        points.Add(point);
                    }
                }
            }
        }
        
        /// <summary>
        /// Updates the neighbours of each cell to adjacent cells
        /// </summary>
        private void UpdateNeighbors()
        {
            foreach (VoronoiPoint cell in points)
            {
                List<VoronoiPoint> neighbours = new List<VoronoiPoint>();
                foreach (VoronoiPoint otherCell in points)
                {
                    if (otherCell != cell && Vector3.Distance(new Vector3(cell.cell.x, cell.cell.y, cell.cell.z), new Vector3(otherCell.cell.x, otherCell.cell.y, otherCell.cell.z)) < 1.5f)
                    {
                        neighbours.Add(otherCell);
                    }
                }
                cell.neighbours = neighbours.ToArray();
            }
        }
        
        public void Clear()
        {
            points.Clear();
        }
        
        public int GetClosestCellIndex(Vector3 position)
        {
            int closest = -1;
            float closestDistance = float.MaxValue;
            for (int i = 0; i < points.Count; i++)
            {
                float distance = Vector3.Distance(points[i].position, position);
                if (distance < closestDistance)
                {
                    closest = i;
                    closestDistance = distance;
                }
            }
            return closest;
        }
        
        /// <summary>
        /// Draws the diagram
        /// </summary>
        public void Draw(float scale = 1)
        {
            foreach (VoronoiPoint cell in points)
            {
                Debug.DrawLine(new Vector3(cell.position.x, cell.position.y, cell.position.z)*scale, new Vector3(cell.cell.x, cell.cell.y, cell.cell.z)*scale, Color.red);
            }
        }

        /// <summary>
        /// Draws the diagram as a mesh and returns the game object
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public GameObject DrawMesh(float scale = 1)
        {
            GameObject.Destroy(voronoiDiagram);

            GameObject go = new GameObject("Voronoi Diagram");
            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Standard"));
            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            foreach (VoronoiPoint point in points)
            {
                vertices.Add(new Vector3(point.position.x, point.position.y, point.position.z)*scale);
                vertices.Add(new Vector3(point.cell.x, point.cell.y, point.cell.z)*scale);
                foreach (VoronoiPoint neighbour in point.neighbours)
                {
                    vertices.Add(new Vector3(neighbour.cell.x, neighbour.cell.x, neighbour.cell.x)*scale);
                    triangles.Add(vertices.Count-3);
                    triangles.Add(vertices.Count-2);
                    triangles.Add(vertices.Count-1);
                }
            }
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mf.mesh = mesh;

            DrawPointsAsSpheres(scale/10);
            
            return go;
        }
        
        public GameObject[] DrawPointsAsSpheres(float scale = 1)
        {
            GameObject[] spheres = new GameObject[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                GameObject go = new GameObject("Voronoi Cell " + i);
                go.transform.position = new Vector3(points[i].position.x, points[i].position.y, points[i].position.z)*scale;
                go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                go.AddComponent<MeshFilter>();
                go.AddComponent<MeshRenderer>();
                go.AddComponent<SphereCollider>();
                spheres[i] = go;
                go.transform.parent = voronoiDiagram.transform;
            }
            
            return spheres;
        }
        

        /// <summary>
        /// Returns the closest cell to a position in 1D space
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public VoronoiPoint GetClosestPoint(float position)
        {
            return GetClosestPoint(new Vector3(position, 0, 0));
        }
        
        /// <summary>
        /// Returns the closest cell to a position in 2D space
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public VoronoiPoint GetClosestPoint(Vector2 position)
        {
            return GetClosestPoint(new Vector3(position.X, position.Y, 0));
        }

        /// <summary>
        /// Returns the closest cell to a position in 3D space
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public VoronoiPoint GetClosestPoint(Vector3 position)
        {
            VoronoiPoint closestPoint = null;
            float closestDistance = float.MaxValue;
            foreach (VoronoiPoint point in points)
            {
                float distance = Vector3.Distance(position, new Vector3(point.position.x, point.position.y, point.position.z));
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = point;
                }
            }
            return closestPoint;
        }
        
        public VoronoiPoint[] GetClosestPoints(Vector3 position, int amount)
        {
            VoronoiPoint[] closestCells = new VoronoiPoint[amount];
            float[] closestDistances = new float[amount];
            for (int i = 0; i < amount; i++)
            {
                closestDistances[i] = float.MaxValue;
            }
            foreach (VoronoiPoint point in points)
            {
                float distance = Vector3.Distance(position, new Vector3(point.position.x, point.position.y, point.position.z));
                for (int i = 0; i < amount; i++)
                {
                    if (distance < closestDistances[i])
                    {
                        closestDistances[i] = distance;
                        closestCells[i] = point;
                        break;
                    }
                }
            }
            return closestCells;
        }
    }
    
    public class VoronoiPoint
    {
        public Vector3 cell;
        public Vector3 position;
        public VoronoiPoint[] neighbours;
    }
}