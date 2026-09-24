using System.Collections;
using UnityEngine;

public class NarutoDashTrail : MonoBehaviour
{
    [Header("Trail Density & Timing")]
    [Tooltip("Very small delay for high density overlapping mesh snapshots.")]
    [SerializeField] private float meshRefreshRate = 0.012f;
    [Tooltip("How long each ghost stays visible.")]
    [SerializeField] private float meshDestroyDelay = 0.35f;

    [Header("Material Override")]
    [SerializeField] private Material trailMaterial;
    [SerializeField] private string colorProperty = "_Color";

    [Header("Glow & Fade Settings")]
    [ColorUsage(true, true)]
    [SerializeField] private Color glowColor = new Color(2.5f, 0.8f, 0.1f, 0.7f); // HDR Orange/Gold glow
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    private Coroutine activeTrailCoroutine;

    private void Awake()
    {
        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    public void ShowTrail(float duration)
    {
        if (activeTrailCoroutine != null)
        {
            StopCoroutine(activeTrailCoroutine);
        }

        activeTrailCoroutine = StartCoroutine(ActivateTrailRoutine(duration));
    }

    private IEnumerator ActivateTrailRoutine(float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                if (skinnedMeshRenderers[i] == null || !skinnedMeshRenderers[i].enabled)
                    continue;

                // Create ghost container
                GameObject ghostObj = new GameObject("DashGhost");
                ghostObj.transform.SetPositionAndRotation(
                    skinnedMeshRenderers[i].transform.position,
                    skinnedMeshRenderers[i].transform.rotation
                );
                ghostObj.transform.localScale = skinnedMeshRenderers[i].transform.lossyScale;

                // Attach components
                MeshRenderer mr = ghostObj.AddComponent<MeshRenderer>();
                MeshFilter mf = ghostObj.AddComponent<MeshFilter>();

                // Disable shadow casting/receiving on ghosts
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = false;

                // Bake mesh pose
                Mesh bakedMesh = new Mesh();
                skinnedMeshRenderers[i].BakeMesh(bakedMesh);
                mf.mesh = bakedMesh;

                // Assign Material instance
                if (trailMaterial != null)
                {
                    Material instancedMat = new Material(trailMaterial);
                    mr.material = instancedMat;

                    // Fade out ghost smoothly
                    StartCoroutine(FadeAndDestroy(ghostObj, instancedMat));
                }
                else
                {
                    Destroy(ghostObj);
                }
            }

            timer += meshRefreshRate;
            yield return new WaitForSeconds(meshRefreshRate);
        }

        activeTrailCoroutine = null;
    }

    private IEnumerator FadeAndDestroy(GameObject ghostObject, Material mat)
    {
        float elapsed = 0f;

        while (elapsed < meshDestroyDelay)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / meshDestroyDelay;
            float alphaEvaluated = fadeCurve.Evaluate(normalizedTime);

            if (mat != null)
            {
                Color currentGlow = glowColor;
                currentGlow.a *= alphaEvaluated;

                if (mat.HasProperty(colorProperty))
                    mat.SetColor(colorProperty, currentGlow);

                if (mat.HasProperty("_EmissionColor"))
                    mat.SetColor("_EmissionColor", currentGlow);
            }

            yield return null;
        }

        Destroy(ghostObject);
    }
}