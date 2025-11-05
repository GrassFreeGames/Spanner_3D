using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class SaturationRenderPass : ScriptableRenderPass
{
    private Material material;
    private static readonly int SaturationProperty = Shader.PropertyToID("_Saturation");

    private class PassData
    {
        internal Material material;
        internal float saturation;
    }

    public SaturationRenderPass(Material material)
    {
        this.material = material;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var stack = VolumeManager.instance.stack;
        var saturationVolume = stack.GetComponent<SaturationVolume>();

        if (material == null || saturationVolume == null || !saturationVolume.IsActive())
            return;

        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData = frameData.Get<UniversalCameraData>();

        if (resourceData.isActiveTargetBackBuffer)
            return;

        var source = resourceData.activeColorTexture;
        var destinationDesc = cameraData.cameraTargetDescriptor;
        destinationDesc.depthBufferBits = 0;
        destinationDesc.msaaSamples = 1;

        TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(
            renderGraph, destinationDesc, "SaturationTexture", false);

        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Saturation Effect", out var passData))
        {
            passData.material = material;
            passData.saturation = saturationVolume.saturation.value;

            builder.UseTexture(source, AccessFlags.Read);
            builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                data.material.SetFloat(SaturationProperty, data.saturation);
                data.material.SetTexture("_MainTex", source);
                Blitter.BlitTexture(context.cmd, source, new Vector4(1, 1, 0, 0), data.material, 0);
            });
        }

        resourceData.cameraColor = destination;
    }

    // Fallback Execute method (required even when using Render Graph)
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        // This method is not used when Render Graph is enabled
        // It's here to prevent warnings
    }
}
