using Unity.VisualScripting;
using UnityEditor;
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
    [SerializeField] public ComputeShader blankSphereGenerator;
    [SerializeField] public ComputeShader heightMapPainter;
    [SerializeField] public ComputeShader tectonicTextureGenerator;
    [SerializeField] public ComputeShader tectonicPainterCompute;
    [SerializeField] public ComputeShader tectonicBoundariesCompute;
    [SerializeField] public ComputeShader tectonicLookupCompute;

    void InitializeReferences()
    {
        blankSphereGenerator = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/ComputeShaders/EarthGeneration/BlankSphereTexGenerator.compute");
        heightMapPainter = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/ComputeShaders/EarthGeneration/CS_EarthPainter.compute");
        tectonicTextureGenerator = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/ComputeShaders/TectonicPlates/CS_TectonicMapTextureGenerator.compute");
        tectonicPainterCompute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/ComputeShaders/TectonicPlates/CS_TectonicPainter.compute");
        tectonicBoundariesCompute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/ComputeShaders/TectonicPlates/CS_PlateBoundaryCompute.compute");
        tectonicLookupCompute = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/ComputeShaders/TectonicPlates/CS_PlateColourLookupGenerator.compute");
    }
}
