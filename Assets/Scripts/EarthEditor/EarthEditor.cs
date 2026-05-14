using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public enum EarthEditorState
{
    None,
    Landmass,
    Tectonics
}

public class EarthEditor : MonoBehaviour
{
    public EarthEditorState currentMode;


    

    [HideInInspector] public float radius = 5f;
    [HideInInspector] public float strength = 1f;
    [HideInInspector] public float falloff = 0.4f;
    [HideInInspector] public int plateIDToPaint = 0;

    private Vector3 brushPosition = Vector3.zero;
    public EarthTimelinePoint currentTimelineToEdit;

    [Header("Properties")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask earthLayer;
    
    public Material earthMaterial;
    public int renderTextureSize = 256;

    [Header("Compute Shader References")]
    public ComputeShader compute;
    public ComputeShader paintCompute;
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        earthMaterial.SetTexture("_TectonicTexture", currentTimelineToEdit.tectonicData.tectonicTexture);
        earthMaterial.SetTexture("_LandmassTexture", currentTimelineToEdit.heightData.heightTexture);
        if (currentMode == EarthEditorState.Tectonics)
        {
            earthMaterial.SetInt("_ViewTectonics", 1);
        }
        else
        {
            earthMaterial.SetInt("_ViewTectonics", 0);
        }

        if (currentMode == EarthEditorState.None) return;

        Vector3 mouseScreenPosition = new Vector3(Mouse.current.position.x.ReadValue(), Mouse.current.position.y.ReadValue(), 0);

        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, earthLayer))
        {
            brushPosition = hit.point;
            if (Mouse.current.leftButton.IsPressed())
            {
                //user is currently painting on the surface of the Earth,

                // convert world space coordinates into texture space coordinates using:
                // radius of the Earth and the size of the texture
                // planetRadius/textureSize
                // 20 / 256 = 0.078125 == 1 unit

                // multiply the unit by the world position vector to get the texture position

                if (currentMode == EarthEditorState.Landmass)
                {
                    paintCompute.SetFloat("currentBrushRadius", radius);
                    paintCompute.SetFloat("brushStrength", strength);
                    paintCompute.SetFloat("falloff", falloff);
                    paintCompute.SetVector("currentBrushPosition", hit.point);
                    paintCompute.SetTexture(0, "SphereTexture", currentTimelineToEdit.landmassMap);
                    paintCompute.SetFloat("planetRadius", 20);
                    paintCompute.SetInt("textureSize", renderTextureSize);
                    paintCompute.SetInt("addOrSubtract", 1);
                    paintCompute.Dispatch(0, currentTimelineToEdit.landmassMap.width / 8, currentTimelineToEdit.landmassMap.height / 8, currentTimelineToEdit.landmassMap.volumeDepth / 8);
                }
                else if (currentMode == EarthEditorState.Tectonics)
                {

                    currentTimelineToEdit.tectonicData.Paint(radius, plateIDToPaint, hit.point);

                    
                }

                

            }
            else if (Mouse.current.rightButton.IsPressed())
            {
                if (currentMode == EarthEditorState.Tectonics) return;
                paintCompute.SetFloat("currentBrushRadius", radius);
                paintCompute.SetFloat("brushStrength", strength);
                paintCompute.SetFloat("falloff", falloff);
                paintCompute.SetVector("currentBrushPosition", hit.point);
                paintCompute.SetTexture(0, "SphereTexture", currentTimelineToEdit.landmassMap);
                paintCompute.SetFloat("planetRadius", 20);
                paintCompute.SetInt("textureSize", renderTextureSize);
                paintCompute.SetInt("addOrSubtract", -1);
                paintCompute.Dispatch(0, currentTimelineToEdit.landmassMap.width / 8, currentTimelineToEdit.landmassMap.height / 8, currentTimelineToEdit.landmassMap.volumeDepth / 8);
            }


        }

    }

    public void GenerateTimelinePoint()
    {
        currentTimelineToEdit = new EarthTimelinePoint();
    }



    private void OnDrawGizmos()
    {
        if (currentMode == EarthEditorState.None) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);

        Gizmos.DrawSphere(brushPosition, radius);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(brushPosition, radius);


    }

    // function needed because render textures are not serialisable
    // found at https://discussions.unity.com/t/save-a-3d-render-texture-to-file/863563/4
    void ConvertToAsset(RenderTexture renderTexture, int heightOrTectonic = 0)
    {
        int width = renderTexture.width;
        int height = renderTexture.height;
        int depth = renderTexture.volumeDepth;
        var a = new NativeArray<byte>((width * height * depth) * 2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        AsyncGPUReadback.RequestIntoNativeArray(ref a, renderTexture, 0, (_) =>
        {
            Texture3D output = new Texture3D(width, height, depth, renderTexture.graphicsFormat, TextureCreationFlags.None);
            output.SetPixelData(a, 0);
            output.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            if (heightOrTectonic == 0)
            {
                AssetDatabase.CreateAsset(output, "Assets/Pre-Compute/HeightTextures/" + "EarthMap.asset");
            }
            
            AssetDatabase.SaveAssetIfDirty(output);
            a.Dispose();
            //renderTexture.Release();
        });
    }

    public void SaveTimelinePoint()
    {

    }

    

    

}

[CustomEditor(typeof(EarthEditor))]
public class EarthEditorInspector : Editor
{
    override public void OnInspectorGUI()
    {
        var earthEditor = target as EarthEditor;
        DrawDefaultInspector();

        if (earthEditor.currentTimelineToEdit == null)
        {
            if (GUILayout.Button("New Timeline Point"))
            {
                earthEditor.GenerateTimelinePoint();
            }

        }
        else
        {
            switch (earthEditor.currentMode)
            {
                case EarthEditorState.None:
                    break;
                case EarthEditorState.Landmass:
                    earthEditor.radius = EditorGUILayout.Slider("Brush Radius: ", earthEditor.radius, 0, 20f);
                    earthEditor.strength = EditorGUILayout.Slider("Brush Strength: ", earthEditor.strength, 0, 1f);
                    earthEditor.falloff = EditorGUILayout.Slider("Brush Falloff: ", earthEditor.falloff, 0, 10f);
                    break;
                case EarthEditorState.Tectonics:
                    earthEditor.radius = EditorGUILayout.Slider("Brush Radius: ", earthEditor.radius, 0, 20f);
                    earthEditor.plateIDToPaint = EditorGUILayout.IntSlider("Plate To Paint:", earthEditor.plateIDToPaint, 0, earthEditor.currentTimelineToEdit.tectonicPlates.Count);
                    // TODO: need a fold out of the currently selected plate
                    break;
            }
        }
            
        
        

    }
}