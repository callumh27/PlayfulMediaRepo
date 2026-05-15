using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using static UnityEditor.Rendering.CameraUI;

public class EarthTimelinePoint: ScriptableObject
{

    public HeightData heightData;
    public TectonicData tectonicData;

    private int renderTextureSize = 256;

    public void Initialise()
    {
        heightData = ScriptableObject.CreateInstance<HeightData>();
        tectonicData = ScriptableObject.CreateInstance<TectonicData>();

        

        AssetDatabase.CreateAsset(heightData, CreateCorrectPathName("Assets/Pre-Compute/EarthTimelinePoints/HeightObjects/", "HeightData"));
        AssetDatabase.CreateAsset(tectonicData, CreateCorrectPathName("Assets/Pre-Compute/EarthTimelinePoints/TectonicObjects/", "TectonicData"));
        AssetDatabase.SaveAssets();

        heightData.Initialise(GenerateSphereRenderTexture());
        tectonicData.Initialise(GenerateSphereRenderTexture());
        
    }

    public void SaveToAsset()
    {
        //check and destroy previous textures
        if (heightData.heightTexture != null)
        {
            Destroy(heightData.heightTexture);
        }
        if (tectonicData.tectonicTexture != null)
        {
            Destroy(tectonicData.tectonicTexture);
        }

        heightData.heightTexture = ConvertToAsset(heightData.editableHeightTexture);
        tectonicData.tectonicTexture = ConvertToAsset(tectonicData.editableTectonicTexture);
    }

    string CreateCorrectPathName(string path, string assetName)
    {
        int fileNumber = 0;
        string fileName;
        string fullPath;

        do
        {
            fileName = $"{assetName}_{fileNumber}.asset";
            fullPath = path + fileName;
            fileNumber++;
        }
        while (File.Exists(fullPath));

        return fullPath;
    }

    // function needed because render textures are not serialisable
    // found at https://discussions.unity.com/t/save-a-3d-render-texture-to-file/863563/4
    Texture3D ConvertToAsset(RenderTexture renderTexture, int heightOrTectonic = 0)
    {
        int width = renderTexture.width;
        int height = renderTexture.height;
        int depth = renderTexture.volumeDepth;
        var a = new NativeArray<byte>((width * height * depth) * 2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        Texture3D output = new Texture3D(width, height, depth, renderTexture.graphicsFormat, TextureCreationFlags.None);
        AsyncGPUReadback.RequestIntoNativeArray(ref a, renderTexture, 0, (_) =>
        {
            output.SetPixelData(a, 0);
            output.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            if (heightOrTectonic == 0)
            {
                AssetDatabase.CreateAsset(output, CreateCorrectPathName("Assets/Pre-Compute/HeightTextures/", "HeightMap"));
            }
            else
            {
                AssetDatabase.CreateAsset(output, CreateCorrectPathName("Assets/Pre-Compute/TectonicTextures/", "TectonicMap"));
            }

                AssetDatabase.SaveAssetIfDirty(output);
            a.Dispose();
            //renderTexture.Release();

        });
        return output;
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


        AssetDatabase.CreateAsset(renderTexture, CreateCorrectPathName("Assets/Pre-Compute/Cache/", "BlankEarthRenderTexture"));

        //ComputeShader sphereGeneratorCompute = ComputeShaderReferences.Instance.blankSphereGenerator;

        ComputeShaderReferences.Instance.blankSphereGenerator.SetTexture(0, "SphereTexture", renderTexture); // can use .FindKernel() method if using multiple kernels
        ComputeShaderReferences.Instance.blankSphereGenerator.SetInt("textureSize", renderTextureSize);
        ComputeShaderReferences.Instance.blankSphereGenerator.SetFloat("planetRadius", 10);
        ComputeShaderReferences.Instance.blankSphereGenerator.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, renderTexture.volumeDepth / 8);

        return renderTexture;
    }


}
