using UnityEngine;
using System.Collections;


public class ComputeShaderOutput : MonoBehaviour {

    public ComputeShader computeShader;
    public const int VertCount = 10 * 10 * 10 * 10 * 10 * 10;

    public ComputeBuffer outputBuffer;

    public Shader PointShader;
    Material PointMaterial;

    public bool DebugRender = false;

    int CSKernel;

    void InitializeBuffers()
    {
        outputBuffer = new ComputeBuffer(VertCount, (sizeof(float) * 3));
        computeShader.SetBuffer(CSKernel, "outputBuffer", outputBuffer);

        if (DebugRender)
            PointMaterial.SetBuffer("buffer", outputBuffer);

    }
    static bool dispatchDone = false;
    public void Dispatch()
    {
        if(!SystemInfo.supportsComputeShaders)
        {
            Debug.LogWarning("Compute shaders not suppported.");
            return;
        }
        
        if (dispatchDone) return;
        computeShader.Dispatch(CSKernel, 10, 10, 10);
        dispatchDone = true;
    }

    void ReleaseBuffers()
    {
        outputBuffer.Release();
    }

    void Start()
    {
        CSKernel = computeShader.FindKernel("CSMain");

        if( DebugRender)
        {
            PointMaterial = new Material(PointShader);
            PointMaterial.SetVector("_worldPos", transform.position);
        }

        InitializeBuffers();
    }
     

	void OnRenderObject()
    {
        if( DebugRender)
        {
            Dispatch();
            PointMaterial.SetPass(0);
            PointMaterial.SetColor("col", Color.red);

            Graphics.DrawProcedural(MeshTopology.Points, VertCount);
        }
    }




    private void OnDisable()
    {
        ReleaseBuffers();
    }
}
