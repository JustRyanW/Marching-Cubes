using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float Perlin3D(float x, float y, float z, float scale = 1f, Vector3? offset = null)
    {

        if (scale > 0.001f)
        {
            x /= scale; y /= scale; z /= scale;
        }

        if (offset != null)
        {
            Vector3 o = (Vector3)offset;
            x += o.x; y += o.y; z += o.z;
        }

        float xy = Mathf.PerlinNoise(x, y);
        float yz = Mathf.PerlinNoise(y, z);
        float zx = Mathf.PerlinNoise(z, x);

        float xz = Mathf.PerlinNoise(x, z);
        float zy = Mathf.PerlinNoise(z, y);
        float yx = Mathf.PerlinNoise(y, x);

        float xyz = xy + yz + zx + xz + zy + yx;
        return xyz / 6f;
    }
}
