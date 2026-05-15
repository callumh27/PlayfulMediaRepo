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
    [HideInInspector] public EarthTimelinePoint currentTimelineToEdit;
    [SerializeReference] public EditableTimelinePoint editableTimeLine;

    [Header("Properties")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask earthLayer;
    
    public Material earthMaterial;
    public int renderTextureSize = 256;

    

    // Update is called once per frame
    void Update()
    {
        if (editableTimeLine != null)
        {
            earthMaterial.SetTexture("_TectonicTexture", editableTimeLine.tectonicMap);
            earthMaterial.SetTexture("_HeightmapTexture", editableTimeLine.heightMap);
            earthMaterial.SetTexture("_PlateColourLookupTexture", editableTimeLine.plateColourLookup);
            if (currentMode == EarthEditorState.Tectonics)
            {
                if (editableTimeLine.tectonicMap == null)
                {
                    editableTimeLine.GenerateTectonicTexture();
                }
                
            }
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
                    editableTimeLine.LandPaint(radius, strength, falloff, hit.point, true);
                    
                }
                else if (currentMode == EarthEditorState.Tectonics)
                {
                    editableTimeLine.TectonicPaint(radius, plateIDToPaint, hit.point);

                }

            }
            else if (Mouse.current.rightButton.IsPressed())
            {
                if (currentMode == EarthEditorState.Tectonics) return;
                editableTimeLine.LandPaint(radius, strength, falloff, hit.point, false);
            }


        }

    }

    



    private void OnDrawGizmos()
    {
        if (currentMode == EarthEditorState.None) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);

        Gizmos.DrawSphere(brushPosition, radius);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(brushPosition, radius);


    }

    public void GenerateTimelinePoint()
    {
        editableTimeLine = new EditableTimelinePoint();

    }

    public void LoadTimeLinePoint()
    {
        if (currentTimelineToEdit != null)
        {
            editableTimeLine = new EditableTimelinePoint(currentTimelineToEdit.tectonicMap, currentTimelineToEdit.heightMap);
        }
    }

    public void RemoveTimeLinePoint()
    {
        editableTimeLine = null;
    }

    public void SaveTimelinePoint()
    {
        currentTimelineToEdit = ScriptableObject.CreateInstance<EarthTimelinePoint>();

        //currentTimelineToEdit.heightMap = editableTimeLine.heightMap;
        //currentTimelineToEdit.tectonicMap = editableTimeLine.tectonicMap;

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

        editableTimeLine = null;
    }

    

    

}

[CustomEditor(typeof(EarthEditor))]
public class EarthEditorInspector : Editor
{


    EarthEditor earthEditor;


    bool showHeightData = false;
    bool showTectonicData = false;


    override public void OnInspectorGUI()
    {
        //serializedObject.Update();

        earthEditor = target as EarthEditor;
        DrawDefaultInspector();

        


        if (earthEditor.editableTimeLine == null)
        {
           
            earthEditor.currentTimelineToEdit = EditorGUILayout.ObjectField("Timeline Point to Load: ", earthEditor.currentTimelineToEdit, typeof(EarthTimelinePoint), false) as EarthTimelinePoint;
            if (GUILayout.Button("New Timeline Point"))
            {
                earthEditor.GenerateTimelinePoint();
            }
            if (GUILayout.Button("Load Timeline Point"))
            {
                earthEditor.LoadTimeLinePoint();
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
                    earthEditor.plateIDToPaint = EditorGUILayout.IntSlider("Plate To Paint:", earthEditor.plateIDToPaint, 0, earthEditor.editableTimeLine.tectonicPlates.Count - 1);
                    // TODO: need a fold out of the currently selected plate
                    break;
            }
            //showTectonicData = EditorGUILayout.Foldout(showTectonicData, "Tectonic Data");
            //if (showTectonicData)
            //{
            //    EditorList.Show(serializedObject.FindProperty("tectonicPlates"));
            //}


            if (GUILayout.Button("Save Timeline Point"))
            {
                //earthEditor.currentTimelineToEdit.SaveToAsset();
            }
            if (GUILayout.Button("Discard Timeline Point"))
            {
                earthEditor.RemoveTimeLinePoint();
            }


        }
            
        
        

    }
}