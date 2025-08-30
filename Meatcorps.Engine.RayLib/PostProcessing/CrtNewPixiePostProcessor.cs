using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Raylib_cs;

public class CrtNewPixiePostProcessor : BaseFinalPostProcessor
{
    private float _time;
    private Texture2D? _frameTex;
    private static Texture2D? _fallback1x1;

    public float Curvature { get; set; } = 2f;
    public float WiggleToggle { get; set; } = 0.0f;
    public float Scanroll { get; set; } = 1.5f;
    public float Vignette { get; set; } = 1.01f;
    public float Ghosting { get; set; } = 0.5f;
    public bool UseFrame { get; set; } = false;

    public CrtNewPixiePostProcessor()
        : base("Assets/Shaders/crt_newpixie.fx",
            new[] { "resolution","time","curvature","wiggleToggle","scanroll","vignette","ghosting","useFrame","frameTex" }) {}

    public void SetFrameTexture(Texture2D tex)
    {
        _frameTex = tex; UseFrame = true;
    }

    protected override void ApplyValues(Shader shader, Texture2D target)
    {
        _time += Raylib.GetFrameTime();

        SetResolutionValue("resolution", target);
        SetValue("time", _time);
        SetValue("curvature", Curvature);
        SetValue("wiggleToggle", WiggleToggle);
        SetValue("scanroll", Scanroll);
        SetValue("vignette", Vignette);
        SetValue("ghosting", Ghosting);
        SetValue("useFrame", 0.0f); // UseFrame ? 1.0f :

        Raylib.SetShaderValueTexture(shader, ShaderLocations["frameTex"], GetFallback());
    }

    protected override void DoOverlayRender(PointInt size)
    {
        if (_frameTex is not null)
            Raylib.DrawTexturePro(
                _frameTex.Value, 
                new Rectangle(0, 0,  _frameTex.Value.Width, _frameTex.Value.Height), 
                new Rectangle(0, 0, size.X, size.Y), Vector2.Zero, 0, Color.White);
    }

    private static Texture2D GetFallback()
    {
        if (_fallback1x1 is null)
        {
            var img = Raylib.GenImageColor(1, 1, new Color(0,0,0,0));
            _fallback1x1 = Raylib.LoadTextureFromImage(img);
            Raylib.UnloadImage(img);
        }
        return _fallback1x1.Value;
    }
}