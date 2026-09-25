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

    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 7f;
    [SerializeField] private float dashDuration = 0.12f;
    [SerializeField] private float dashCooldown = 0.6f;

    [Header("Obstacle & Boundary Collision")]
    [Tooltip("Select solid layers (e.g. Default, Environment, Obstacles) that Naruto CANNOT dash through.")]
    [SerializeField] private LayerMask solidObstacleLayers = 1;
    [Tooltip("Minimum distance to a wall required to trigger a dash.")]
    [SerializeField] private float minDashWallDistance = 0.6f;
    [Tooltip("Safety offset distance to stop before hitting a solid wall.")]
    [SerializeField] private float wallStoppingOffset = 0.4f;

    [Header("Dash Visual Effects")]
    [SerializeField] private ParticleSystem dashWindParticles;
    [SerializeField] private ParticleSystem footstepDustParticles;
    [SerializeField] private NarutoDashTrail dashTrail;

    [Header("UI References")]
    [SerializeField] private DashCooldownUI dashUI;

    [Header("Dust Customization")]
    [Tooltip("Time delay between footstep dust puffs while running.")]
    [SerializeField] private float runDustInterval = 0.25f;
    [Tooltip("Number of small dust particles per step / dash start.")]
    [SerializeField] private int dustEmitCount = 2;

    [Header("Dash Detection & Filtering (Breakables)")]
    [SerializeField] private DetectionMode detectionMode = DetectionMode.SphereCast;
    [SerializeField] private float detectionRadius = 0.5f;
    [Tooltip("Select ONLY breakable layers ('Bushes', 'BreakableBarrels') here.")]
    [SerializeField] private LayerMask dashDetectionLayers;

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    [SerializeField] private HadesCameraFollow cameraFollow;
    [SerializeField] private Transform mainCameraTransform;

    private bool isDashing;
    private bool canDash = true;
    private bool isTouchingWall;

    private float dashTimer;
    private float cooldownTimer;
    private float verticalVelocity;
    private float runDustTimer;

    private Vector3 dashDirection;
    private Vector3 dashStartPosition;
    private Vector3 dashEndPosition;

    private readonly HashSet<Collider> detectedColliders = new HashSet<Collider>();

    private void Reset()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        cameraFollow = Object.FindFirstObjectByType<HadesCameraFollow>();
        dashTrail = GetComponent<NarutoDashTrail>();
        dashUI = Object.FindFirstObjectByType<DashCooldownUI>();

        if (Camera.main != null)
            mainCameraTransform = Camera.main.transform;
    }

    private void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (cameraFollow == null)
            cameraFollow = Object.FindFirstObjectByType<HadesCameraFollow>();

        if (dashTrail == null)
            dashTrail = GetComponent<NarutoDashTrail>();

        if (dashUI == null)
            dashUI = Object.FindFirstObjectByType<DashCooldownUI>();

        if (mainCameraTransform == null && Camera.main != null)
            mainCameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        CheckWallProximity();

        if (isDashing)
        {
            UpdateDash();
            return;
        }

        UpdateCooldown();
        HandleNormalMovement();
        HandleDashInput();
    }

    private void CheckWallProximity()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 checkDirection = transform.forward;

        // Check if Naruto is facing a boundary wall within minimum required dash distance
        if (Physics.Raycast(rayOrigin, checkDirection, out RaycastHit wallHit, minDashWallDistance, solidObstacleLayers, QueryTriggerInteraction.Ignore))
        {
            isTouchingWall = true;
        }
        else
        {
            isTouchingWall = false;
        }

        // Shade out UI icon if touching boundary wall
        if (dashUI != null)
        {
            dashUI.SetWallBlockedState(isTouchingWall);
        }
    }

    private void HandleNormalMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 rawInput = new Vector3(horizontal, 0f, vertical);

        if (rawInput.sqrMagnitude > 1f)
            rawInput.Normalize();

        Vector3 moveDirection = Vector3.zero;

        if (mainCameraTransform != null)
        {
            Vector3 camForward = mainCameraTransform.forward;
            Vector3 camRight = mainCameraTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();

            moveDirection = (camForward * rawInput.z) + (camRight * rawInput.x);
        }
        else
        {
            moveDirection = rawInput;
        }

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        Vector3 velocity = moveDirection * moveSpeed;

        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        velocity.y = verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);

        float movementAmount = rawInput.magnitude;
        animator.SetFloat("Speed", movementAmount);

        HandleStepDust(movementAmount > 0.1f && characterController.isGrounded);
    }

    private void HandleStepDust(bool isMoving)
    {
        if (footstepDustParticles == null)
            return;

        if (isMoving)
        {
            runDustTimer += Time.deltaTime;
            if (runDustTimer >= runDustInterval)
            {
                runDustTimer = 0f;
                EmitDustPuff();
            }
        }
        else
        {
            runDustTimer = runDustInterval;
        }
    }

    private void EmitDustPuff()
    {
        if (footstepDustParticles != null)
        {
            footstepDustParticles.Emit(dustEmitCount);
        }
    }

    private void HandleDashInput()
    {
        if (!canDash || isTouchingWall)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartDash();
        }
    }

    private void StartDash()
    {
        isDashing = true;
        canDash = false;

        dashTimer = 0f;
        cooldownTimer = dashCooldown;

        detectedColliders.Clear();

        dashStartPosition = transform.position;
        dashDirection = transform.forward.normalized;

        float actualDashDistance = dashDistance;

        Vector3 rayOrigin = dashStartPosition + Vector3.up * 0.5f;
        if (Physics.Raycast(rayOrigin, dashDirection, out RaycastHit wallHit, dashDistance, solidObstacleLayers, QueryTriggerInteraction.Ignore))
        {
            actualDashDistance = Mathf.Max(0f, wallHit.distance - wallStoppingOffset);
        }

        Vector3 dashDisplacement = dashDirection * actualDashDistance;
        dashEndPosition = dashStartPosition + dashDisplacement;

        animator.ResetTrigger("DashEnd");
        animator.SetTrigger("Dash");

        // Trigger radial cooldown animation on UI
        if (dashUI != null)
        {
            dashUI.StartCooldown();
        }

        if (dashWindParticles != null)
        {
            dashWindParticles.Play();
        }

        EmitDustPuff();

        if (dashTrail != null)
        {
            dashTrail.ShowTrail(dashDuration);
        }

        if (cameraFollow != null)
        {
            cameraFollow.SetDashZoom(true);
        }

        DetectDashPath();

        Debug.DrawLine(
            dashStartPosition,
            dashEndPosition,
            Color.red,
            2f
        );
    }

    private void UpdateDash()
    {
        dashTimer += Time.deltaTime;

        float normalizedTime = dashTimer / dashDuration;

        if (normalizedTime >= 1f)
            normalizedTime = 1f;

        Vector3 targetPosition = Vector3.Lerp(
            dashStartPosition,
            dashEndPosition,
            normalizedTime
        );

        Vector3 displacementThisFrame =
            targetPosition - transform.position;

        displacementThisFrame.y = 0f;

        characterController.Move(displacementThisFrame);

        if (normalizedTime >= 1f)
            FinishDash();
    }

    private void FinishDash()
    {
        isDashing = false;
        animator.SetTrigger("DashEnd");

        if (dashTrail != null)
        {
            dashTrail.StopTrail();
        }

        if (cameraFollow != null)
        {
            cameraFollow.SetDashZoom(false);
        }
    }

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

    private void DetectDashPath()
    {
        Vector3 origin = dashStartPosition + Vector3.up * detectionRadius;
        float distance = Vector3.Distance(dashStartPosition, dashEndPosition);

        if (detectionMode == DetectionMode.Raycast)
        {
            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                dashDirection,
                distance,
                dashDetectionLayers,
                QueryTriggerInteraction.Collide
            );

            foreach (RaycastHit hit in hits)
                RegisterDetectedCollider(hit.collider);
        }
        else
        {
            RaycastHit[] hits = Physics.SphereCastAll(
                origin,
                detectionRadius,
                dashDirection,
                distance,
                dashDetectionLayers,
                QueryTriggerInteraction.Collide
            );

            foreach (RaycastHit hit in hits)
                RegisterDetectedCollider(hit.collider);
        }
    }

    private void RegisterDetectedCollider(Collider detectedCollider)
    {
        if (detectedCollider == null)
            return;

        if (detectedCollider.transform == transform)
            return;

        if (detectedColliders.Add(detectedCollider))
        {
            BreakableObject breakable = detectedCollider.GetComponent<BreakableObject>();
            if (breakable != null)
            {
                detectedCollider.enabled = false;
                breakable.Break();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        if (isDashing)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(
                dashStartPosition + Vector3.up * detectionRadius,
                detectionRadius
            );

            Gizmos.DrawWireSphere(
                dashEndPosition + Vector3.up * detectionRadius,
                detectionRadius
            );

            Gizmos.DrawLine(
                dashStartPosition + Vector3.up * detectionRadius,
                dashEndPosition + Vector3.up * detectionRadius
            );
        }
    }
}