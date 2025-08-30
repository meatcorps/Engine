using System.Numerics;
using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.PostProcessing;

public class PaletteSwapPostProcessor : BasePostProcessor
{
    public Color ColorA { get; set; } = new Color(0f, 0f, 0f);
    public Color ColorB { get; set; } = new Color(0.33f, 0.33f, 0.33f);
    public Color ColorC { get; set; } = new Color(0.66f, 0.66f, 0.66f);
    public Color ColorD { get; set; } = new Color(1f, 1f, 1f);

    public PaletteSwapPostProcessor() 
        : base("Assets/Shaders/paletteswap.fx", new[] { "colorA", "colorB", "colorC", "colorD" }) { }

    protected override void ApplyValues(Shader shader, Texture2D target)
    {
        SetValue("colorA", ColorA);
        SetValue("colorB", ColorB);
        SetValue("colorC", ColorC);
        SetValue("colorD", ColorD);
    }
}