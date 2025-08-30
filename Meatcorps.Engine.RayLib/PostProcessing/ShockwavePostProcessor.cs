using System.Numerics;
using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.PostProcessing;

public class ShockwavePostProcessor : BaseFinalPostProcessor
{
    private float _time;
    public Vector2 Center { get; set; } = new Vector2(0.5f, 0.5f);
    public float Speed { get; set; } = 4f;
    public float Size { get; set; } = 10f;

    public ShockwavePostProcessor() 
        : base("Assets/Shaders/shockwave.fx", new[] { "resolution", "time", "center", "speed", "size" }) { }

    protected override void ApplyValues(Shader shader, Texture2D target)
    {
        _time += Raylib.GetFrameTime();
        SetResolutionValue("resolution", target);
        SetValue("time", _time);
        SetValue("center", Center);
        SetValue("speed", Speed);
        SetValue("size", Size);
    }
}