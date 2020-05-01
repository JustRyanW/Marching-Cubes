using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxels : MonoBehaviour
{
    public Vector3Int size = new Vector3Int(8, 8, 8);
    public float surface = 0;

    private float[,,] voxels = new float[8, 8, 8];

    private void Start() { 

        // List all offsets
        for (int i = 0; i < 8; i++)
        {
            float x = (i & 4) >> 2;
            float y = (i & 2) >> 1;
            float z = i & 1;

           // Debug.Log(x + " " + y + " " + z);
        }
    }

    [ContextMenu("Randomize Voxels")]
    private void Randomize() {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    voxels[x, y, z] = Random.Range(0f, 1f);
                }
            }
        }
       
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube((Vector3)size / 2, size);

        for (int x = size.x - 1; x >= 0; x--)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = size.z - 1; z >= 0; z--)
                {
                    if (voxels[x, y, z] >= surface)
                    {
                        Gizmos.color = Color.Lerp(Color.black, Color.white, voxels[x, y, z]);
                        Gizmos.DrawCube(new Vector3(x, y, z) + Vector3.one * 0.5f, Vector3.one * 0.1f);   
                    }
                }
            }
        }
    }

    private void OnValidate() {
        if (size.x != voxels.GetLength(0) || size.y != voxels.GetLength(1) || size.z != voxels.GetLength(2))
        {
            size.Clamp(Vector3Int.one, Vector3Int.one * 16);
            voxels = new float[size.x, size.y, size.z];
        }

        surface = Mathf.Clamp01(surface);
    }
}
