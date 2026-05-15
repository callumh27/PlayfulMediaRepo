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

    //public HeightData heightData;
    //public TectonicData tectonicData;

    public Texture3D heightMap;
    public Texture3D tectonicMap;

    public List<TectonicPlate> tectonicPlates = new List<TectonicPlate>();
    public Vector4[] tectonicPoints;

    private int renderTextureSize = 256;


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

    


}
