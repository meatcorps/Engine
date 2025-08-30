using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.PostProcessing;

public class FilmGrainPostProcessor : BasePostProcessor
{
    private float _time;
    public float Strength { get; set; } = 0.05f;

    public FilmGrainPostProcessor() 
        : base("Assets/Shaders/filmgrain.fx", new[] { "resolution", "time", "strength" }) { }

    protected override void ApplyValues(Shader shader, Texture2D target)
    {
        _time += Raylib.GetFrameTime();
        SetResolutionValue("resolution", target);
        SetValue("time", _time);
        SetValue("strength", Strength);
    }
}