using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AnimeFullScreenFeature : ScriptableRendererFeature
{
    class AnimeRenderPass : ScriptableRenderPass
    {
        private Material passMaterial;
        private RTHandle sourceTexture;
        private RTHandle tempTexture;

        public AnimeRenderPass(Material material)
        {
            passMaterial = material;
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            sourceTexture = renderingData.cameraData.renderer.cameraColorTargetHandle;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (passMaterial == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("AnimeFullScreenPass");

            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            RenderingUtils.ReAllocateIfNeeded(ref tempTexture, descriptor, name: "_TempAnimeTexture");

            Blitter.BlitCameraTexture(cmd, sourceTexture, tempTexture, passMaterial, 0);
            Blitter.BlitCameraTexture(cmd, tempTexture, sourceTexture);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void ReleaseTextures()
        {
            if (tempTexture != null) tempTexture.Release();
        }
    }

    [System.Serializable]
    public class Settings
    {
        public Material animePostMaterial;
    }

    public Settings settings = new Settings();
    private AnimeRenderPass animePass;

    public override void Create()
    {
        if (settings.animePostMaterial != null)
        {
            animePass = new AnimeRenderPass(settings.animePostMaterial);
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (animePass != null && settings.animePostMaterial != null)
        {
            renderer.EnqueuePass(animePass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (animePass != null)
        {
            animePass.ReleaseTextures();
        }
    }
}