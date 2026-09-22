using System.Collections.Generic;
using UnityEngine;

public class NarutoDashController : MonoBehaviour
{
    public enum DetectionMode
    {
        Raycast,
        SphereCast
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -20f;

    [Header("Dash")]
    [SerializeField] private float dashDistance = 7f;
    [SerializeField] private float dashDuration = 0.12f;
    [SerializeField] private float dashCooldown = 0.6f;

    [Header("Dash Detection")]
    [SerializeField] private DetectionMode detectionMode = DetectionMode.SphereCast;
    [SerializeField] private float detectionRadius = 0.5f;
    [SerializeField] private LayerMask dashDetectionLayers = ~0;

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;

    private bool isDashing;
    private bool canDash = true;

    private float dashTimer;
    private float cooldownTimer;
    private float verticalVelocity;

    private Vector3 dashDirection;
    private Vector3 dashStartPosition;
    private Vector3 dashEndPosition;

    private readonly HashSet<Collider> detectedColliders = new HashSet<Collider>();

    private void Reset()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isDashing)
        {
            UpdateDash();
            return;
        }

        UpdateCooldown();
        HandleNormalMovement();
        HandleDashInput();
    }

    // ============================================================
    // NORMAL MOVEMENT
    // ============================================================

    private void HandleNormalMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Hades-style movement direction
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical);

        // Prevent diagonal movement from being faster
        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        // -----------------------------------------
        // ROTATE CHARACTER TOWARD MOVEMENT
        // -----------------------------------------

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // -----------------------------------------
        // ACTUAL MOVEMENT
        // -----------------------------------------

        Vector3 velocity = moveDirection * moveSpeed;

        // Gravity
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        velocity.y = verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);

        // -----------------------------------------
        // ANIMATION
        // -----------------------------------------

        float movementAmount = moveDirection.magnitude;

        animator.SetFloat("Speed", movementAmount);
    }

    // ============================================================
    // DASH INPUT
    // ============================================================

    private void HandleDashInput()
    {
        if (!canDash)
            return;

        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartDash();
        }
    }

    // ============================================================
    // START DASH
    // ============================================================

    private void StartDash()
    {
        isDashing = true;
        canDash = false;

        dashTimer = 0f;
        cooldownTimer = dashCooldown;

        detectedColliders.Clear();

        // Capture starting position
        dashStartPosition = transform.position;

        // Direction vector
        dashDirection = transform.forward.normalized;

        // Displacement
        Vector3 dashDisplacement = dashDirection * dashDistance;

        // Destination
        dashEndPosition = dashStartPosition + dashDisplacement;

        // Start animation
        animator.ResetTrigger("DashEnd");
        animator.SetTrigger("Dash");

        // Check the ENTIRE dash path before moving
        DetectDashPath();

        Debug.DrawLine(
            dashStartPosition,
            dashEndPosition,
            Color.red,
            2f
        );
    }

    // ============================================================
    // DASH UPDATE
    // ============================================================

    private void UpdateDash()
    {
        dashTimer += Time.deltaTime;

        float normalizedTime = dashTimer / dashDuration;

        if (normalizedTime >= 1f)
        {
            normalizedTime = 1f;
        }

        // Smooth interpolation from start to end.
        Vector3 targetPosition = Vector3.Lerp(
            dashStartPosition,
            dashEndPosition,
            normalizedTime
        );

        Vector3 displacementThisFrame =
            targetPosition - transform.position;

        characterController.Move(displacementThisFrame);

        // Dash complete
        if (normalizedTime >= 1f)
        {
            FinishDash();
        }
    }

    // ============================================================
    // DASH END
    // ============================================================

    private void FinishDash()
    {
        isDashing = false;

        // Trigger Dash_Cut2
        animator.SetTrigger("DashEnd");
    }

    // ============================================================
    // COOLDOWN
    // ============================================================

    private void UpdateCooldown()
    {
        if (canDash)
            return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f)
        {
            cooldownTimer = 0f;
            canDash = true;
        }
    }

    // ============================================================
    // DASH PATH DETECTION
    // ============================================================

    private void DetectDashPath()
    {
        Vector3 origin = dashStartPosition + Vector3.up * detectionRadius;

        float distance = dashDistance;

        if (detectionMode == DetectionMode.Raycast)
        {
            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                dashDirection,
                distance,
                dashDetectionLayers,
                QueryTriggerInteraction.Ignore
            );

            foreach (RaycastHit hit in hits)
            {
                RegisterDetectedCollider(hit.collider);
            }
        }
        else
        {
            RaycastHit[] hits = Physics.SphereCastAll(
                origin,
                detectionRadius,
                dashDirection,
                distance,
                dashDetectionLayers,
                QueryTriggerInteraction.Ignore
            );

            foreach (RaycastHit hit in hits)
            {
                RegisterDetectedCollider(hit.collider);
            }
        }
    }

    // ============================================================
    // REGISTER DETECTED OBJECT
    // ============================================================

    private void RegisterDetectedCollider(Collider detectedCollider)
    {
        if (detectedCollider == null)
            return;

        if (detectedCollider.transform == transform)
            return;

        if (detectedColliders.Add(detectedCollider))
        {
            Debug.Log(
                "Dash detected: " +
                detectedCollider.gameObject.name
            );
        }
    }

    // ============================================================
    // DEBUG GIZMOS
    // ============================================================

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        if (isDashing)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(
                dashStartPosition,
                detectionRadius
            );

            Gizmos.DrawWireSphere(
                dashEndPosition,
                detectionRadius
            );

            Gizmos.DrawLine(
                dashStartPosition,
                dashEndPosition
            );
        }
    }
}