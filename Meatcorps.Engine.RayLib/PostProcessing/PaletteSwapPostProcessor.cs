using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.PostProcessing;

public class PaletteSwapPostProcessor : BasePostProcessor
{
    public PaletteSwapPostProcessor()
        : base("Assets/Shaders/paletteswap.fx", new[] { "colorA", "colorB", "colorC", "colorD" })
    {
    }

    public Color ColorA { get; set; } = new(0f, 0f, 0f);
    public Color ColorB { get; set; } = new(0.33f, 0.33f, 0.33f);
    public Color ColorC { get; set; } = new(0.66f, 0.66f, 0.66f);
    public Color ColorD { get; set; } = new(1f, 1f, 1f);

    protected override void ApplyValues(Shader shader, Texture2D target)
    {
        SetValue("colorA", ColorA);
        SetValue("colorB", ColorB);
        SetValue("colorC", ColorC);
        SetValue("colorD", ColorD);
    }
}