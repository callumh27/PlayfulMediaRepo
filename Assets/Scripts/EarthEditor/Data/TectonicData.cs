using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TectonicPlate
{
    public bool isOceanic;
    public Color plateColour;
    public string plateName;
}

public class TectonicData
{

    public Texture3D tectonicTexture;
    public Texture2D plateColourLookup;
    public Vector4[] tectonicPoints;
    public List<TectonicPlate> tectonicPlates = new List<TectonicPlate>();

    private int amountOfTectonicPoints = 256;
    private ComputeBuffer tectonicPointBuffer;


    public RenderTexture editableTectonicTexture;

    public ComputeShader tectonicPainterCompute;

    public void Initialise(RenderTexture blankSphereTexture)
    {

        editableTectonicTexture = blankSphereTexture;

        plateColourLookup = new Texture2D(25, 1);

        tectonicPoints = GenerateTectonicPoints();

        tectonicPointBuffer = new ComputeBuffer(tectonicPoints.Length, sizeof(float) * 4);
        tectonicPointBuffer.SetData(tectonicPoints);

        UpdateTectonicLookupTexture();
    }

    public void UpdateTectonicLookupTexture()
    {
        if (!tectonicPointBuffer.IsValid()) return;

        if (plateColourLookup == null)
        {
            plateColourLookup = new Texture2D(25, 1);
        }

        Vector4[] tectonicColours = new Vector4[tectonicPlates.Count];
        for (int i = 0; i < tectonicPlates.Count; i++)
        {
            tectonicColours[i] = new Vector4(tectonicPlates[i].plateColour.r, tectonicPlates[i].plateColour.g, tectonicPlates[i].plateColour.b, 0);
            plateColourLookup.SetPixel(i, 0, tectonicPlates[i].plateColour);
        }



        tectonicPointBuffer.GetData(tectonicPoints);
        ComputeShader tectonicCompute = ComputeShaderReferences.Instance.tectonicTextureGenerator;
        tectonicCompute.SetTexture(0, "TectonicLookupTexture", editableTectonicTexture);
        tectonicCompute.SetVectorArray("tectonicPoints", tectonicPoints);
        tectonicCompute.SetVectorArray("tectonicColours", tectonicColours);
        tectonicCompute.SetFloat("planetRadius", 40);
        tectonicCompute.SetInt("textureSize", 256);
        tectonicCompute.SetInt("amountOfPlates", tectonicPlates.Count);


        tectonicCompute.Dispatch(0, editableTectonicTexture.width / 8, editableTectonicTexture.height / 8, editableTectonicTexture.volumeDepth / 8);
        // possibly make this a parameter
        //earthMaterial.SetTexture("_TectonicTexture", editableTectonicTexture);
        
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


    public void Paint(float radius, int plateID, Vector3 position)
    {
        if (!tectonicPointBuffer.IsValid())
        {
            tectonicPointBuffer = new ComputeBuffer(tectonicPoints.Length, sizeof(float) * 4);
            tectonicPointBuffer.SetData(tectonicPoints);
        }

        ComputeShader tectonicPainterCompute = Resources.Load<ComputeShader>("ComputeShaders/CS_TectonicPainter");

        tectonicPainterCompute.SetFloat("currentBrushRadius", radius);
        tectonicPainterCompute.SetInt("currentPaintIndex", plateID);

        tectonicPainterCompute.SetVector("currentBrushPosition", position);
        tectonicPainterCompute.SetBuffer(0, "tectonicPoints", tectonicPointBuffer);
        tectonicPainterCompute.Dispatch(0, tectonicPoints.Length / 8, 1, 1);

        UpdateTectonicLookupTexture();
    }
}
