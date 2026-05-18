using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;



/* 

to get rotated plate at current time, check the two closest timeline points,
get their tectonic maps and check the rotation required between them
pass this rotation to a compute shader which will construct the final tectonic and height maps

*/
public class TimelineManager : MonoBehaviour
{

    [Header("Data")]
    [SerializeField] private List<EarthTimelinePoint> timelinePoints;
    [Range(0,100f)]
    public float currentTime = 0;

    public ComputeShader heightmapLerpCompute;
    private int textureResolution = 256;


    [Header("UI References")]
    [SerializeField] private GameObject timelinePointPrefab;
    [SerializeField] private RectTransform timelineBar;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text periodText;

    public Material earthMaterial;

    [Header("Output")]
    public RenderTexture outputTectonicTexture;
    public RenderTexture outputHeightTexture;

    public float currentTemperature;
    public float currentSeaLevel;

    private TimelineTransition[] transitions;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BakeTransitions();
        GenerateOutputTextures();
        earthMaterial.SetTexture("_TectonicTexture", outputTectonicTexture);
        earthMaterial.SetTexture("_HeightmapTexture", outputHeightTexture);
        
        // fix this if you want to have a setting to view the tectonic plates
        //earthMaterial.SetTexture("_PlateColourLookupTexture", editableTimeLine.plateColourLookup);
        

    }

    private void Update()
    {
        //UpdateOutputTextures(currentTime/100f);
        earthMaterial.SetFloat("_Temperature", currentTemperature);
        earthMaterial.SetFloat("_SeaLevel", currentSeaLevel);
    }

    

    private void OnValidate()
    {
        UpdateOutputTextures(currentTime / 100f);
    }

    public void SetTime(float time)
    {
        currentTime = time;
        UpdateOutputTextures(currentTime/100f);
        timeText.text = FormatGeologicalTime();
        
    }

    public void SliderChanged(float newValue)
    {
        SetTime(newValue);
    }

    public void GenerateTimelinePoints()
    {
        foreach (Transform child in timelineBar)
        {
            if (child.gameObject != timelinePointPrefab) Destroy(child.gameObject);
        }

        float totalWidth = timelineBar.rect.width;

        foreach (var point in timelinePoints)
        {
            /*GameObject newPoint = Instantiate(timelinePointPrefab, timelineBar);

            RectTransform rt = newPoint.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0,0.5f);
            rt.anchorMax = new Vector2(0,0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            float xPosition = point.normalizedTime * totalWidth;
            rt.anchoredPosition = new Vector2(xPosition, 0);*/

        }
    }

    void BakeTransitions()
    {
        if (timelinePoints == null || timelinePoints.Count < 2) return;
        transitions = new TimelineTransition[timelinePoints.Count - 1];
        for (int i = 0; i < timelinePoints.Count - 1; i++)
        {
            transitions[i] = new TimelineTransition
            {
                point1 = timelinePoints[i],
                point2 = timelinePoints[i + 1]
            };
            transitions[i].Bake();
        }
    }

    void GenerateOutputTextures()
    {
        outputHeightTexture = GenerateRenderTexture();
        outputTectonicTexture = GenerateRenderTexture();
    }

    void UpdateOutputTextures(float time)
    {

        for (int i = 0; i < transitions.Length; i++)
        {
            float point1Time = 1 - (timelinePoints[i].millionYearsAgo / 4540f);
            float point2Time = 1 - (timelinePoints[i + 1].millionYearsAgo / 4540f);

            UpdateTimePeriod(time);

            if (time < point1Time || time > point2Time)
            {
                continue;
            }

            

            float t = Mathf.InverseLerp(point1Time, point2Time, time);

            currentTemperature = Mathf.Lerp(timelinePoints[i].earthTemperature, timelinePoints[i + 1].earthTemperature, t);
            currentSeaLevel = Mathf.Lerp(timelinePoints[i].seaLevel, timelinePoints[i + 1].seaLevel, t);

            heightmapLerpCompute.SetFloat("t", t);


            heightmapLerpCompute.SetTexture(0, "srcHeightTexture", transitions[i].bakedHeightMap1);
            heightmapLerpCompute.SetTexture(0, "destHeightTexture", transitions[i].bakedHeightMap2);

            heightmapLerpCompute.SetTexture(0, "outputHeightTexture", outputHeightTexture);

            heightmapLerpCompute.Dispatch(0, textureResolution / 8, textureResolution / 8, textureResolution / 8);

            //Debug.Log("updated");

            return;

        }
    }

    void UpdateTimePeriod(float time)
    {
        string timePeriod = "Holocene";
        int mya = (int)((1 - time) * 4540);
        if (mya <= 4540)
        {
            timePeriod = "Hadean";
        }
        else if (mya <= 4000)
        {
            timePeriod = "Archean";
        }
        else if (mya <= 2500)
        {
            timePeriod = "Proterozoic";
        }
        else if (mya <= 540)
        {
            timePeriod = "Cambrian";
        }
        else if (mya <= 490)
        {
            timePeriod = "Ordovician";
        }
        else if (mya <= 445)
        {
            timePeriod = "Silurian";
        }
        else if (mya <= 415)
        {
            timePeriod = "Devonian";
        }
        else if (mya <= 360)
        {
            timePeriod = "Carboniferous";
        }
        else if (mya <= 300)
        {
            timePeriod = "Permian";
        }
        else if (mya <= 250)
        {
            timePeriod = "Triassic";
        }
        else if (mya <= 250)
        {
            timePeriod = "Triassic";
        }
        else if (mya <= 200)
        {
            timePeriod = "Jurassic";
        }
        else if (mya <= 150)
        {
            timePeriod = "Cretaceous";
        }
        else if (mya <= 65)
        {
            timePeriod = "Paleogene";
        }
        else if (mya <= 20)
        {
            timePeriod = "Quaternary";
        }
        periodText.text = $"Current Period - {timePeriod}";
    }
    RenderTexture GenerateRenderTexture()
    {
        RenderTexture renderTexture = new RenderTexture(textureResolution, textureResolution, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8_SNorm;
        renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        renderTexture.volumeDepth = textureResolution;
        renderTexture.filterMode = FilterMode.Bilinear;
        renderTexture.Create();

        AssetDatabase.CreateAsset(renderTexture, CreateCorrectPathName("Assets/Pre-Compute/Cache/", "OutputTexture"));

        return renderTexture;
    }

    string FormatGeologicalTime()
    {
        float mya = (1 - currentTime/100f) * 4540;

        if (mya >= 1000)
        {
            float bya = Mathf.Round(mya / 10f) / 100f;
            return $"{bya} BYA";
        }
        else
        {
            float rounded = Mathf.Round(mya);
            if (rounded == 0)
            {
                return "Present Day";
            }
            return $"{rounded} MYA";
        }
    }

    string CreateCorrectPathName(string path, string assetName)
    {
        int fileNumber = 0;
        string fileName;
        string fullPath;

        do
        {
            fileName = $"{assetName}_{fileNumber}.asset";
            fullPath = path + fileName;
            fileNumber++;
        }
        while (File.Exists(fullPath));

        return fullPath;
    }



}
