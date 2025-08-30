using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.PostProcessing;

public class BloomThresholdPostProcessor : BaseFinalPostProcessor
{
    public float Threshold { get; set; } = 0.8f;
    public float Knee { get; set; } = 0.1f;

    public BloomThresholdPostProcessor()
        : base("Assets/Shaders/bloom_threshold.fx", new[] { "threshold", "knee" }) { }

    protected override void ApplyValues(Shader shader, Texture2D target)
    {
        SetValue("threshold", Threshold);
        SetValue("knee", Knee);
    }
}