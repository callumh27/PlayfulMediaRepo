using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;

[Serializable]
public struct TectonicPlate
{
    public int plateID;
    public Vector3 plateDirection;
    public bool isOceanic;
}

public class TectonicEditor : MonoBehaviour
{
    public bool isEnabled;
    public Vector3 position = Vector3.zero;

    [Header("Controls")]
    [Range(0.1f, 20)]
    public float radius = 5f;


    [Header("Current Map Settings")]
    public int amountOfTectonicPoints = 256;
    public List<TectonicPlate> tectonicPlates = new List<TectonicPlate>(0);
    public string mapYear;
    public RenderTexture renderTexture;

    public int currentTectonicIndex = 0;

    [Header("Prerequisites")]
    public ComputeShader tectonicCompute;
    public ComputeShader tectonicPainterCompute;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask earthLayer;
    public Material earthMaterial;
    

    private ComputeBuffer tectonicPointBuffer;
    private Vector4[] tectonicPoints;
    private int renderTextureSize = 256;
    

    void Update()
    {
        if(renderTexture == null) return;
        if(!isEnabled) return;


        Vector3 mouseScreenPosition = new Vector3(Mouse.current.position.x.ReadValue(), Mouse.current.position.y.ReadValue(), 0);
        
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, earthLayer))
        {
            //somehow need to get points in the tectonicPoints array that are within the radius
            //of the ray hit point
            position = hit.point;
            if (Mouse.current.leftButton.IsPressed()){

                tectonicPainterCompute.SetFloat("currentBrushRadius", radius);
                tectonicPainterCompute.SetFloat("currentPaintIndex", (float)currentTectonicIndex / (float)tectonicPlates.Count);
                tectonicPainterCompute.SetVector("currentBrushPosition", hit.point);
                tectonicPainterCompute.SetBuffer(0, "tectonicPoints", tectonicPointBuffer);
                tectonicPainterCompute.Dispatch(0, tectonicPoints.Length / 8, 1, 1);

                UpdateTectonicLookupTexture();

            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                // this allows for painting probably but it also means that we cant see
                // paint changes until after a brush stroke, peak
                //tectonicPointBuffer.Release();
            }
                
                            
            }


        

    }

    // generate a certain amount of equally spaced points and randomly skew them
    public void GenerateTectonicPoints()
    {
        tectonicPoints = new Vector4[amountOfTectonicPoints];
        float phi = Mathf.PI * (3f - Mathf.Sqrt(5f));

        for(int i = 0; i < amountOfTectonicPoints; i++)
        {
            float y = 1f - (i / (float)(amountOfTectonicPoints - 1)) * 2f;
            float r = Mathf.Sqrt(1f - y * y);
            float theta = phi * i;

            float x = Mathf.Cos(theta) * r;
            float z = Mathf.Sin(theta) * r;
            tectonicPoints[i] = new Vector4(x,y,z,0) * 20f;
        }


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

        GenerateTectonicPoints();

        tectonicPointBuffer = new ComputeBuffer(tectonicPoints.Length, sizeof(float)*4);
        tectonicPointBuffer.SetData(tectonicPoints);

        UpdateTectonicLookupTexture();


    }
    public void SaveTectonicMap()
    {
        
    }

    public void UpdateTectonicLookupTexture()
    {
        if(!tectonicPointBuffer.IsValid()) return;
        
        tectonicPointBuffer.GetData(tectonicPoints);
        tectonicCompute.SetTexture(0, "TectonicLookupTexture", renderTexture);
        tectonicCompute.SetVectorArray("tectonicPoints", tectonicPoints);

        tectonicCompute.SetFloat("planetRadius", 40);
        tectonicCompute.SetInt("textureSize", renderTextureSize);

        tectonicCompute.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, renderTexture.volumeDepth / 8);
        earthMaterial.SetTexture("_TectonicTexture", renderTexture);
    }

    private void OnDrawGizmos()
    {
        if (!isEnabled) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);

        Gizmos.DrawSphere(position, radius);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(position, radius);


    }

}

