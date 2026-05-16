using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LightTransport;
using UnityEngine.SocialPlatforms;

public class EarthSpin : MonoBehaviour
{

    [SerializeField] float dragStrength = 1f;
    public bool spin = false;

    private float rotationX = 0f;
    private float rotationY = 0f;

    private bool isMouseOver = false;


    private Vector3 rotationalVelocity = Vector3.zero;
    private bool isMouseDown = false;

    // Update is called once per frame
    void Update()
    {
        if (isMouseOver && GameObject.Find("EarthEditor").GetComponent<EarthEditor>().currentMode != EarthEditorState.None)
        {
            isMouseDown = false;
            return;
        }

        if (Input.GetMouseButtonUp(1))
        {
            float xDelta = Input.mousePositionDelta.x * 0.5f;
            float yDelta = Input.mousePositionDelta.y * 0.5f;
            //rotationalVelocity = new Vector3(yDelta,-xDelta);
            Debug.Log(rotationalVelocity);
        }
        else if (Input.GetMouseButton(1))
        {
            isMouseDown = true;
            float xDelta = Input.mousePositionDelta.x * 0.5f;
            float yDelta = Input.mousePositionDelta.y * 0.5f;

            rotationX += yDelta * dragStrength;
            rotationX = Mathf.Clamp(rotationX, -25f, 25f);
            rotationY += xDelta * -dragStrength;

            Quaternion worldX = Quaternion.AngleAxis(rotationX, Vector3.right);
            Quaternion localY = Quaternion.AngleAxis(rotationY, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, worldX * localY, 0.2f);
            rotationalVelocity = new Vector3(yDelta,-xDelta) * dragStrength;
            //transform.Rotate(new Vector3(0,1,0), xDelta * -dragStrength, Space.Self);
            //if (transform.rotation.eulerAngles.x < 25 && transform.rotation.eulerAngles.x > -25)
            //{
            //    transform.Rotate(new Vector3(1,0,0), yDelta * dragStrength, Space.World);
            // }   
        }
        else
        {
            isMouseDown = false;
        }
        if (spin == true)
        {
            transform.Rotate(new Vector3(0, 1, 0), 0.1f, Space.Self);
        }
       
        
    }

    void FixedUpdate()
    {
        //rotational velocity decay
        if (isMouseDown) return;
        //if (rotationalVelocity.magnitude < 1) return;

        rotationalVelocity = Vector3.Lerp(rotationalVelocity, Vector3.zero, 0.05f);
        float xVelocity = rotationalVelocity.x;
        float yVelocity = rotationalVelocity.y;

        rotationX += xVelocity;
        rotationX = Mathf.Clamp(rotationX, -25f, 25f);
        rotationY += yVelocity;

        Quaternion worldX = Quaternion.AngleAxis(rotationX, Vector3.right);
        Quaternion localY = Quaternion.AngleAxis(rotationY, Vector3.up);

        transform.rotation = worldX * localY;

        //transform.Rotate(new Vector3(xVelocity,0,0), Space.World);
        //transform.Rotate(new Vector3(0,yVelocity,0), Space.Self);
    }

    private void OnMouseOver()
    {
        isMouseOver = true;
    }

    private void OnMouseExit()
    {
        isMouseOver = false;
    }
}
