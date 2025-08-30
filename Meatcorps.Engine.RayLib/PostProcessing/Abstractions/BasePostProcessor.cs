using System.Numerics;
using System.Runtime.InteropServices;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.PostProcessing.Abstractions;

public abstract class BasePostProcessor : IPostProcessor, IDisposable
{
    private Shader _shader;
    private readonly string _fxFilename;
    protected readonly Dictionary<string, int> ShaderLocations = new();
    private bool _isLoaded;

    public BasePostProcessor(string fxFilename, string[] shaderValues, bool enabled = true, bool includeUI = false)
    {
        Enabled = enabled;
        IncludeUI = includeUI;
        _fxFilename = fxFilename;
        foreach (var shaderValue in shaderValues)
        {
            ShaderLocations.Add(shaderValue, 0);
        }
    }

    public bool Enabled { get; set; }
    public bool IncludeUI { get; set; }
    
    private bool _isDisposed;
    
    public void Load()
    {
        if (_isLoaded) 
            return;
        
        _isLoaded = true;
        
        _shader = Raylib.LoadShader(null, _fxFilename);
        
        foreach (var shaderLocation in ShaderLocations)
            ShaderLocations[shaderLocation.Key] = Raylib.GetShaderLocation(_shader, shaderLocation.Key);

        // Optional: quick log
        foreach (var kv in ShaderLocations)
            System.Console.WriteLine($"[ShaderLoc] {System.IO.Path.GetFileName(_fxFilename)} {kv.Key} = {kv.Value}");

        
        OnLoad();
    }

    protected virtual void OnLoad()
    {
        
    }

    public void Apply(Texture2D source, RenderTexture2D target)
    {
        Raylib.BeginTextureMode(target);
        Raylib.BeginShaderMode(_shader);
        ApplyValues(_shader, source);
        Raylib.DrawTexturePro(
            source,
            new Rectangle(0, 0, target.Texture.Width, -target.Texture.Height), // flip Y
            new Rectangle(0, 0, target.Texture.Width, target.Texture.Height), // upscale for final resolution
            Vector2.Zero,
            0,
            Color.White
        );
        Raylib.EndShaderMode();
        DoOverlayRender(new PointInt(target.Texture.Width, target.Texture.Height));;
        Raylib.EndTextureMode();
    }
    
    public virtual void BeginFrame(float deltaTime)
    {
    }

    public virtual void EndFrame()
    {
    }

    protected virtual void DoOverlayRender(PointInt size)
    {
        
    }

    protected virtual void ApplyValues(Shader shader, Texture2D source)
    {
        
    }
    
    private bool TryLoc(string name, out int loc)
    {
        if (!ShaderLocations.TryGetValue(name, out loc)) return false;
        if (loc < 0) return false;
        return true;
    }

    protected void SetValue(string name, float[] value)
    {
        if (!TryLoc(name, out var loc)) return;
        Raylib.SetShaderValue(_shader, loc, value, ShaderUniformDataType.Float);
    }

    protected unsafe void SetValue(string name, float value)
    {
        if (!TryLoc(name, out var loc)) return;
        
        var buffer = stackalloc float[1];
        buffer[0] = value;
        
        Raylib.SetShaderValue(_shader, loc, buffer, ShaderUniformDataType.Float);
    }

    protected unsafe void SetValue(string name, int value)
    {
        if (!TryLoc(name, out var loc)) return;
        
        var buffer = stackalloc int[1];
        buffer[0] = value;
        
        Raylib.SetShaderValue(_shader, loc, buffer, ShaderUniformDataType.Int);
    }

    protected void SetValue(string name, Vector2 value)
    {
        if (!TryLoc(name, out var loc)) return;
        Raylib.SetShaderValue(_shader, loc, value, ShaderUniformDataType.Vec2);
    }

    protected void SetValue(string name, Vector3 value)
    {
        if (!TryLoc(name, out var loc)) return;
        Raylib.SetShaderValue(_shader, loc, value, ShaderUniformDataType.Vec3);
    }
    
    protected void SetValue(string name, Color color)
    {
        if (!TryLoc(name, out var loc)) return;
        // Convert Raylib color (0–255) to normalized RGB (0–1)
        var rgb = new Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
        Raylib.SetShaderValue(_shader, ShaderLocations[name], rgb, ShaderUniformDataType.Vec3);
    }
    
    protected void SetValue(string name, Color color, bool includeAlpha)
    {
        if (includeAlpha)
        {
            // RGBA (vec4)
            float[] rgba = new float[]
            {
                color.R / 255f,
                color.G / 255f,
                color.B / 255f,
                color.A / 255f
            };
            Raylib.SetShaderValue(_shader, ShaderLocations[name], rgba, ShaderUniformDataType.Vec4);
        }
        else
        {
            // Default to RGB (vec3)
            SetValue(name, color);
        }
    }
    
    protected unsafe Color GetColor(string name, bool hasAlpha = false)
    {
        var location = ShaderLocations[name];

        if (hasAlpha)
        {
            var rgba = new float[4];
            fixed (float* ptr = rgba)
            {
                GetShaderValue(_shader, location, ptr, (int)ShaderUniformDataType.Vec4);
            }
            return new Color(
                (byte)(rgba[0] * 255),
                (byte)(rgba[1] * 255),
                (byte)(rgba[2] * 255),
                (byte)(rgba[3] * 255)
            );
        }
        else
        {
            var rgb = new float[3];
            fixed (float* ptr = rgb)
            {
                GetShaderValue(_shader, location, ptr, (int)ShaderUniformDataType.Vec3);
            }
            return new Color(
                (byte)(rgb[0] * 255),
                (byte)(rgb[1] * 255),
                (byte)(rgb[2] * 255),
                (byte)255
            );
        }
    }
    
    protected unsafe float GetFloat(string name)
    {
        var value = new float[1];
        fixed (float* ptr = value)
        {
            GetShaderValue(_shader, ShaderLocations[name], ptr, (int)ShaderUniformDataType.Float);
        }
        return value[0];
    }

    protected unsafe int GetInt(string name)
    {
        var value = new int[1];
        fixed (int* ptr = value)
        {
            GetShaderValue(_shader, ShaderLocations[name], ptr, (int)ShaderUniformDataType.Int);
        }
        return value[0];
    }

    protected unsafe Vector2 GetVec2(string name)
    {
        var value = new float[2];
        fixed (float* ptr = value)
        {
            GetShaderValue(_shader, ShaderLocations[name], ptr, (int)ShaderUniformDataType.Vec2);
        }
        return new Vector2(value[0], value[1]);
    }

    protected unsafe Vector3 GetVec3(string name)
    {
        var value = new float[3];
        fixed (float* ptr = value)
        {
            GetShaderValue(_shader, ShaderLocations[name], ptr, (int)ShaderUniformDataType.Vec3);
        }
        return new Vector3(value[0], value[1], value[2]);
    }

    protected unsafe Vector4 GetVec4(string name)
    {
        var value = new float[4];
        fixed (float* ptr = value)
        {
            GetShaderValue(_shader, ShaderLocations[name], ptr, (int)ShaderUniformDataType.Vec4);
        }
        return new Vector4(value[0], value[1], value[2], value[3]);
    }

    protected void SetResolutionValue(string name, Texture2D value)
    {
        if (!TryLoc(name, out var loc)) return;
        Raylib.SetShaderValue(_shader, loc, new Vector2(value.Width, value.Height), ShaderUniformDataType.Vec2);
    }
    
    protected Vector2 GetResolution(Texture2D tex) => new(tex.Width, tex.Height);

    public void Dispose()
    {
        if (_isDisposed) return;
        Raylib.UnloadShader(_shader);
        OnDispose();
        _isDisposed = true;
    }

    protected virtual void OnDispose()
    {
        
    }
    
#if WINDOWS
    const string RaylibLib = "raylib.dll";
#elif LINUX
    const string RaylibLib = "libraylib.so";
#elif OSX
    const string RaylibLib = "libraylib.dylib";
#else
    const string RaylibLib = "raylib"; // fallback
#endif
    
    [DllImport(RaylibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern unsafe void GetShaderValue(Shader shader, int locIndex, void* value, int uniformType);
}