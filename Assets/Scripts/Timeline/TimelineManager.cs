using System.Collections.Generic;
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

    public ComputeShader plateMovementCompute;
    private int textureResolution = 256;


    [Header("UI References")]
    [SerializeField] private GameObject timelinePointPrefab;
    [SerializeField] private RectTransform timelineBar;

    public Material earthMaterial;

    [Header("Output")]
    public RenderTexture outputTectonicTexture;
    public RenderTexture outputHeightTexture;

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
        UpdateOutputTextures(currentTime);
    }

    public void SetTime(float time)
    {
        currentTime = time;
        UpdateOutputTextures(currentTime);
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

        for (int i = 0; i < timelinePoints.Count -1; i++)
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
            float point1Time = 1 - (timelinePoints[i].millionYearsAgo / 4540);
            float point2Time = 1 - (timelinePoints[i + 1].millionYearsAgo / 4540);

            if (time < point1Time || time > point2Time)
            {
                continue;
            }

            float t = Mathf.InverseLerp(point1Time, point2Time, time);
            Vector4[] rotations = transitions[i].GetSlerpedRotations(t);
            Vector4[] tectonicPoints = transitions[i].GetRotatedTectonicPoints(t);

            earthMaterial.SetFloat("_amountOfPlates", transitions[i].point1.tectonicPlates.Count);

            plateMovementCompute.SetVectorArray("plateRotations", rotations);
            plateMovementCompute.SetVectorArray("tectonicPoints", tectonicPoints);
            plateMovementCompute.SetFloat("planetRadius", 20);
            plateMovementCompute.SetInt("textureSize", 256);
            plateMovementCompute.SetFloat("t", t);

            Debug.Log(transitions[i].point1.tectonicMap == null);

            plateMovementCompute.SetTexture(0, "srcTectonicTexture", transitions[i].bakedTectonicMap1);
            plateMovementCompute.SetTexture(0, "srcHeightTexture", transitions[i].bakedHeightMap1);
            plateMovementCompute.SetTexture(0, "destHeightTexture", transitions[i].bakedHeightMap2);

            plateMovementCompute.SetTexture(0, "outputHeightTexture", outputHeightTexture);
            plateMovementCompute.SetTexture(0, "outputTectonicTexture", outputTectonicTexture);

            Debug.Log($"tectonic map: {transitions[i].point1.tectonicMap.width} x {transitions[i].point1.tectonicMap.height} x {transitions[i].point1.tectonicMap.depth} format: {transitions[i].point1.tectonicMap.graphicsFormat}");

            plateMovementCompute.Dispatch(0, textureResolution / 8, textureResolution / 8, textureResolution / 8);

            //Debug.Log("updated");

            return;

        }
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

        return renderTexture;
    }



}
