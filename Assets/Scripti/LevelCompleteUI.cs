using UnityEngine;

public class StuckNeedle : MonoBehaviour
{
    [SerializeField] private float embedDepth = 0.2f;
    private bool isStuck = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isStuck) return;

        // Check if hitting the player or breakable environment
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            isStuck = true;

            // Stop physics/movement
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
            }

            // Disable collider so player doesn't trip on stuck needles
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Push slightly forward to embed inside character body
            transform.position += transform.forward * embedDepth;

            // Parent to the exact body part bone hit so needle moves with character animation
            transform.SetParent(other.transform);

            // Optional: Spawn small blood splash at impact point
            BreakableObject breakable = other.GetComponent<BreakableObject>();
            if (breakable != null)
            {
                breakable.Break();
            }
        }
    }
}