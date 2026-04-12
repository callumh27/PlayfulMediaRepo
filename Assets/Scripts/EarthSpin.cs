using UnityEngine;

public class EarthSpin : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0,1,0), 0.1f);
    }
}
