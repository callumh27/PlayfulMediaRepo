using System.Collections.Generic;
using UnityEngine;

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


    [Header("Output")]
    public RenderTexture outputTectonicTexture;
    public RenderTexture outputHeightTexture;

    private ComputeBuffer rotationBuffer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //GenerateTimelinePoints();

    }

    private void OnDestroy()
    {
        rotationBuffer.Release();
    }

    public void SetTime(float time)
    {
        currentTime = time;
        UpdateMovementCompute();
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

    void BakeMovements()
    {

    }


}
