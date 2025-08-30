using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.PostProcessing;

public class CrtPostProcessor : BaseFinalPostProcessor
{
    private float _time;

    // Configurable values
    public float Distortion { get; set; } = 0.04f;
    public float ScanlineIntensity { get; set; } = 0.03f;
    public float FlickerStrength { get; set; } = 0.00001f;
    public float ChromaticAberration { get; set; } = 0.002f;
    public float NoiseStrength { get; set; } = 0.01f;
    public float VignetteStrength { get; set; } = 0.99f; // from 0.0 (none) to 1.0 (full)
    public float VignetteRadius { get; set; } = 0.1f;  // how far from center it starts

    public CrtPostProcessor() : base("Assets/Shaders/crt.fx", new[]
    {
        "resolution", "time", "distortion", "scanlineIntensity", "flickerStrength",
        "chromaticAberration", "noiseStrength", "vignetteStrength", "vignetteRadius"
    }, true)
    {
    }

    protected override void ApplyValues(Shader shader, Texture2D target)
    {
        _time += Raylib.GetFrameTime();

        SetResolutionValue("resolution", target);
        SetValue("time", _time);

        SetValue("distortion", Distortion);
        SetValue("scanlineIntensity", ScanlineIntensity);
        SetValue("flickerStrength", FlickerStrength);
        SetValue("chromaticAberration", ChromaticAberration);
        SetValue("noiseStrength", NoiseStrength);
        SetValue("vignetteStrength", VignetteStrength);
        SetValue("vignetteRadius", VignetteRadius);
    }
}