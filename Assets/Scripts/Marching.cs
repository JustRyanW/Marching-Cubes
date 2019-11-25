using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Marching : MonoBehaviour {

    MeshFilter meshFilter;

    float terrainSurface = 0.5f;
    int width = 32;
    int height = 8;
    float[,,] terrainMap;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    private void Start() {

        meshFilter = GetComponent<MeshFilter>();
        terrainMap = new float[width + 1, height + 1, width + 1];
        PopulateTerrainMap();
        CreateMeshData();

    }

    void CreateMeshData() {

        ClearMeshData();

        // Loop through each "cube" in our terrain.
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < width; z++) {

                    // Create an array of floats representing each corner of a cube and get the value from our terrainMap.
                    float[] cube = new float[8];
                    for (int i = 0; i < 8; i++) {

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

    void PopulateTerrainMap () {

        // The data points for terrain are stored at the corners of our "cubes", so the terrainMap needs to be 1 larger
        // than the width/height of our mesh.
        for (int x = 0; x < width + 1; x++) {
            for (int z = 0; z < width + 1; z++) {
                for (int y = 0; y < height + 1; y++) {

                    // Get a terrain height using regular old Perlin noise.
                    float thisHeight = (float)height * Mathf.PerlinNoise((float)x / 16f * 1.5f + 0.001f, (float)z / 16f * 1.5f + 0.001f);

                    float point = 0;
                    // We're only interested when point is within 0.5f of terrain surface. More than 0.5f less and it is just considered
                    // solid terrain, more than 0.5f above and it is just air. Within that range, however, we want the exact value.
                    if (y <= thisHeight - 0.5f)
                        point = 0f;
                    else if (y > thisHeight + 0.5f)
                        point = 1f;
                    else if (y > thisHeight)
                        point = (float)y - thisHeight;
                    else
                        point = thisHeight - (float)y;

                    // Set the value of this point in the terrainMap.
                    terrainMap[x, y, z] = point;

                }
            }
        }
    }

    void MarchCube (Vector3 position, float[] cube) {

        // Get the configuration index of this cube.
        int configIndex = GetCubeConfiguration(cube);

        // If the configuration of this cube is 0 or 255 (completely inside the terrain or completely outside of it) we don't need to do anything.
        if (configIndex == 0 || configIndex == 255)
            return;

        // Loop through the triangles. There are never more than 5 triangles to a cube and only three vertices to a triangle.
        int edgeIndex = 0;
        for(int i = 0; i < 5; i++) {
            for(int p = 0; p < 3; p++) {

                // Get the current indice. We increment triangleIndex through each loop.
                int indice = MarchingCubes.TriangleTable[configIndex, edgeIndex];

                // If the current edgeIndex is -1, there are no more indices and we can exit the function.
                if (indice == -1)
                    return;

                // Get the vertices for the start and end of this edge.
                Vector3 vert1 = position + MarchingCubes.CornerTable[MarchingCubes.EdgeIndexes[indice, 0]];
                Vector3 vert2 = position + MarchingCubes.CornerTable[MarchingCubes.EdgeIndexes[indice, 1]];

                // Get the midpoint of this edge.
                Vector3 vertPosition = (vert1 + vert2) / 2f;

                // Add to our vertices and triangles list and incremement the edgeIndex.
                vertices.Add(vertPosition);
                triangles.Add(vertices.Count - 1);
                edgeIndex++;

            }
        }
    }

    int GetCubeConfiguration (float[] cube) {

        // Starting with a configuration of zero, loop through each point in the cube and check if it is below the terrain surface.
        int configurationIndex = 0;
        for (int i = 0; i < 8; i++) {

            // If it is, use bit-magic to the set the corresponding bit to 1. So if only the 3rd point in the cube was below
            // the surface, the bit would look like 00100000, which represents the integer value 32.
            if (cube[i] > terrainSurface)
                configurationIndex |= 1 << i;

        }

        return configurationIndex;

    }

    void ClearMeshData () {

        vertices.Clear();
        triangles.Clear();

    }

    void BuildMesh () {

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

    }

}