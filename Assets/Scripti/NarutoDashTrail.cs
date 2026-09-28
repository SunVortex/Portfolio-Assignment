using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarutoDashTrail : MonoBehaviour
{
    [Header("Multi-Mesh Settings")]
    [Tooltip("Leave empty to automatically grab all sub-mesh parts under this character.")]
    [SerializeField] private SkinnedMeshRenderer[] playerRenderers;

    [Header("Ghost Trail Settings")]
    [SerializeField] private Material ghostMaterial;
    [SerializeField] private float ghostSpawnInterval = 0.03f;
    [SerializeField] private float ghostLifetime = 0.4f;

    [Header("Opacity Gradient (Start Faded -> End Bold)")]
    [Range(0f, 1f)][SerializeField] private float startOpacity = 0.15f;
    [Range(0f, 1f)][SerializeField] private float endOpacity = 0.95f;

    private bool isSpawningTrail;

    private void Awake()
    {
        // Auto-find all sub-mesh renderers (baju, celana, kepala, etc.) under Naruto
        if (playerRenderers == null || playerRenderers.Length == 0)
        {
            playerRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        }
    }

    public void ShowTrail(float dashDuration)
    {
        if (!isSpawningTrail)
        {
            StartCoroutine(SpawnTrailRoutine(dashDuration));
        }
    }

    public void StopTrail()
    {
        isSpawningTrail = false;
    }

    private IEnumerator SpawnTrailRoutine(float dashDuration)
    {
        isSpawningTrail = true;
        float elapsed = 0f;

        while (elapsed < dashDuration && isSpawningTrail)
        {
            elapsed += ghostSpawnInterval;

            // Progress from 0 (start of dash) to 1 (end of dash)
            float dashProgress = Mathf.Clamp01(elapsed / dashDuration);

            // Interpolate opacity so starting clones fade and ending clones remain bold
            float targetAlpha = Mathf.Lerp(startOpacity, endOpacity, dashProgress);

            SpawnFullGhostClone(targetAlpha);

            yield return new WaitForSeconds(ghostSpawnInterval);
        }

        isSpawningTrail = false;
    }

    private void SpawnFullGhostClone(float alpha)
    {
        if (playerRenderers == null || playerRenderers.Length == 0)
            return;

        // Container object for the full ghost clone
        GameObject fullGhostObj = new GameObject("DashGhostClone_Full");

        // Loop through all body parts (baju, celana, kepala, rambut, tangan, etc.)
        foreach (SkinnedMeshRenderer smr in playerRenderers)
        {
            if (smr == null || !smr.gameObject.activeInHierarchy)
                continue;

            // Create child mesh instance for each body part
            GameObject partObj = new GameObject(smr.name + "_Ghost");
            partObj.transform.SetParent(fullGhostObj.transform);
            partObj.transform.SetPositionAndRotation(smr.transform.position, smr.transform.rotation);
            partObj.transform.localScale = smr.transform.lossyScale;

            MeshFilter mf = partObj.AddComponent<MeshFilter>();
            MeshRenderer mr = partObj.AddComponent<MeshRenderer>();

            Mesh bakedMesh = new Mesh();
            smr.BakeMesh(bakedMesh);
            mf.mesh = bakedMesh;

            // Material selection
            Material baseMat = (ghostMaterial != null) ? ghostMaterial : smr.sharedMaterial;
            Material matInstance = new Material(baseMat);

            if (matInstance.HasProperty("_Color"))
            {
                Color c = matInstance.color;
                c.a = alpha;
                matInstance.color = c;
            }

            mr.material = matInstance;
        }

        // Smoothly fade out the entire combined clone structure
        StartCoroutine(FadeAndDestroyGhost(fullGhostObj, alpha));
    }

    private IEnumerator FadeAndDestroyGhost(GameObject ghostObj, float initialAlpha)
    {
        float timer = 0f;

        // Collect all mesh renderers inside the full clone
        MeshRenderer[] renderers = ghostObj.GetComponentsInChildren<MeshRenderer>();

        while (timer < ghostLifetime)
        {
            timer += Time.deltaTime;
            float fadeProgress = 1f - (timer / ghostLifetime);
            float currentAlpha = initialAlpha * fadeProgress;

            foreach (MeshRenderer mr in renderers)
            {
                if (mr != null && mr.material != null && mr.material.HasProperty("_Color"))
                {
                    Color c = mr.material.color;
                    c.a = currentAlpha;
                    mr.material.color = c;
                }
            }

            yield return null;
        }

        Destroy(ghostObj);
    }
}