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
    [Range(0,4540)]
    public int millionYearsAgo;

    public Texture3D heightMap;
    public Texture3D tectonicMap;

    public List<TectonicPlate> tectonicPlates = new List<TectonicPlate>();
    public Vector4[] tectonicPoints;

    [Range(0f, 100f)]
    public float earthTemperature = 30f;

    public string currentPeriod;

    [Range(0f, 1f)]
    public float seaLevel = 0.3f;

    private int renderTextureSize = 256;


    

    

    


}
