
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;


public class EarthLandPainter : MonoBehaviour
{
    public bool isEnabled;

    [Header("Controls")]
    [Range(0.1f, 20)]
    public float radius = 5f;
    [Range(0f, 1f)]
    public float strength = 1f;

    [Header("Properties")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask earthLayer;
    public int renderTextureSize = 256;
    public Material earthMaterial;

    public Vector3 position = Vector3.zero;

    public RenderTexture renderTexture;
    public Texture3D texture;

    public ComputeShader compute;
    public ComputeShader paintCompute;

    private void Update()
    {
        if (!isEnabled) return;

       earthMaterial.SetTexture("_LandmassTexture", renderTexture);


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
                paintCompute.SetVector("currentBrushPosition", hit.point);
                paintCompute.SetTexture(0, "SphereTexture", renderTexture);
                paintCompute.SetFloat("planetRadius", 20);
                paintCompute.SetInt("textureSize", renderTextureSize);
                paintCompute.SetInt("addOrSubtract", 1);
                paintCompute.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, renderTexture.volumeDepth / 8);

            }
            else if (Mouse.current.rightButton.IsPressed())
            {
                paintCompute.SetFloat("currentBrushRadius", radius);
                paintCompute.SetFloat("brushStrength", strength);
                paintCompute.SetVector("currentBrushPosition", hit.point);
                paintCompute.SetTexture(0, "SphereTexture", renderTexture);
                paintCompute.SetFloat("planetRadius", 20);
                paintCompute.SetInt("textureSize", renderTextureSize);
                paintCompute.SetInt("addOrSubtract", -1);
                paintCompute.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, renderTexture.volumeDepth / 8);
            }
            
                        
        }


    }


    // generates blank 3D texture and stores it in the class
    public void CreateNewRenderTexture()
    {
       
        texture = new Texture3D(renderTextureSize, renderTextureSize, renderTextureSize, TextureFormat.RGBA32, false);
        

    }

    // converts the 3D texture into a render texture and then saves it as an asset
    public void Save3DTexture()
    {
        Graphics.CopyTexture(texture, renderTexture);
        AssetDatabase.CreateAsset(renderTexture, "Assets/" + "EarthRenderTexture" + ".asset");

    }

    public void GenerateBlankSphereRenderTexture()
    {
        renderTexture = new RenderTexture(renderTextureSize, renderTextureSize, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat;
        renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        renderTexture.volumeDepth = renderTextureSize;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.Create();

        AssetDatabase.CreateAsset(renderTexture, "Assets/Pre-Compute/" + "BlankEarthRenderTexture" + ".asset");

        compute.SetTexture(0, "SphereTexture", renderTexture); // can use .FindKernel() method if using multiple kernels
        compute.SetInt("textureSize", renderTextureSize);
        compute.SetFloat("planetRadius", 10);
        compute.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, renderTexture.volumeDepth / 8);
    }

    public void SaveRenderTexture()
    {
        //Texture3D assetTexture = new Texture3D(renderTexture.width, renderTexture.height, renderTexture.volumeDepth, TextureFormat.RGBA32, false);
        //Graphics.CopyTexture(renderTexture, assetTexture);
        //AssetDatabase.CreateAsset(renderTexture, "Assets/Pre-Compute/" + "BlankEarthRenderTexture" + ".asset");
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
