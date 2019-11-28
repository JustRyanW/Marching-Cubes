using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shader : MonoBehaviour
{
    public ComputeShader shader1;
    [Space]
    [SerializeField]
    Vector3[] data = new Vector3[1];
    [SerializeField]
    Vector3[] output = new Vector3[1];


    private void Start()
    {
        RunShader();
    }

    void RunShader()
    {
        ComputeBuffer buffer = new ComputeBuffer(data.Length, 12);
        buffer.SetData(data);
        int kernel = shader1.FindKernel("Multiply");
        shader1.SetBuffer(kernel, "dataBuffer", buffer);
        shader1.Dispatch(kernel, data.Length, 1, 1);
        buffer.GetData(output);
        buffer.Dispose();
    }
}
