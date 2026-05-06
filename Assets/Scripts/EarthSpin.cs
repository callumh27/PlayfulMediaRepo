using UnityEngine;

public class EarthSpin : MonoBehaviour
{
    // user should be able to drag on the screen (if they arent clicking the planet)
    // should have a delay to the drag



    // Update is called once per frame
    void Update()
    {
        //transform.Rotate(new Vector3(0,1,0), 0.1f);
        float xChange = Input.mousePositionDelta.x;
        transform.Rotate(new Vector3(0,xChange,0), 0.1f);
    }
}
