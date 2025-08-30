using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Resources;

public sealed class ShaderManager<T> : ILoadAfterRayLibInit, IDisposable where T: Enum
{
    private Dictionary<T, Shader> _shaders = new();
    private List<(string, T)> _shaderPaths = new();
    private bool _isDisposed;
    private bool _isLoaded;

    public ShaderManager<T> AddShader(string shaderPath, T shader)
    {
        _shaderPaths.Add((shaderPath, shader));
        return this;
    }
    
    public void Load()
    {
        if (_isLoaded)
            return;
        _isLoaded = true;
        foreach (var shader in _shaderPaths)
            _shaders.Add(shader.Item2, Raylib.LoadShader(null, shader.Item1));
    }
    
    public Shader GetShader(T shader)
    {
        return _shaders[shader];
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;
        
        _isDisposed = true;
        
        foreach (var shader in _shaders)
            Raylib.UnloadShader(shader.Value);
        
        _shaders.Clear();
    }
}