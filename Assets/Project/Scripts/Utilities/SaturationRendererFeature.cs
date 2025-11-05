using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SaturationRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private Shader shader;
    private Material material;
    private SaturationRenderPass renderPass;

    public override void Create()
    {
        if (shader == null)
        {
            shader = Shader.Find("Hidden/SaturationEffectURP");
        }

        if (shader == null)
        {
            Debug.LogError("Saturation shader not found!");
            return;
        }

        material = new Material(shader);
        renderPass = new SaturationRenderPass(material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderPass == null || material == null)
            return;

        renderer.EnqueuePass(renderPass);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && material != null)
        {
            DestroyImmediate(material);
        }
    }
}
