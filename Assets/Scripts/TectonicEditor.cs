using System.Collections.Generic;
using UnityEngine;


public class TectonicEditor : MonoBehaviour
{
    


    [Header("Current Map Settings")]
    public List<TectonicPlate> tectonicPlates = new List<TectonicPlate>();
    public string mapYear;
    public RenderTexture renderTexture;
    

    [Header("Prerequisites")]
    public ComputeShader tectonicCompute;
    public ComputeShader tectonicPainterCompute;


    private Vector4[] tectonicPoints;
    private int renderTextureSize = 256;

    void Update()
    {
        if(renderTexture == null) return;

        tectonicCompute.SetVectorArray("tectonicPoints", tectonicPoints);
        tectonicCompute.SetVectorArray("tectonicColours", tectonicPoints);
        tectonicCompute.SetTexture(0, "TectonicMapTexture", renderTexture);

    }
    public void GenerateRenderTexture()
    {

        //generate tectonic points and plates here

        renderTexture = new RenderTexture(renderTextureSize, renderTextureSize, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat;
        renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        renderTexture.volumeDepth = renderTextureSize;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.Create();


        tectonicCompute.SetTexture(0, "TectonicMapTexture", renderTexture);

    }
    public void SaveTectonicMap()
    {
        
    }
}
