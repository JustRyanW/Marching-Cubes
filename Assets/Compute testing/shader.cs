using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shader : MonoBehaviour
{
    public ComputeShader shader1;
    [Space]
    [SerializeField, ContextMenuItem("Randomise Vectors", "Randomise")]
    float[,,] data = new float[4, 4, 4];
    [SerializeField, ContextMenuItem("RunShader", "RunShader")]
    float[] output = new float[1];

    private void Start()
    {
        Randomise();
    }

    void Randomise()
    {
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int z = 0; z < 4; z++)
                {
                    data[x, y, z] = Random.Range(0, 1.0f);
                }
            }
        }
        RunShader();
    }

    void RunShader()
    {
        ComputeBuffer buffer = new ComputeBuffer(data.Length, 32);
        buffer.SetData(data);
        int kernel = shader1.FindKernel("Multiply");
        shader1.SetBuffer(kernel, "dataBuffer", buffer);
        shader1.Dispatch(kernel, 4, 4, 4);
        buffer.GetData(output);
        buffer.Dispose();
    }
}