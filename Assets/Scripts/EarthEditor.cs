using UnityEngine;

public class EarthEditor : MonoBehaviour
{

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
    }
}
