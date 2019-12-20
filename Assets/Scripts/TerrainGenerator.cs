using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public enum TerrainType { Perlin3D, Flat, Landscape };

    [Header("Settings")]
    public TerrainType terrainType = TerrainType.Perlin3D;

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
                    switch (terrainType)
                    {

                        case TerrainType.Perlin3D:

                            // Get values from 3D noise function.
                            terrainMap[x, y, z] = Mathf.Clamp01((Noise.Perlin3D(x, y, z, scale, transform.position + offset) - 0.5f) * 10f + 0.5f); 

                            break;
                        case TerrainType.Flat:
                            terrainMap[x, y, z] = 0;
                            break;
                        case TerrainType.Landscape:

                            // Create a landscape terrain but smooth
                            float value = Mathf.PerlinNoise(x / 16f * frequency + lOffset.x, z / 16f * frequency + lOffset.y) * amplitude;
                            value += Mathf.PerlinNoise(x / 8f * frequency + lOffset.x, z / 8f * frequency + lOffset.y) * amplitude / 4;
                            value += Mathf.PerlinNoise(x / 3f * frequency + lOffset.x, z / 3f * frequency + lOffset.y) * amplitude / 16;
                            terrainMap[x, y, z] = (value + 0.5f) - y / 8f;
                            break;
                    }
                }
            }
        }
    }




    // Editor Only

    VoxelTerrain terrain;

    private void OnValidate()
    {
        if (terrain == null)
            terrain = GetComponent<VoxelTerrain>();
        terrain.UpdateMesh();
    }
}
