using UnityEngine;

[CreateAssetMenu(fileName = "ComputeShaderReferences", menuName = "Reference Objects/ComputeShaderReferences")]
public class ComputeShaderReferences : ScriptableObject
{
    private static ComputeShaderReferences _instance;
    public static ComputeShaderReferences Instance
    {
        get { return _instance; }
    }

    [Header("Compute Shaders")]
    public ComputeShader blankSphereGenerator;
}
