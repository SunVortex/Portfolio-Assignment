using UnityEngine;

public class HadesCameraFollow : MonoBehaviour
{
    [Header("Target & Position")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 defaultOffset = new Vector3(0f, 10f, -8f);
    [SerializeField] private float followSpeed = 10f;

    [Header("Scroll Zoom Settings")]
    [Tooltip("How much scrolling affects zoom distance.")]
    [SerializeField] private float scrollSensitivity = 5f;
    [Tooltip("Maximum allowed temporary zoom-in (offset multiplier).")]
    [SerializeField] private float maxZoomIn = 0.5f;
    [Tooltip("Maximum allowed temporary zoom-out (offset multiplier).")]
    [SerializeField] private float maxZoomOut = 1.8f;
    [Tooltip("Speed at which the camera snaps back to default after releasing scroll.")]
    [SerializeField] private float returnSpeed = 3f;

    [Header("Dash Zoom Settings")]
    [SerializeField] private float dashZoomMultiplier = 1.2f;
    [SerializeField] private float dashZoomSpeed = 12f;

    private float currentZoomOffset = 1f;
    private bool isDashing;

    private void LateUpdate()
    {
        if (target == null) return;

        HandleScrollZoomInput();
        UpdateCameraPosition();
    }

    private void HandleScrollZoomInput()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            // Zoom in/out based on wheel direction
            currentZoomOffset -= scrollInput * scrollSensitivity;
            currentZoomOffset = Mathf.Clamp(currentZoomOffset, maxZoomIn, maxZoomOut);
        }
        else
        {
            // Smoothly spring back to default position (multiplier = 1) when scroll wheel is idle
            currentZoomOffset = Mathf.Lerp(currentZoomOffset, 1f, Time.deltaTime * returnSpeed);
        }
    }

    private void UpdateCameraPosition()
    {
        // Combine base offset with scroll zoom factor and dash zoom factor
        float targetMultiplier = currentZoomOffset * (isDashing ? dashZoomMultiplier : 1f);
        Vector3 targetOffset = defaultOffset * targetMultiplier;
        Vector3 desiredPosition = target.position + targetOffset;

        float activeFollowSpeed = isDashing ? dashZoomSpeed : followSpeed;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * activeFollowSpeed);

        // Keep camera looking smoothly at character
        transform.LookAt(target.position + Vector3.up * 1.2f);
    }

    /// <summary>
    /// Call this from NarutoDashController to trigger dynamic dash camera widening.
    /// </summary>
    public void SetDashZoom(bool dashing)
    {
        isDashing = dashing;
    }
}