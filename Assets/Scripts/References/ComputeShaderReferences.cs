using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "ComputeShaderReferences", menuName = "Reference Objects/ComputeShaderReferences")]
public class ComputeShaderReferences : ScriptableObject
{
    private static ComputeShaderReferences _instance;
    public static ComputeShaderReferences Instance
    {
        get 
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ComputeShaderReferences>("ComputeShaderReferences");

                if (_instance != null)
                {
                    // Force-trigger explicit data-binding updates on load
                    _instance.InitializeReferences();
                }
                if (_instance == null)
                {
                    Debug.LogError("asset missing");
                }
            }


            return _instance;
        }
    }

    [Header("Compute Shaders")]
    public ComputeShader blankSphereGenerator;
    public ComputeShader heightMapPainter;
    public ComputeShader tectonicTextureGenerator;
    public ComputeShader tectonicPainterCompute;
    public ComputeShader tectonicBoundariesCompute;
    public ComputeShader tectonicLookupCompute;

    void InitializeReferences()
    {
        blankSphereGenerator = Resources.Load<ComputeShader>("Earth Generation/BlankSphereTexGenerator.compute");
        heightMapPainter = Resources.Load<ComputeShader>("Earth Generation/CS_EarthPainter.compute");
        tectonicTextureGenerator = Resources.Load<ComputeShader>("Earth Generation/CS_TectonicMapTextureGenerator.compute");
        tectonicPainterCompute = Resources.Load<ComputeShader>("Earth Generation/CS_TectonicPainter.compute");
        tectonicBoundariesCompute = Resources.Load<ComputeShader>("Earth Generation/CS_PlateBoundaryCompute.compute");
        tectonicLookupCompute = Resources.Load<ComputeShader>("Earth Generation/CS_PlateColourLookupGenerator.compute");
    }
}
