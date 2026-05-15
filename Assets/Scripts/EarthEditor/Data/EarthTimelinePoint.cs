using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using static UnityEditor.Rendering.CameraUI;

public class EarthTimelinePoint: ScriptableObject
{

    //public HeightData heightData;
    //public TectonicData tectonicData;
    public string yearsAgo;

    public Texture3D heightMap;
    public Texture3D tectonicMap;

    public List<TectonicPlate> tectonicPlates = new List<TectonicPlate>();
    public Vector4[] tectonicPoints;

    private int renderTextureSize = 256;


    

    

    


}
