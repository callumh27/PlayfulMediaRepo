using UnityEngine;

public class HeightData : ScriptableObject
{
    [HideInInspector] public Texture3D tectonicTexture;

    [HideInInspector] public RenderTexture editableTectonicTexture;

    public HeightData(RenderTexture blankSphereTexture)
    {

    }
}
