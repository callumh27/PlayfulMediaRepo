using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EarthTimelinePoint: ScriptableObject
{

    public Texture3D landmassMap;
    public Texture3D tectonicMap;

    public List<TectonicPlate> tectonicPlates = new List<TectonicPlate>();
}
