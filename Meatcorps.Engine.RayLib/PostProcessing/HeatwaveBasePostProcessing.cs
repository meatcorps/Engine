using System.Numerics;
using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.PostProcessing;

public class HeatwaveBasePostProcessing : BasePostProcessor
{
    private float _time;

    public HeatwaveBasePostProcessing() : base("Assets/Shaders/heatwave.fx", ["time", "resolution"], true)
    {
    }

    protected override void ApplyValues(Shader shader, Texture2D target)
    {
        _time += Raylib.GetFrameTime();
        SetValue("time", _time);
        SetResolutionValue("resolution", target);
        base.ApplyValues(shader, target);
    }
}