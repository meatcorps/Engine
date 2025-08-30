using System.Numerics;
using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.PostProcessing;

public class GaussianBlurPostProcessor : BaseFinalPostProcessor
{
    public Vector2 Direction { get; set; } = new Vector2(1f, 0f); // set (0,1) for vertical
    public float Spread { get; set; } = 1.0f; 
    
    public GaussianBlurPostProcessor()
        : base("Assets/Shaders/gaussian_blur.fx", new[] { "resolution", "direction", "spread" }) { }

    protected override void ApplyValues(Shader shader, Texture2D target)
    {
        SetResolutionValue("resolution", target);
        SetValue("direction", Direction);
        SetValue("spread", Spread);
    }
}