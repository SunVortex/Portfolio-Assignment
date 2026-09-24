using UnityEngine;

public class HadesCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Camera Offset")]
    [SerializeField] private Vector3 offset = new Vector3(8f, 10f, -8f);

    [Header("Fixed Camera Rotation")]
    [Tooltip("Enable to lock camera rotation completely.")]
    [SerializeField] private bool lockRotation = true;
    [Tooltip("Fixed Euler angles for camera rotation (X, Y, Z). Set Y to 45.")]
    [SerializeField] private Vector3 fixedRotation = new Vector3(45f, 45f, 0f);

    [Header("Follow Settings")]
    [SerializeField] private float followSmoothness = 8f;

    [Header("Dash Zoom Settings")]
    [Tooltip("Target FOV or size during dash.")]
    [SerializeField] private float dashZoomFOV = 65f;
    [SerializeField] private float zoomInSpeed = 10f;
    [SerializeField] private float zoomOutSpeed = 5f;

    private Camera cam;
    private float defaultFOV;
    private bool isZooming;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            defaultFOV = cam.orthographic ? cam.orthographicSize : cam.fieldOfView;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        // Position Follow (smooth transition to target + offset)
        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSmoothness * Time.deltaTime
        );

        // Rotation Control (Fixed rotation instead of LookAt)
        if (lockRotation)
        {
            transform.rotation = Quaternion.Euler(fixedRotation);
        }
        else
        {
            transform.LookAt(target);
        }

        // Dynamic Zoom Logic
        HandleZoom();
    }

    private void HandleZoom()
    {
        if (cam == null)
            return;

        float targetFOV = isZooming ? dashZoomFOV : defaultFOV;
        float currentSpeed = isZooming ? zoomInSpeed : zoomOutSpeed;

        if (cam.orthographic)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetFOV, currentSpeed * Time.deltaTime);
        }
        else
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, currentSpeed * Time.deltaTime);
        }
    }

    public void SetDashZoom(bool active)
    {
        isZooming = active;
    }
}