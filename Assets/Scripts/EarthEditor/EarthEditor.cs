using System.IO;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
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
        if (currentTimelineToEdit != null)
        {
            earthMaterial.SetTexture("_TectonicTexture", currentTimelineToEdit.tectonicData.editableTectonicTexture);
            earthMaterial.SetTexture("_LandmassTexture", currentTimelineToEdit.heightData.editableHeightTexture);
            earthMaterial.SetTexture("_PlateColourLookupTexture", currentTimelineToEdit.tectonicData.plateColourLookup);
        }
        
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
                    paintCompute.SetTexture(0, "SphereTexture", currentTimelineToEdit.heightData.heightTexture);
                    paintCompute.SetFloat("planetRadius", 20);
                    paintCompute.SetInt("textureSize", renderTextureSize);
                    paintCompute.SetInt("addOrSubtract", 1);
                    paintCompute.Dispatch(0, currentTimelineToEdit.heightData.heightTexture.width / 8, currentTimelineToEdit.heightData.heightTexture.height / 8, currentTimelineToEdit.heightData.heightTexture.depth / 8);
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
                paintCompute.SetTexture(0, "SphereTexture", currentTimelineToEdit.heightData.heightTexture);
                paintCompute.SetFloat("planetRadius", 20);
                paintCompute.SetInt("textureSize", renderTextureSize);
                paintCompute.SetInt("addOrSubtract", -1);
                paintCompute.Dispatch(0, currentTimelineToEdit.heightData.heightTexture.width / 8, currentTimelineToEdit.heightData.heightTexture.height / 8, currentTimelineToEdit.heightData.heightTexture.depth / 8);
            }


        }

    }

    public void GenerateTimelinePoint()
    {
        currentTimelineToEdit = ScriptableObject.CreateInstance<EarthTimelinePoint>();
        int fileNumber = 0;
        string fileName;
        string fullPath;

        do
        {
            fileName = $"NewTimeLinePoint_{fileNumber}.asset";
            fullPath = "Assets/Pre-Compute/EarthTimelinePoints/" + fileName;
            fileNumber++;
        }
        while (File.Exists(fullPath));
        AssetDatabase.CreateAsset(currentTimelineToEdit, fullPath);
        AssetDatabase.SaveAssets();

        currentTimelineToEdit.Initialise();
    }



    private void OnDrawGizmos()
    {
        if (currentMode == EarthEditorState.None) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);

        Gizmos.DrawSphere(brushPosition, radius);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(brushPosition, radius);


    }

    

    public void SaveTimelinePoint()
    {

    }

    

    

}

[CustomEditor(typeof(EarthEditor))]
public class EarthEditorInspector : Editor
{

    enum displayFieldType { DisplayAsAutomaticFields, DisplayAsCustomizableGUIFields };
    displayFieldType DisplayFieldType;

    SerializedObject GetTarget;
    SerializedProperty ThisList;
    int ListSize;

    private SerializedProperty currentTimelinePoint;

    bool showHeightData = false;
    bool showTectonicData = false;


    void OnEnable()
    {
        currentTimelinePoint = serializedObject.FindProperty("currentTimelineToEdit");
    }

    override public void OnInspectorGUI()
    {
        serializedObject.Update();

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
                    earthEditor.plateIDToPaint = EditorGUILayout.IntSlider("Plate To Paint:", earthEditor.plateIDToPaint, 0, earthEditor.currentTimelineToEdit.tectonicData.tectonicPlates.Count - 1);
                    // TODO: need a fold out of the currently selected plate
                    break;
            }
            showTectonicData = EditorGUILayout.Foldout(showTectonicData, "Tectonic Data");
            if (showTectonicData)
            {
                if (currentTimelinePoint.objectReferenceValue != null)
                {
                    SerializedObject timelinePointObject = new SerializedObject(currentTimelinePoint.objectReferenceValue);
                    timelinePointObject.Update();

                    SerializedProperty tectonicData = timelinePointObject.FindProperty("tectonicData");
                    SerializedObject tectonicDataObject = new SerializedObject(tectonicData.objectReferenceValue);
                    tectonicDataObject.Update();

                    SerializedProperty tectonicPlatesList = tectonicDataObject.FindProperty("tectonicPlates");

                    if (tectonicPlatesList != null && tectonicPlatesList.isArray)
                    {
                        EditorGUILayout.LabelField("Tectonic Plates", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;

                        tectonicPlatesList.arraySize = EditorGUILayout.IntField("Plate Count", tectonicPlatesList.arraySize);
                        EditorGUILayout.Space();

                        for (int i = 0; i < tectonicPlatesList.arraySize; i++)
                        {
                            SerializedProperty tectonicPlateProperty = tectonicPlatesList.GetArrayElementAtIndex(i);

                            EditorGUILayout.BeginVertical("helpbox");
                            EditorGUILayout.LabelField($"Plate #{i}", EditorStyles.miniBoldLabel);

                            SerializedProperty nameProperty = tectonicPlateProperty.FindPropertyRelative("plateName");
                            SerializedProperty oceanicProperty = tectonicPlateProperty.FindPropertyRelative("isOceanic");
                            SerializedProperty colourProperty = tectonicPlateProperty.FindPropertyRelative("plateColour");

                            if (nameProperty != null) EditorGUILayout.PropertyField(nameProperty);
                            if (oceanicProperty != null) EditorGUILayout.PropertyField(oceanicProperty);
                            if (colourProperty != null) EditorGUILayout.PropertyField(colourProperty);

                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space(2);
                        }

                        EditorGUI.indentLevel--;

                    }
                    tectonicDataObject.ApplyModifiedProperties();




                }
                serializedObject.ApplyModifiedProperties();


            }


            if (GUILayout.Button("Save Timeline Point"))
            {
                earthEditor.currentTimelineToEdit.SaveToAsset();
            }


        }
            
        
        

    }
}