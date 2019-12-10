using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class VoxelTerrain : MonoBehaviour
{
    [Header("Settings")]
    public Vector3Int size = new Vector3Int(16, 16, 16);
    [Tooltip("Interpolates the edge midpoint between vertexes to match the voxel data, creates smooth shapes.")]
    public bool smoothTerrain = true;
    [Tooltip("Smooths the normals of the terrain so it appears smooth.")]
    public bool smoothShading = false;
    [Range(0, 1), Tooltip("Changes the surface value of the terrain."), ContextMenuItem("Reset", "ResetSurfaceValue")]
    public float terrainSurface = 0.5f;

    [Header("Debug")]
    public bool drawBorder = true;
    public bool constantUpdate = false;
    public bool showoff = false;

    [Header("Private")]
    float[,,] terrainMap;
    Vector3[,,,] vertexMap;
    int[,,,] indexMap;
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
        DrawBrush(true, new Vector3(8, 8, 8), 8);
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Input.GetMouseButton(0) && Physics.Raycast(ray, out hitInfo))
        {
            DrawBrush(true, hitInfo.point, 20f);
        } else if (Input.GetMouseButton(1) && Physics.Raycast(ray, out hitInfo))
            DrawBrush(false, hitInfo.point, 20f);

        if (showoff)
        {
            transform.position = new Vector3(
                Mathf.Sin(Time.time),
                Mathf.Cos(Time.time),
                Mathf.Sin(Time.time)
            ) * 16;
            constantUpdate = true;
        }
        if (constantUpdate)
            UpdateMesh();
    }

    public void UpdateMesh()
    {
        terrainGenerator.GenerateTerrain(ref terrainMap, size);
        UpdateMesh2();
    }

    public void UpdateMesh2()
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
        vertexMap = new Vector3[size.x + 1, size.y + 1, size.z + 1, 3];
        indexMap = new int[size.x + 1, size.y + 1, size.z + 1, 3];

        for (int x = 0; x < size.x + 1; x++)
        {
            for (int y = 0; y < size.y + 1; y++)
            {
                for (int z = 0; z < size.z + 1; z++)
                {
                    Vector3Int vec0 = new Vector3Int(x, y, z);

                    for (int i = 0; i < 3; i++)
                    {
                        if ((i == 0 && x < size.x) || (i == 1 && y < size.y) || (i == 2 && z < size.z))
                        {
                            Vector3Int vec1 = vec0 + MarchingCubes.offsets[i];
                            if (smoothTerrain)
                            {
                                float val = Mathf.InverseLerp(terrainMap[x, y, z], terrainMap[vec1.x, vec1.y, vec1.z], terrainSurface);
                                vertexMap[x, y, z, i] = Vector3.Lerp(vec0, vec1, val);
                            }
                            else
                                vertexMap[x, y, z, i] = (Vector3)(vec0 + vec1) / 2;

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
                Vector3 vert1 = position + MarchingCubes.CornerTable[MarchingCubes.EdgeIndexes[indice]];
                // get the offset for this edge
                int vertIndex = MarchingCubes.VertIndexes[indice];

                if (smoothShading)
                {
                    // get the index from the index map
                    int index = indexMap[(int)vert1.x, (int)vert1.y, (int)vert1.z, vertIndex];
                    // check the index map if a vertex has already been created here
                    if (index == 0)
                    {
                        // if none has been created add a new one vertex and increment the triangles and add to the index map
                        vertices.Add(vertexMap[(int)vert1.x, (int)vert1.y, (int)vert1.z, vertIndex]);
                        triangles.Add(vertices.Count - 1);
                        indexMap[(int)vert1.x, (int)vert1.y, (int)vert1.z, vertIndex] = vertices.Count - 1;
                    }
                    else
                        // if so add the index of that vertice
                        triangles.Add(index);
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

    void ResetSurfaceValue()
    {
        terrainSurface = 0.5f;
        UpdateMesh();
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

    void DrawBrush(bool addTerrain, Vector3 pos, float brushSize)
    {
        Vector2Int minMaxX = new Vector2Int((int)(pos.x - brushSize), (int)(pos.x + brushSize));
        Vector2Int minMaxY = new Vector2Int((int)(pos.y - brushSize), (int)(pos.y + brushSize));
        Vector2Int minMaxZ = new Vector2Int((int)(pos.z - brushSize), (int)(pos.z + brushSize));
        Vec2IntClamp(ref minMaxX, 0, size.x + 1);
        Vec2IntClamp(ref minMaxY, 0, size.y + 1);
        Vec2IntClamp(ref minMaxZ, 0, size.z + 1);

        for (int x = minMaxX.x; x < minMaxX.y; x++)
        {
            for (int y = minMaxY.x; y < minMaxY.y; y++)
            {
                for (int z = minMaxZ.x; z < minMaxZ.y; z++)
                {
                    float halfBrushSize = brushSize / 2;
                    float dist = Vector3.Distance(new Vector3(x, y, z), pos);
                    if (addTerrain)
                    {
                        if (dist <= brushSize && terrainMap[x, y, z] < halfBrushSize - dist)
                        {
                            terrainMap[x, y, z] = Mathf.Clamp01(terrainMap[x, y, z] + 0.01f);
                        }
                    }
                    else
                    {
                        if (dist <= brushSize && terrainMap[x, y, z] > -halfBrushSize + dist)
                        {
                            terrainMap[x, y, z] = Mathf.Clamp01(terrainMap[x, y, z] - 0.01f);
                        }
                    }
                }
            }
        }
        // queue mesh update at the end of update only if a change has been made, will save performace on multiple edits in one frame
        UpdateMesh2();
    }
    void Vec2IntClamp(ref Vector2Int vector, int min, int max)
    {
        vector.x = (int)Mathf.Clamp(vector.x, min, max);
        vector.y = (int)Mathf.Clamp(vector.y, min, max);
    }

    // Editor Only

    private void OnValidate()
    {
        FindComponents();
        UpdateMesh();
    }

    private void OnDrawGizmos()
    {
        if (drawBorder)
            Gizmos.DrawWireCube(transform.position + (Vector3)size / 2, (terrainGenerator.solidEdges) ? size - Vector3.one * 2 : size);
    }

}