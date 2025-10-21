using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Game.HorrorJackpot.Data;
using Meatcorps.Game.HorrorJackpot.GameEnums;
using Raylib_cs;
using Rlgl = Raylib_cs.Rlgl;

namespace Meatcorps.Game.HorrorJackpot.Shaders;

public class DrumRenderer
{
    private readonly OneTexture _textureGlow;
    private readonly OneTexture _textureBase;
    private readonly Shader _shader;
    private FixedTimer _glowTimer = new(1000);

    // Public controls
    public float Glow { get; set; } // 0..1
    public float Speed { get; set; } // wraps per second
    public float Rotation { get; set; } // 0..1 (wrap)

    public float BlinkSpeed { get; set; } = 1f;
    
    private int _locRotation;
    private int _locGlow;
    private int _locVScale;
    private int _locUScale;
    private int _locUOffset;
    private int _locUAnchor;
    private int _locBands;
    private int _locVFrac;
    private int _locVOff;
    private int _locUvOffset;
    private int _locUvSize;
    private int _locTbDarkness;
    private int _locTbPower;
    private int _locGlowMode;
    private int _locGlowMask;
    private int _locGlowTint;
    private readonly ShaderManager<GameShaders> _shaderManager;
    private readonly IRenderTargetStrategy _renderTarget;
    public float VScale { get; set; } = 1f;
    public float UScale { get; set; } = 1f;
    public RectF Rectangle { get; set; } = new(52, 52, 256, 536);
    public Color GlowColor { get; set; }
    public DrumRenderer()
    {
        _textureBase = GlobalObjectManager.ObjectManager.Get<OneTexture>("HorrorJackpot")!;
        _textureGlow = GlobalObjectManager.ObjectManager.Get<OneTexture>("HorrorJackpot_glow")!;
        _shaderManager = GlobalObjectManager.ObjectManager.Get<ShaderManager<GameShaders>>()!;
        _renderTarget = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()!;
        _shader = _shaderManager.GetShader(GameShaders.Drum);
        SetupShader();
    }

    private void SetupShader()
    {
        var locTexBase = Raylib.GetShaderLocation(_shader, "texBase");
        var locTexGlow = Raylib.GetShaderLocation(_shader, "texGlow");
        _locRotation = Raylib.GetShaderLocation(_shader, "rotation01");
        _locGlow     = Raylib.GetShaderLocation(_shader, "glow01");
        _locUScale   = Raylib.GetShaderLocation(_shader, "uScale");
        _locUAnchor  = Raylib.GetShaderLocation(_shader, "uAnchor");
        _locUOffset  = Raylib.GetShaderLocation(_shader, "uOffset");
        _locVScale   = Raylib.GetShaderLocation(_shader, "vScale");
        _locBands    = Raylib.GetShaderLocation(_shader, "bandCount");
        _locVFrac    = Raylib.GetShaderLocation(_shader, "vVisibleFrac");
        _locVOff     = Raylib.GetShaderLocation(_shader, "vVisibleOffset");
        _locUvOffset  = Raylib.GetShaderLocation(_shader, "uvOffset");
        _locUvSize    = Raylib.GetShaderLocation(_shader, "uvSize");
        _locTbDarkness = Raylib.GetShaderLocation(_shader, "tbDarkness");
        _locTbPower    = Raylib.GetShaderLocation(_shader, "tbPower");
        _locGlowMode    = Raylib.GetShaderLocation(_shader, "glowMode");
        _locGlowMask    = Raylib.GetShaderLocation(_shader, "glowMaskBase");
        _locGlowTint    = Raylib.GetShaderLocation(_shader, "glowTint");
        
        Raylib.SetShaderValue(_shader, locTexBase, 0, ShaderUniformDataType.Int);
        Raylib.SetShaderValue(_shader, locTexGlow, 1, ShaderUniformDataType.Int);
        Raylib.SetShaderValue(_shader, _locVScale, VScale, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(_shader, _locUScale, UScale, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(_shader, _locUOffset, 0.0f, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(_shader, _locUAnchor, 0.0f, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(_shader, _locBands, 1.0f /* or 9/10 depending on your layout */, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(_shader, _locVFrac, 0.5f, ShaderUniformDataType.Float);   // show half
        Raylib.SetShaderValue(_shader, _locVOff, 0.0f, ShaderUniformDataType.Float);    // choose which half
        Raylib.SetShaderValue(_shader, _locTbDarkness, 0.1f, ShaderUniformDataType.Float); // darkest top/bottom
        Raylib.SetShaderValue(_shader, _locTbPower,    1.0f, ShaderUniformDataType.Float); // curve exponent
        Raylib.SetShaderValue(_shader, _locGlowMode, 1.0f, Raylib_cs.ShaderUniformDataType.Float); // 0=Add
        Raylib.SetShaderValue(_shader, _locGlowMask, 1.0f, Raylib_cs.ShaderUniformDataType.Float); // respect base alpha
        
        Speed = 0.05f;
        Rotation = 0f;
        Glow = 0.1f;
    }

    public void Update(float deltaTime)
    {
        if (BlinkSpeed > 0)
            _glowTimer.ChangeSpeed(BlinkSpeed * 1000f);
        
        _glowTimer.Update(deltaTime);
        var rotation = Rotation;
        var speed = Speed;
        Tween.ReelUpdateExp(ref rotation, ref speed, halfLife: 0.95f, minSpeed: 0.001f, deltaTime);
        Rotation = rotation;
        Speed = speed;
        
        Rotation += Speed * deltaTime;
        Rotation -= MathF.Floor(Rotation);
        if (!BlinkSpeed.EqualsSafe(0))
            Glow = Tween.NormalToUpDown(_glowTimer.NormalizedElapsed);
    }

    public void Draw()
    {
        Rlgl.ActiveTextureSlot(1);
        Rlgl.EnableTexture(_textureGlow.Texture.Id);
        Rlgl.ActiveTextureSlot(0);

        var uvOffset = new System.Numerics.Vector2(Rectangle.X / _renderTarget.RenderWidth, Rectangle.Y / _renderTarget.RenderHeight);
        var uvSize   = new System.Numerics.Vector2(Rectangle.Width / _renderTarget.RenderWidth, Rectangle.Height / _renderTarget.RenderHeight);

        Raylib.SetShaderValue(_shader, _locUvOffset, uvOffset, ShaderUniformDataType.Vec2);
        Raylib.SetShaderValue(_shader, _locUvSize,   uvSize,   ShaderUniformDataType.Vec2);
        Raylib.SetShaderValue(_shader, _locRotation, Rotation.Wrap(1), ShaderUniformDataType.Float);
        Raylib.SetShaderValue(_shader, _locGlow, Glow * 0.5f, ShaderUniformDataType.Float);
        var tint = new System.Numerics.Vector3(GlowColor.R / 255f, GlowColor.G / 255f, GlowColor.B / 255f); // neutral
        Raylib.SetShaderValue(_shader, _locGlowTint, tint, Raylib_cs.ShaderUniformDataType.Vec3);

        using (_shaderManager.UseShader(GameShaders.Drum))
        {
            var src = new Rectangle(0, 0, _textureBase.Texture.Width, _textureBase.Texture.Height);
            var dst = new Rectangle(0, 0, _renderTarget.RenderWidth, _renderTarget.RenderHeight);
            Raylib.DrawTexturePro(_textureBase.Texture, src, dst, Vector2.Zero, 0f, Color.White);
        }
    }
}