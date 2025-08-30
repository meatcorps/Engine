using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.PostProcessing;

public class VignettePostProcessor : BasePostProcessor
{
    public float Strength { get; set; } = 1.2f;
    public float Radius { get; set; } = 0.75f;

    public VignettePostProcessor() 
        : base("Assets/Shaders/vignette.fx", new[] { "resolution", "strength", "radius" }) { }

    protected override void ApplyValues(Shader shader, Texture2D target)
    {
        SetResolutionValue("resolution", target);
        SetValue("strength", Strength);
        SetValue("radius", Radius);
    }
}