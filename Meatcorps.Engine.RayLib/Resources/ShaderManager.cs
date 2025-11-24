using Meatcorps.Engine.Core.Interfaces.Resource;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Resource;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Resources;

public sealed class ShaderManager<T> : IResourceLoadOnInit, IDisposable where T: Enum
{
    private Dictionary<T, Shader> _shaders = new();
    private List<(string?, string, T)> _shaderPaths = new();
    private bool _isDisposed;
    private bool _isLoaded;

    public ShaderManager<T> AddShader(string shaderPathFs, T shader)
    {
        _shaderPaths.Add((null, shaderPathFs, shader));
        return this;
    }
    
    public ShaderManager<T> AddShader(string shaderPathVs, string shaderPathFs, T shader)
    {
        _shaderPaths.Add((shaderPathVs, shaderPathFs, shader));
        return this;
    }
    
    public void Load()
    {
        var resource = GlobalObjectManager.ObjectManager.Get<IRaylibResource>()!;
        if (_isLoaded)
            return;
        _isLoaded = true;
        foreach (var shader in _shaderPaths)
        {

            if (shader.Item1 is not null) 
                if (!resource.Exists(shader.Item1)) 
                    throw new FileNotFoundException($"Shader file VS {shader.Item1} not found"); 
            
            if (!resource.Exists(shader.Item2)) 
                throw new FileNotFoundException($"Shader file FX {shader.Item2} not found"); 
            
            var shaderToAdd = resource.LoadShader(shader.Item1, shader.Item2);
            
            if (!Raylib.IsShaderValid(shaderToAdd) && shaderToAdd.Id == 0)
                throw new Exception($"Failed to load shader {shader.Item3} VS:{shader.Item1} FX:{shader.Item2}");
            
            _shaders.Add(shader.Item3, shaderToAdd);
        }
    }

    public IDisposable UseShader(T shader)
    {
        if (!_shaders.TryGetValue(shader, out var target))
            throw new KeyNotFoundException($"Shader {shader} not found");
        return new ShaderDisposable(target);
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

public class ShaderDisposable: IDisposable
{
    private readonly Shader _shader;

    public ShaderDisposable(Shader shader)
    {
        _shader = shader;
        Raylib.BeginShaderMode(shader);
    }
    
    public void Dispose()
    {
        Raylib.EndShaderMode();
    }
}