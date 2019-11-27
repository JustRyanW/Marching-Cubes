using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VoxelTerrain : MonoBehaviour
{
    [Header("Settings")]
    public Vector3Int size = new Vector3Int(16, 16, 16);
    [Tooltip("Interpolates the edge midpoint between vertexes to match the voxel data, creates smooth shapes.")]
    public bool smoothTerrain = true;
    [Tooltip("Smooths the normals of the terrain so it appears smooth.")]
    public bool smoothShading = false;
    [Range(0, 1), Tooltip("Changes the surface value of the terrain.")]
    public float terrainSurface = 0.5f;

    [Header("Private")]
    float[,,] terrainMap;
    Vector3[,,,] vertexMap;
    float[,,,] indexMap;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    MeshFilter meshFilter;
    TerrainGenerator terrainGenerator;
    Mesh mesh;
    MeshCollider meshCollider;

    private void Start()
    {
        FindComponents();
        UpdateMesh();
    }

    private void Update() {
        // UpdateMesh();
    }

    public void UpdateMesh()
    {
        PopulateTerrainMap();
        CreateMeshData();
    }

    void CreateMeshData()
    {
        ClearMeshData();

        // Loop through each "cube" in our terrain.
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    // Create an array of floats representing each corner of a cube and get the value from our terrainMap.
                    float[] cube = new float[8];
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3Int corner = new Vector3Int(x, y, z) + MarchingCubes.CornerTable[i];
                        cube[i] = terrainMap[corner.x, corner.y, corner.z];
                    }

                    // Pass the value into our MarchCube function.
                    MarchCube(new Vector3(x, y, z), cube);

                }
            }
        }
        BuildMesh();
    }

    void PopulateTerrainMap()
    {
        terrainGenerator.GenerateTerrain(ref terrainMap, size);

        vertexMap = new Vector3[size.x + 1, size.y + 1, size.z + 1, 3];
        indexMap = new float[size.x + 1, size.y + 1, size.z + 1, 3];

        for (int x = 0; x < size.x + 1; x++)
        {
            for (int y = 0; y < size.y + 1; y++)
            {
                for (int z = 0; z < size.z + 1; z++)
                {
                    Vector3 vec0 = new Vector3(x, y, z);
                    Vector3Int[] vec = new Vector3Int[3] {
                        new Vector3Int(x + 1, y, z),
                        new Vector3Int(x, y + 1, z),
                        new Vector3Int(x, y, z + 1)
                    };

                    for (int i = 0; i < 3; i++)
                    {
                        if ((i == 0 && x < size.x) || (i == 1 && y < size.y) || (i == 2 && z < size.z))
                        {
                            if (smoothTerrain)
                            {
                                // vec1 = vec0

                                // Calculate the difference between the terrain values.
                                float difference = (float)terrainMap[vec[i].x, vec[i].y, vec[i].z] - terrainMap[x, y, z];

                                // If the difference is 0, then the terrain passes through the middle.
                                if (difference == 0)
                                    difference = terrainSurface;
                                else
                                    difference = (terrainSurface - terrainMap[x, y, z]) / difference;

                                // Calcualte the point along the edge that passes through.
                                vertexMap[x, y, z, i] = vec0 + ((vec[i] - vec0) * difference);
                            } else
                                vertexMap[x, y, z, i] = (vec0 + vec[i]) / 2;

                        }
                    }
                }
            }
        }
    }

    void MarchCube(Vector3 position, float[] cube)
    {

        // Get the configuration index of this cube.
        int configIndex = GetCubeConfiguration(cube);

        // If the configuration of this cube is 0 or 255 (completely inside the terrain or completely outside of it) we don't need to do anything.
        if (configIndex == 0 || configIndex == 255)
            return;

        // Loop through the triangles. There are never more than 5 triangles to a cube and only three vertices to a triangle.
        int edgeIndex = 0;
        for (int i = 0; i < 5; i++)
        {
            for (int p = 0; p < 3; p++)
            {
                // Get the current indice. We increment triangleIndex through each loop.
                int indice = MarchingCubes.TriangleTable[configIndex, edgeIndex];

                // If the current edgeIndex is -1, there are no more indices and we can exit the function.
                if (indice == -1)
                    return;


                // Get the vertices for the start of this edge.
                Vector3 vert1 = position + MarchingCubes.CornerTable[MarchingCubes.EdgeIndexes[indice, 0]];
                // get the offset for this edge
                int vertIndex = MarchingCubes.VertIndexes[indice];

                if (smoothShading)
                {
                    // check the index map if a vertex has already been created here
                    if (indexMap[(int)vert1.x, (int)vert1.y, (int)vert1.z, vertIndex] == 0)
                    {
                        // if none has been created add a new one vertex and increment the triangles and add to the index map
                        vertices.Add(vertexMap[(int)vert1.x, (int)vert1.y, (int)vert1.z, vertIndex]);
                        triangles.Add(vertices.Count - 1);
                        indexMap[(int)vert1.x, (int)vert1.y, (int)vert1.z, vertIndex] = vertices.Count - 1;
                    }
                    else
                        // if so add the index of that vertice
                        triangles.Add((int)indexMap[(int)vert1.x, (int)vert1.y, (int)vert1.z, vertIndex]);
                }
                else
                {
                    vertices.Add(vertexMap[(int)vert1.x, (int)vert1.y, (int)vert1.z, vertIndex]);
                    triangles.Add(vertices.Count - 1);
                }


                edgeIndex++;
            }
        }
    }

    int VertForIndice(Vector3 vert)
    {
        // Loop through all the vertices currently in the vertices list.
        for (int i = 0; i < vertices.Count; i++)
        {
            // If we find a vert that matches ours, then simply return this index.
            if (vertices[i] == vert)
                return i;
        }

        // If we didn't find a match, add this vert to the list and return last index.
        vertices.Add(vert);
        return vertices.Count - 1;

    }



    int GetCubeConfiguration(float[] cube)
    {
        // Starting with a configuration of zero, loop through each point in the cube and check if it is below the terrain surface.
        int configurationIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            // If it is, use bit-magic to the set the corresponding bit to 1. So if only the 3rd point in the cube was below
            // the surface, the bit would look like 00100000, which represents the integer value 32.
            if (cube[i] < terrainSurface)
                configurationIndex |= 1 << i;
        }
        return configurationIndex;
    }

    void ClearMeshData()
    {
        vertices.Clear();
        triangles.Clear();
    }

    void BuildMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        if (meshCollider != null)
            meshCollider.sharedMesh = mesh;
    }

    private void FindComponents()
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();
        if (terrainGenerator == null)
            terrainGenerator = GetComponent<TerrainGenerator>();
        if (mesh == null)
        {
            mesh = new Mesh();
            meshFilter.mesh = mesh;
        }
        if (GetComponent<MeshCollider>() != null)
            meshCollider = GetComponent<MeshCollider>();
    }

    // Editor Only

    private void OnValidate()
    {
        FindComponents();
        UpdateMesh();
    }

}