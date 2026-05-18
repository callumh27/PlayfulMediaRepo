using UnityEngine;

public class HeightData : ScriptableObject
{
     public Texture3D heightTexture;

     public RenderTexture editableHeightTexture;

    public void Initialise(RenderTexture blankSphereTexture)
    {
        editableHeightTexture = blankSphereTexture;
    }
}
