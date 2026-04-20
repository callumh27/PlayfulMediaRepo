
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

    public Vector3 position = Vector3.zero;

    public RenderTexture renderTexture;
    public Texture3D texture;

    private void Update()
    {
        if (!isEnabled) return;

       


        Vector3 mouseScreenPosition = new Vector3(Mouse.current.position.x.ReadValue(), Mouse.current.position.y.ReadValue(), 0);

        
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, earthLayer))
        {
            position = hit.point;
            if (Mouse.current.leftButton.IsPressed())
            {
                float rSquared = radius * radius;

                for (int u = (int)(hit.point.x - radius); u < (int)(hit.point.x + radius) + 1; u++)
                {
                    for (int v = (int)(hit.point.y - radius); v < (int)(hit.point.y + radius) + 1; v++)
                    {
                        for (int w = (int)(hit.point.z - radius); w < (int)(hit.point.z + radius) + 1; w++)
                        {
                            if ((hit.point.x - u) * (hit.point.x - u) + (hit.point.y - v) * (hit.point.y - v) + (hit.point.z - w) * (hit.point.z - w) < rSquared) {
                                Vector3 convertedWorldPos = new Vector3(u - 0.5f, v - 0.5f, w - 0.5f) / (renderTextureSize - 1.0f);
                                Vector3 texturePos = convertedWorldPos * 10;
                                texture.SetPixel((int)texturePos.x, (int)texturePos.y, (int)texturePos.z, new Color(0.0f, 0.0f, 0.0f, 1.0f));
                            }
                        }
                    }
                }
                texture.Apply();
            }
            
                        
        }


    }

    public void CreateNewRenderTexture()
    {
       
        texture = new Texture3D(renderTextureSize, renderTextureSize, renderTextureSize, TextureFormat.RGBA32, false);
        

    }

    public void SaveRenderTexture()
    {
        RenderTexture newRenderTexture = new RenderTexture(renderTextureSize, renderTextureSize, 0);
        newRenderTexture.enableRandomWrite = true;
        newRenderTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_UInt;
        newRenderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        newRenderTexture.volumeDepth = renderTextureSize;
        newRenderTexture.filterMode = FilterMode.Point;
        newRenderTexture.Create();
        renderTexture = newRenderTexture;

        Graphics.CopyTexture(texture, newRenderTexture);
        AssetDatabase.CreateAsset(renderTexture, "Assets/" + "EarthRenderTexture" + ".asset");

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
