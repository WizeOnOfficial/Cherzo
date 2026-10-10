// using UnityEngine;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.Universal;

// public class PixelizePass : ScriptableRenderPass
// {
//     private PixelizeFeature.CustomPassSettings settings;

//     private RTHandle colorBuffer, pixelBuffer;
//     private int pixelBufferID = Shader.PropertyToID("_PixelBuffer");

//     private Material material;
//     private int pixelScreenHeight, pixelScreenWidth;

//     public PixelizePass(PixelizeFeature.CustomPassSettings settings)
//     {
//         this.settings =  settings;
//         this.renderPassEvent = settings.renderPassEvent;
//         if(material == null) material = CoreUtils.CreateEngineMaterial("PixelizeRender");
//     }

//     public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//     {
//         CommandBuffer cmd = CommandBufferPool.Get();
//         using (new ProfilingScope(cmd, new ProfilingSampler("Pixelize Pass")))
//         {
//             Blitter.BlitCameraTexture(cmd, colorBuffer, pixelBuffer, material, 0);
//             Blitter.BlitCameraTexture(cmd, pixelBuffer, colorBuffer);
//         }

//         context.ExecuteCommandBuffer(cmd);
//         CommandBufferPool.Release(cmd);
//     }

//     public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
//     {
//         colorBuffer = renderingData.cameraData.renderer.cameraColorTargetHandle;
//         RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        
//         pixelScreenHeight = settings.screenHeight;
//         pixelScreenWidth = (int) (pixelScreenHeight * renderingData.cameraData.camera.aspect + 0.5f);

//         material.SetVector("_BlockCount", new Vector2(pixelScreenWidth, pixelScreenHeight));
//         material.SetVector("_BlockSize", new Vector2(1.0f/ pixelScreenWidth, 1.0f / pixelScreenHeight));
//         material.SetVector("_HalfBlockSize", new Vector2(0.5f / pixelScreenWidth, 0.5f / pixelScreenHeight));

//         descriptor.height = pixelScreenHeight;
//         descriptor.width = pixelScreenWidth;

//         // cmd.GetTemporaryRT(pixelBufferID, descriptor, FilterMode.Point);
//         // pixelBuffer = new RenderTargetIdentifier(pixelBufferID);

//         RenderingUtils.ReAllocateHandleIfNeeded(ref pixelBuffer, descriptor, FilterMode.Point, TextureWrapMode.Clamp, name: "_PixelBuffer");
//     }

//     public override void OnCameraCleanup(CommandBuffer cmd)
//     {
//         if (cmd == null) throw new System.ArgumentNullException("cmd");
//         cmd.ReleaseTemporaryRT(pixelBufferID);
//     }
// }
//
// NEW
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class PixelizePass : ScriptableRenderPass
{
    private PixelizeFeature.CustomPassSettings settings;
    private Material material;

    public PixelizePass(PixelizeFeature.CustomPassSettings settings)
    {
        this.settings = settings;
        this.renderPassEvent = settings.renderPassEvent;
        if (material == null) material = CoreUtils.CreateEngineMaterial("PixelizeRender");
    }

    // Pass Data container required for Render Graph execution
    private class PassData
    {
        public TextureHandle src;
        public TextureHandle pixelTarget;
        public Material material;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (material == null) return;

        // Fetch camera and resource data from Unity 6 frame context
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        TextureHandle source = resourceData.activeColorTexture;
        if (!source.IsValid()) return;

        // Calculate low-resolution target dimensions
        int pixelScreenHeight = settings.screenHeight;
        int pixelScreenWidth = Mathf.Max(1, (int)(pixelScreenHeight * cameraData.camera.aspect + 0.5f));

        // Pass uniform data to shader
        material.SetVector("_BlockCount", new Vector2(pixelScreenWidth, pixelScreenHeight));
        material.SetVector("_BlockSize", new Vector2(1.0f / pixelScreenWidth, 1.0f / pixelScreenHeight));
        material.SetVector("_HalfBlockSize", new Vector2(0.5f / pixelScreenWidth, 0.5f / pixelScreenHeight));

        // Create temporary render texture descriptor inside Render Graph
        RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
        descriptor.height = pixelScreenHeight;
        descriptor.width = pixelScreenWidth;
        descriptor.depthBufferBits = 0;

        TextureHandle pixelTarget = UniversalRenderer.CreateRenderGraphTexture(
            renderGraph, descriptor, "_PixelBuffer", false, FilterMode.Point, TextureWrapMode.Clamp
        );

        // --- Pass 1: Downsample to low-res pixel texture ---
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Pixelize Pass - Downsample", out var passData))
        {
            passData.src = source;
            passData.pixelTarget = pixelTarget;
            passData.material = material;

            builder.UseTexture(source, AccessFlags.Read);
            builder.SetRenderAttachment(pixelTarget, 0, AccessFlags.Write);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), data.material, 0);
            });
        }

        // --- Pass 2: Upsample back to main camera color texture ---
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Pixelize Pass - Upsample", out var passData))
        {
            passData.src = pixelTarget;
            passData.pixelTarget = source;

            builder.UseTexture(pixelTarget, AccessFlags.Read);
            builder.SetRenderAttachment(source, 0, AccessFlags.Write);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                Blitter.BlitTexture(context.cmd, data.src, new Vector4(1, 1, 0, 0), 0, false);
            });
        }
    }

    public void Dispose()
    {
        CoreUtils.Destroy(material);
    }
}