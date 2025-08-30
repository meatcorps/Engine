using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.PostProcessing;

public class OnePassBloomPostProcessor : BaseFinalPostProcessor
{
    public float Intensity { get; set; } = 0.5f;
    public float Threshold { get; set; } = 0.8f;

    public OnePassBloomPostProcessor() 
        : base("Assets/Shaders/bloom.fx", new[] { "resolution", "intensity", "threshold" }) { }

    protected override void ApplyValues(Shader shader, Texture2D target)
    {
        SetResolutionValue("resolution", target);
        SetValue("intensity", Intensity);
        SetValue("threshold", Threshold);
    }
}