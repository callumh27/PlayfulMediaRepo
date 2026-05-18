using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// need to bake the plate rotations between these two timeline points
// calculate the centre point of each plate and then calculate the rotation on the surface of the sphere to get from 1 to 2
public class TimelineTransition
{
    public EarthTimelinePoint point1;
    public EarthTimelinePoint point2;

    public RenderTexture bakedHeightMap1;
    public RenderTexture bakedHeightMap2;




    public void Bake()
    {
        bakedHeightMap1 = ConvertToRenderTexture(point1.heightMap);
        bakedHeightMap2 = ConvertToRenderTexture(point2.heightMap);
        
    }

    RenderTexture ConvertToRenderTexture(Texture3D source)
    {
        RenderTexture renderTexture = new RenderTexture(source.width, source.height, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8_SNorm;
        renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        renderTexture.volumeDepth = source.depth;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.Create();
        AssetDatabase.CreateAsset(renderTexture, CreateCorrectPathName("Assets/Pre-Compute/Cache/", "BakedTexture"));

        Graphics.CopyTexture(source, renderTexture);
        return renderTexture;
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
   

}
