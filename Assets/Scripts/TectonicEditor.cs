using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;


public class TectonicEditor : MonoBehaviour
{
    public bool isEnabled;

    [Header("Controls")]
    [Range(0.1f, 20)]
    public float radius = 5f;
    [Range(0f, 1f)]
    public float strength = 1f;
    [Range(3f, 10f)]
    public float falloff = 4f;

    [Header("Current Map Settings")]
    public int amountOfTectonicPoints = 1024;
    public List<TectonicPlate> tectonicPlates = new List<TectonicPlate>(0);
    public string mapYear;
    public RenderTexture renderTexture;

    [Header("Current Tectonic Plate")]
    public int currentPlateIndex = 0;
    public Vector3 plateDirection;
    public bool isOceanic;

    [Header("Prerequisites")]
    public ComputeShader tectonicCompute;
    public ComputeShader tectonicPainterCompute;


    private Vector4[] tectonicPoints;
    private int renderTextureSize = 256;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask earthLayer;
    public Material earthMaterial;
    public Vector3 position = Vector3.zero;

    void Update()
    {
        if(renderTexture == null) return;
        if(!isEnabled) return;


        Vector3 mouseScreenPosition = new Vector3(Mouse.current.position.x.ReadValue(), Mouse.current.position.y.ReadValue(), 0);
        
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, earthLayer))
            {
                position = hit.point;
                if (Mouse.current.leftButton.IsPressed())
                {
                    //user is currently painting on the surface of the Earth,

                    // convert world space coordinates into texture space coordinates using:
                    // radius of the Earth and the size of the texture
                    // planetRadius/textureSize
                    // 20 / 256 = 0.078125 == 1 unit

                    // multiply the unit by the world position vector to get the texture position


                    paintCompute.SetFloat("currentBrushRadius", radius);
                    paintCompute.SetFloat("brushStrength", strength);
                    paintCompute.SetFloat("falloff", falloff);
                    paintCompute.SetVector("currentBrushPosition", hit.point);
                    paintCompute.SetTexture(0, "SphereTexture", renderTexture);
                    paintCompute.SetFloat("planetRadius", 20);
                    paintCompute.SetInt("textureSize", renderTextureSize);
                    paintCompute.SetInt("addOrSubtract", 1);
                    paintCompute.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, renderTexture.volumeDepth / 8);

                }
                
                            
            }


        

    }

    // generate a certain amount of equally generated points and randomly skew them
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
            tectonicPoints[i] = new Vector4(x,y,z,0);
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


        tectonicCompute.SetTexture(0, "TectonicMapTexture", renderTexture);

    }
    public void SaveTectonicMap()
    {
        
    }
}
