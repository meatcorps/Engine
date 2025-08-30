using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.PostProcessing;

public class BloomCompositePostProcessor : BaseFinalPostProcessor, INeedsSceneTexture
{
    private Texture2D _scene;
    public float Intensity { get; set; } = 0.6f;

    public BloomCompositePostProcessor()
        : base("Assets/Shaders/bloom_composite.fx", new[] { "sceneTex", "intensity" }) { }

    public void SetSceneTexture(Texture2D scene) => _scene = scene;

    protected override void ApplyValues(Shader shader, Texture2D target)
    {
        // Bind the original scene as a second sampler
        Raylib.SetShaderValueTexture(shader, ShaderLocations["sceneTex"], _scene);
        SetValue("intensity", Intensity);
    }
}