
using UnityEngine;
using UnityEngine.InputSystem;


public class EarthLandPainter : MonoBehaviour
{
    public bool isEnabled;

    [Header("Controls")]
    [Range(0.1f, 20)]
    public float radius = 5f;
    [Range(0f, 1f)]
    public float strength = 1f;

    [Header("Properties")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask earthLayer;

    public Vector3 position = Vector3.zero;

    public RenderTexture renderTexture;

    private void Update()
    {
        if (!isEnabled) return;
        Vector3 mouseScreenPosition = new Vector3(Mouse.current.position.x.ReadValue(), Mouse.current.position.y.ReadValue(), 0);

        
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, earthLayer))
        {
            position = hit.point;
        }


    }

    private void OnDrawGizmos()
    {
        if (!isEnabled) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);

        Gizmos.DrawSphere(position, radius);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(position, radius);


    }

}
