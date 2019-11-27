using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public enum TerrainType { Perlin3D, Flat, Landscape };
    public TerrainType terrainType = TerrainType.Perlin3D;

    [Header("Settings")]
    public bool solidEdges = true;

    [Header("Perlin3D")]
    public float scale = 7f;
    public Vector3 offset = Vector3.zero;

    // [Header("Flat")]

    [Header("Landscale")]
    [Range(0, 5)]
    public float amplitude = 2f;
    [Range(0, 5)]
    public float frequency = 1f;
    public Vector2 lOffset = Vector2.zero;

    public void GenerateTerrain(ref float[,,] terrainMap, Vector3Int size)
    {
        terrainMap = new float[size.x + 1, size.y + 1, size.z + 1];

        // The data points for terrain are stored at the corners of our "cubes", so the terrainMap needs to be 1 larger
        // than the width/height/depth of our mesh.
        for (int x = 0; x < size.x + 1; x++)
        {
            for (int y = 0; y < size.y + 1; y++)
            {
                for (int z = 0; z < size.z + 1; z++)
                {
                    if (solidEdges && (x == 0 || x == size.x || y == 0 || y == size.y || z == 0 || z == size.z))
                        terrainMap[x, y, z] = float.MinValue;
                    else
                    {
                        switch (terrainType)
                        {

                            case TerrainType.Perlin3D:

                                // Get values from 3D noise function.
                                terrainMap[x, y, z] = Noise.Perlin3D(x, y, z, scale, transform.position + offset);

                                break;
                            case TerrainType.Flat:

                                // Get a terrain height using regular old Perlin noise.
                                float thisHeight = (float)size.y * Mathf.PerlinNoise((float)x / 16f * 1.5f + 0.001f, (float)z / 16f * 1.5f + 0.001f);

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
                                terrainMap[x, y, z] = -point + 1;

                                break;
                            case TerrainType.Landscape:

                                // Create a landscape terrain but smooth
                                terrainMap[x, y, z] = (Mathf.PerlinNoise(x / 16f * frequency + lOffset.x, z / 16f * frequency + lOffset.y) * amplitude + 0.5f) - y / 8f;
                                break;
                        }
                    }
                }
            }
        }
    }




    // Editor Only

    public VoxelTerrain terrain;

    private void OnValidate()
    {        
        if (terrain == null)
            terrain = GetComponent<VoxelTerrain>();
        terrain.UpdateMesh();
    }
}
