using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.PostProcessing;

public class ScanlinesPostProcessor : BasePostProcessor
{
    public float Intensity { get; set; } = 0.2f;

    public ScanlinesPostProcessor() 
        : base("Assets/Shaders/scanlines.fx", new[] { "resolution", "intensity" }) { }

    protected override void ApplyValues(Shader shader, Texture2D target)
    {
        SetResolutionValue("resolution", target);
        SetValue("intensity", Intensity);
    }
}