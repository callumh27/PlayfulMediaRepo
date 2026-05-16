using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class EarthSpin : MonoBehaviour
{

    [SerializeField] float dragStrength = 1f;


    private Vector3 rotationalVelocity = Vector3.zero;
    private bool isMouseDown = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            float xDelta = Input.mousePositionDelta.x;
            float yDelta = Input.mousePositionDelta.y;
            //rotationalVelocity = new Vector3(yDelta,-xDelta);
            Debug.Log(rotationalVelocity);
        }
        else if (Input.GetMouseButton(0))
        {
            isMouseDown = true;
            Debug.Log("Mose is down");
            float xDelta = Input.mousePositionDelta.x;
            float yDelta = Input.mousePositionDelta.y;
            rotationalVelocity = new Vector3(yDelta,-xDelta) * dragStrength;
            transform.Rotate(new Vector3(0,1,0), xDelta * -dragStrength, Space.Self);
            //if (transform.rotation.eulerAngles.x < 25 && transform.eulerAngles.x > -25)
            //{
                transform.Rotate(new Vector3(1,0,0), yDelta * dragStrength, Space.World);
            //}   
        }
        else
        {
            isMouseDown = false;
        }
        transform.Rotate(new Vector3(0,1,0), 0.1f, Space.Self);
        
    }

    void FixedUpdate()
    {
        //rotational velocity decay
        if (isMouseDown) return;
        if (rotationalVelocity.magnitude < 1) return;

        rotationalVelocity = Vector3.Lerp(rotationalVelocity, Vector3.zero, 0.1f);
        float xVelocity = rotationalVelocity.x;
        float yVelocity = rotationalVelocity.y;
        transform.Rotate(new Vector3(xVelocity,0,0), Space.World);
        transform.Rotate(new Vector3(0,yVelocity,0), Space.Self);
    }

}
