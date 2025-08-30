using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.PostProcessing;
/// <summary>
/// The LUT must be a 16×16×16, stored in a 256×16 image.
/// </summary>
public class ColorGradingPostProcessor : BaseFinalPostProcessor
{
    private Texture2D _lut;

    public ColorGradingPostProcessor(string lutPath) 
        : base("Assets/Shaders/colorgrading.fx", new[] { "lutTexture" })
    {
        _lut = Raylib.LoadTexture(lutPath);
    }

    protected override void ApplyValues(Shader shader, Texture2D target)
    {
        Raylib.SetShaderValueTexture(shader, ShaderLocations["lutTexture"], _lut);
    }

    protected override void OnDispose()
    {
        Raylib.UnloadTexture(_lut);
    }
}