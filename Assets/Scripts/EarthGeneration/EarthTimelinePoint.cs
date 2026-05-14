using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class EarthTimelinePoint: ScriptableObject
{

    public HeightData heightData;
    public TectonicData tectonicData;

    private int renderTextureSize = 256;

    public EarthTimelinePoint()
    {
        heightData = new HeightData(GenerateSphereRenderTexture());
        tectonicData = new TectonicData(GenerateSphereRenderTexture());

        
    }

    RenderTexture GenerateSphereRenderTexture()
    {
        RenderTexture renderTexture = new RenderTexture(renderTextureSize, renderTextureSize, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8_SNorm;
        renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        renderTexture.volumeDepth = renderTextureSize;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.Create();

        AssetDatabase.CreateAsset(renderTexture, "Assets/Pre-Compute/Cache/" + "BlankEarthRenderTexture" + ".asset");

        ComputeShader sphereGeneratorCompute = ComputeShaderReferences.Instance.blankSphereGenerator;

        sphereGeneratorCompute.SetTexture(0, "SphereTexture", renderTexture); // can use .FindKernel() method if using multiple kernels
        sphereGeneratorCompute.SetInt("textureSize", renderTextureSize);
        sphereGeneratorCompute.SetFloat("planetRadius", 10);
        sphereGeneratorCompute.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, renderTexture.volumeDepth / 8);

        return renderTexture;
    }


}
