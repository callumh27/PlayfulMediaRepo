using UnityEngine;

public enum EarthEditorState
{
    None,
    Landmass,
    Tectonics
}

public class EarthEditor : MonoBehaviour
{
    public EarthEditorState currentEditingState;


    public EarthTimelinePoint currentTimelineToEdit;
    public Material earthMaterial;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        earthMaterial.SetTexture("_TectonicTexture", currentTimelineToEdit.tectonicMap);
        earthMaterial.SetTexture("_LandmassTexture", currentTimelineToEdit.landmassMap);
        if (currentEditingState == EarthEditorState.Tectonics)
        {
            earthMaterial.SetInt("_ViewTectonics", 1);
        }
        else
        {
            earthMaterial.SetInt("_ViewTectonics", 0);
        }

    }
}
