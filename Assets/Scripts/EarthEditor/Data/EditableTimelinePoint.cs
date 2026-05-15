using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Rendering;

[System.Serializable]
public class EditableTimelinePoint
{
    public RenderTexture tectonicMap;
    public RenderTexture heightMap;
    public Texture2D plateColourLookup;

    public List<TectonicPlate> tectonicPlates = new List<TectonicPlate>();
    private Vector4[] tectonicPoints;

    private int renderTextureSize = 256;
    private int amountOfTectonicPoints = 256;
    private ComputeBuffer tectonicPointBuffer;

    public EditableTimelinePoint(Texture3D tectonicMapAsset, Texture3D heightMapAsset)
    {
        // convert the textures to RenderTextures
    }

    public EditableTimelinePoint()
    {
        RenderTexture rt = GenerateRenderTexture(false);
        GenerateSphereRenderTexture(ref rt);
        heightMap = rt;


    }

    public void GenerateTectonicTexture()
    {
        RenderTexture rt = GenerateRenderTexture(true);
        GenerateSphereRenderTexture(ref rt);
        tectonicMap = rt;
        tectonicPoints = GenerateTectonicPoints();

        tectonicPointBuffer = new ComputeBuffer(tectonicPoints.Length, sizeof(float) * 4);
        tectonicPointBuffer.SetData(tectonicPoints);

        UpdateTectonicLookupTexture();
    }

    public void LandPaint(float radius, float strength, float falloff, Vector3 position, bool addOrSubtract)
    {

        ComputeShader paintCompute = ComputeShaderReferences.Instance.heightMapPainter;
        paintCompute.SetFloat("currentBrushRadius", radius);
        paintCompute.SetFloat("brushStrength", strength);
        paintCompute.SetFloat("falloff", falloff);
        paintCompute.SetVector("currentBrushPosition", position);
        paintCompute.SetTexture(0, "SphereTexture", heightMap);
        paintCompute.SetFloat("planetRadius", 20);
        paintCompute.SetInt("textureSize", renderTextureSize);
        paintCompute.SetInt("addOrSubtract", addOrSubtract ? 1 : -1);
        paintCompute.Dispatch(0, heightMap.width / 8, heightMap.height / 8, heightMap.volumeDepth / 8);
    }

    public void TectonicPaint(float radius, int plateID, Vector3 position)
    {
        if (!tectonicPointBuffer.IsValid())
        {
            tectonicPointBuffer = new ComputeBuffer(tectonicPoints.Length, sizeof(float) * 4);
            tectonicPointBuffer.SetData(tectonicPoints);
        }

        ComputeShader tectonicPainterCompute = ComputeShaderReferences.Instance.tectonicPainterCompute;

        tectonicPainterCompute.SetFloat("currentBrushRadius", radius);
        tectonicPainterCompute.SetInt("currentPaintIndex", plateID);

        tectonicPainterCompute.SetVector("currentBrushPosition", position);
        tectonicPainterCompute.SetBuffer(0, "tectonicPoints", tectonicPointBuffer);
        tectonicPainterCompute.Dispatch(0, tectonicPoints.Length / 8, 1, 1);

        UpdateTectonicLookupTexture();
    }

    public void UpdateTectonicLookupTexture()
    {
        if (!tectonicPointBuffer.IsValid()) return;

        if (plateColourLookup == null)
        {
            plateColourLookup = new Texture2D(25, 1);
            plateColourLookup.filterMode = FilterMode.Point;
            plateColourLookup.anisoLevel = 0;
        }

        Vector4[] tectonicColours = new Vector4[tectonicPlates.Count];
        for (int i = 0; i < tectonicPlates.Count; i++)
        {
            tectonicColours[i] = new Vector4(tectonicPlates[i].plateColour.r, tectonicPlates[i].plateColour.g, tectonicPlates[i].plateColour.b, 0);
            plateColourLookup.SetPixel(i, 0, tectonicPlates[i].plateColour);
        }
        plateColourLookup.Apply();

        tectonicPointBuffer.GetData(tectonicPoints);
        ComputeShader tectonicCompute = ComputeShaderReferences.Instance.tectonicTextureGenerator;
        tectonicCompute.SetTexture(0, "TectonicLookupTexture", tectonicMap);
        tectonicCompute.SetVectorArray("tectonicPoints", tectonicPoints);
        tectonicCompute.SetVectorArray("tectonicColours", tectonicColours);
        tectonicCompute.SetFloat("planetRadius", 40);
        tectonicCompute.SetInt("textureSize", 256);
        tectonicCompute.SetInt("amountOfPlates", tectonicPlates.Count);


        tectonicCompute.Dispatch(0, tectonicMap.width / 8, tectonicMap.height / 8, tectonicMap.volumeDepth / 8);
        // possibly make this a parameter
        //earthMaterial.SetTexture("_TectonicTexture", editableTectonicTexture);
        Debug.Log("oainbted");
    }

    // generate a certain amount of equally spaced points and randomly skew them
    public Vector4[] GenerateTectonicPoints()
    {
        Vector4[] tectonicPoints = new Vector4[amountOfTectonicPoints];
        float phi = Mathf.PI * (3f - Mathf.Sqrt(5f));

        for (int i = 0; i < amountOfTectonicPoints; i++)
        {
            float y = (1f - (i / (float)(amountOfTectonicPoints - 1)) * 2f) + UnityEngine.Random.Range(-1.0f, 1.0f);
            float r = Mathf.Sqrt(1f - y * y);
            float theta = phi * i;

            float x = Mathf.Cos(theta) * r;
            float z = Mathf.Sin(theta) * r;
            tectonicPoints[i] = new Vector4(x, y, z, 0) * 20f;
        }

        return tectonicPoints;


    }



    public void Save()
    {
        // convert to EarthTimelinePoint

        // should also check if the asset exists and if so just replace it/edit it
        EarthTimelinePoint newTimelinePoint = ScriptableObject.CreateInstance<EarthTimelinePoint>();
        AssetDatabase.CreateAsset(newTimelinePoint, CreateCorrectPathName("Assets/Pre-Compute/EarthTimelinePoints", "TimelinePoint"));
        AssetDatabase.SaveAssets();

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

    RenderTexture GenerateRenderTexture(bool tec)
    {
        RenderTexture renderTexture = new RenderTexture(renderTextureSize, renderTextureSize, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.graphicsFormat = tec ? UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat : UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8_SNorm;
        renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        renderTexture.volumeDepth = renderTextureSize;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.Create();

        return renderTexture;
    }

    void GenerateSphereRenderTexture(ref RenderTexture renderTexture)
    {
        
        AssetDatabase.CreateAsset(renderTexture, CreateCorrectPathName("Assets/Pre-Compute/Cache/", "BlankEarthRenderTexture"));

        ComputeShader sphereGeneratorCompute = ComputeShaderReferences.Instance.blankSphereGenerator;

        sphereGeneratorCompute.SetTexture(0, "SphereTexture", renderTexture); // can use .FindKernel() method if using multiple kernels
        sphereGeneratorCompute.SetInt("textureSize", renderTextureSize);
        sphereGeneratorCompute.SetFloat("planetRadius", 10);
        sphereGeneratorCompute.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, renderTexture.volumeDepth / 8);

    }
}
