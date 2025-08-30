using System;
using System.Numerics;
using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Raylib_cs;

public class FixedPalettePostProcessorTex : BasePostProcessor
{
    public Vector3[] Palette { get; set; } = new Vector3[32]; // 0..1 RGB
    public int PaletteCount { get; set; } = 32;               // <= Palette.Length

    public float DitherStrength { get; set; } = 1f / 255f;
    public float DitherScale { get; set; } = 2f;
    public bool UsePerceptual { get; set; } = false;           // exact sprite colors? keep false
    public float ExactEpsilon { get; set; } = 1f / 255f;     // one LSB wiggle

    private Texture2D _paletteTex;
    private bool _paletteTexReady;
    private readonly Color[] _palettePixels = new Color[32];
    private int _frame;

    public FixedPalettePostProcessorTex()
        : base("Assets/Shaders/fixedpalette_tex.fx",
               new[] { "paletteTex", "paletteSize", "ditherStrength", "ditherScale", "ditherOffset", "exactEpsilon", "usePerceptual" })
    { }

    protected override void OnLoad()
    {
        // Create Nx1 texture once (N=32)
        var img = Raylib.GenImageColor(32, 1, Color.Black);
        _paletteTex = Raylib.LoadTextureFromImage(img);
        Raylib.UnloadImage(img);
        _paletteTexReady = true;

        // *** Force nearest sampling so texelFetch and any UV fallback are crisp ***
        Raylib.SetTextureFilter(_paletteTex, TextureFilter.Point);
    }

    public override void BeginFrame(float dt) => _frame++;

    protected override void ApplyValues(Shader shader, Texture2D target)
    {
        // 1) upload palette to texture each frame (cheap)
        BuildOrUpdatePaletteTexture();

        // 2) bind secondary texture + uniforms
        Raylib.SetShaderValueTexture(shader, ShaderLocations["paletteTex"], _paletteTex);

        var count = Math.Clamp(PaletteCount, 1, _palettePixels.Length);
        SetValue("paletteSize", count);
        SetValue("ditherStrength", DitherStrength);
        SetValue("ditherScale", DitherScale);
        SetValue("exactEpsilon", ExactEpsilon);
        SetValue("usePerceptual", UsePerceptual ? 1 : 0);

        // tiny temporal jitter to reduce static patterns
        var ox = (float)((_frame * 0.6180339887) % 4.0);
        var oy = (float)((_frame * 1.3247179572) % 4.0);
        SetValue("ditherOffset", new Vector2(ox, oy));
    }

    private void BuildOrUpdatePaletteTexture()
    {
        var count = Math.Clamp(PaletteCount, 1, _palettePixels.Length);

        for (int i = 0; i < _palettePixels.Length; i++)
        {
            var v = (i < count) ? Palette[i] : Palette[count - 1];
            _palettePixels[i] = new Color(
                (byte)Math.Clamp((int)MathF.Round(v.X * 255f), 0, 255),
                (byte)Math.Clamp((int)MathF.Round(v.Y * 255f), 0, 255),
                (byte)Math.Clamp((int)MathF.Round(v.Z * 255f), 0, 255),
                (byte)255
            );
        }

        unsafe
        {
            fixed (Color* p = _palettePixels)
            {
                Raylib.UpdateTexture(_paletteTex, p);
            }
        }
    }

    protected override void OnDispose()
    {
        if (_paletteTexReady)
        {
            Raylib.UnloadTexture(_paletteTex);
            _paletteTexReady = false;
        }
    }
}