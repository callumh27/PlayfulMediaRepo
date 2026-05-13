using UnityEngine;
using UnityEditor;

public enum EarthEditorState
{
    None,
    Landmass,
    Tectonics
}

public class EarthEditor : MonoBehaviour
{
    public EarthEditorState currentMode;


    public EarthTimelinePoint currentTimelineToEdit;
    public Material earthMaterial;

    [HideInInspector] public float radius = 5f;
    [HideInInspector] public float strength = 1f;
    [HideInInspector] public float falloff = 0.4f;

    private Vector3 brushPosition = Vector3.zero;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        earthMaterial.SetTexture("_TectonicTexture", currentTimelineToEdit.tectonicMap);
        earthMaterial.SetTexture("_LandmassTexture", currentTimelineToEdit.landmassMap);
        if (currentMode == EarthEditorState.Tectonics)
        {
            earthMaterial.SetInt("_ViewTectonics", 1);
        }
        else
        {
            earthMaterial.SetInt("_ViewTectonics", 0);
        }

    }
}

[CustomEditor(typeof(EarthEditor))]
public class EarthEditorInspector : Editor
{
    override public void OnInspectorGUI()
    {
        var earthEditor = target as EarthEditor;
        DrawDefaultInspector();
        switch (earthEditor.currentMode){
            case EarthEditorState.None:
                break;
            case EarthEditorState.Landmass:
                earthEditor.radius = EditorGUILayout.Slider("Brush Radius: ", earthEditor.radius, 0, 20f);
                earthEditor.strength = EditorGUILayout.Slider("Brush Strength: ", earthEditor.strength, 0, 1f);
                earthEditor.falloff = EditorGUILayout.Slider("Brush Falloff: ", earthEditor.falloff, 0, 10f);
                break;
            case EarthEditorState.Tectonics:
                earthEditor.radius = EditorGUILayout.Slider("Brush Radius: ", earthEditor.radius, 0, 20f);
                break;
        }
    }
}