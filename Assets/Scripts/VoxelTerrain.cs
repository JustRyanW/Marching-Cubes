using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VoxelTerrain : MonoBehaviour
{
    [Header("Settings")]
    public Vector3Int size = new Vector3Int(16, 16, 16);
    [Tooltip("Interpolates the edge midpoint between vertexes to match the voxel data, creates smooth shapes.")]
    public bool smoothTerrain = true;
    [Tooltip("Smooths the normals of the terrain so it appears smooth.\nWaring: Enabling this will cause massive lag when generating the mesh!")]
    public bool smoothShading = false;
    [Range(0, 1), Tooltip("Changes the surface value of the terrain.")]
    public float terrainSurface = 0.5f;

    [Header("Private")]
    float[,,] terrainMap;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    MeshFilter meshFilter;
    TerrainGenerator terrainGenerator;

    private void Start()
    {

        meshFilter = GetComponent<MeshFilter>();
        terrainGenerator = GetComponent<TerrainGenerator>();

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

                // Get the vertices for the start and end of this edge.
                Vector3 vert1 = position + MarchingCubes.CornerTable[MarchingCubes.EdgeIndexes[indice, 0]];
                Vector3 vert2 = position + MarchingCubes.CornerTable[MarchingCubes.EdgeIndexes[indice, 1]];

                Vector3 vertPosition;
                if (smoothTerrain)
                {

                    // Get the terrain values at eaither end of our current edge from the cube array created above.
                    float vert1Sample = cube[MarchingCubes.EdgeIndexes[indice, 0]];
                    float vert2Sample = cube[MarchingCubes.EdgeIndexes[indice, 1]];

                    // Calculate the difference between the terrain values.
                    float difference = vert2Sample - vert1Sample;

                    // If the difference is 0, then the terrain passes through the middle.
                    if (difference == 0)
                        difference = terrainSurface;
                    else
                        difference = (terrainSurface - vert1Sample) / difference;

                    // Calcualte the point along the edge that passes through.
                    vertPosition = vert1 + ((vert2 - vert1) * difference);

                }
                else
                {

                    // Get the midpoint of this edge.
                    vertPosition = (vert1 + vert2) / 2f;

                }

                // Add to our vertices and triangles list and incremement the edgeIndex.
                if (smoothShading)
                    triangles.Add(VertForIndice(vertPosition));
                else
                {
                    vertices.Add(vertPosition);
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

        Mesh mesh;
        if (meshFilter.mesh == null)
        {
            mesh = new Mesh();
            meshFilter.mesh = mesh;
        }
        else
        {
            mesh = meshFilter.mesh;
            mesh.Clear();
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

    }



    // Editor Only

    private void OnValidate()
    {
        if (Application.isPlaying && Time.time > 1)
            UpdateMesh();
    }

    public void UpdateMesh()
    {
        PopulateTerrainMap();
        CreateMeshData();
    }

}