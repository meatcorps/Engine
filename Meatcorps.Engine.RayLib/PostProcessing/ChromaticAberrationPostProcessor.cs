using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.PostProcessing;

public class ChromaticAberrationPostProcessor : BaseFinalPostProcessor
{
    public ChromaticAberrationPostProcessor()
        : base("Assets/Shaders/chromaticaberration.fx", new[] { "resolution", "amount" })
    {
    }

    public float Amount { get; set; } = 2f;

    protected override void ApplyValues(Shader shader, Texture2D target)
    {
        SetResolutionValue("resolution", target);
        SetValue("amount", Amount);
    }
}