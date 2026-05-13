using System.Collections.Generic;
using UnityEngine;

public class TimelineManager : MonoBehaviour
{

    [System.Serializable]
    public struct TimelinePointData
    {
        [Range(0,1f)] public float normalizedTime;
        public string yearsAgo;

    }

    [Range(0,100f)]
    public float timelinePercentage = 0;


    [Header("UI References")]
    [SerializeField] private GameObject timelinePointPrefab;
    [SerializeField] private RectTransform timelineBar;

    [Header("Data")]
    [SerializeField] private List<TimelinePointData> timelinePoints;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateTimelinePoints();
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
            GameObject newPoint = Instantiate(timelinePointPrefab, timelineBar);

            RectTransform rt = newPoint.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0,0.5f);
            rt.anchorMax = new Vector2(0,0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            float xPosition = point.normalizedTime * totalWidth;
            rt.anchoredPosition = new Vector2(xPosition, 0);

        }
    }
}
