using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Voxels : MonoBehaviour
{
    public Vector3Int size = new Vector3Int(8, 8, 8);
    public float surface = 0;

    private float[,,] voxels = new float[9, 9, 9];

    [ContextMenu("Randomize Voxels")]
    private void Randomize() {
        for (int x = 0; x < voxels.GetLength(0); x++)
        {
            for (int y = 0; y < voxels.GetLength(1); y++)
            {
                for (int z = 0; z < voxels.GetLength(2); z++)
                {
                    voxels[x, y, z] = Random.Range(0f, 1f);
                }
            }
        }
       
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube((Vector3)size / 2, size);

        for (int x = size.x; x >= 0; x--)
        {
            for (int y = size.y; y >= 0; y--)
            {
                for (int z = size.z; z >= 0; z--)
                {
                    if (voxels[x, y, z] >= surface)
                    {
                        Gizmos.color = Color.Lerp(Color.black, Color.white, voxels[x, y, z]);
                        Gizmos.DrawSphere(new Vector3(x, y, z), 0.1f);   
                    }
                }
            }
        }
    }

    private void OnValidate() {
        if (size.x + 1 != voxels.GetLength(0) || size.y + 1 != voxels.GetLength(1) || size.z + 1 != voxels.GetLength(2))
        {
            size.Clamp(Vector3Int.one, Vector3Int.one * 16);
            voxels = new float[size.x + 1, size.y + 1, size.z + 1];
        }

        surface = Mathf.Clamp01(surface);
    }
}
