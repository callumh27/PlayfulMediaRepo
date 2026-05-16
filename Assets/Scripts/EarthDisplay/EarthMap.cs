using UnityEngine;

public class EarthMap : MonoBehaviour
{

    public RenderTexture surfaceTexture;
    public Material earthMaterial;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //earthMaterial.SetTexture("_SurfaceTexture", surfaceTexture);
    }
}
