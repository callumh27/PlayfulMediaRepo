using UnityEngine;


[CreateAssetMenu(fileName = "Timeline Event", menuName = "Earth Timeline")]
public class TimelineEvent : ScriptableObject
{

    public int eventTimeMYA = 0;
    public string eventName;
    public string eventText;
    public Sprite eventIcon;
}
