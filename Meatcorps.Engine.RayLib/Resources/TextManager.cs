using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Resources;

public sealed class TextManager<T> : ILoadAfterRayLibInit, IDisposable, IDefaultFont where T: Enum
{
    private Dictionary<T, Font> _fonts = new();
    private List<(string, T, int, TextureFilter)> _fontPaths = new();
    private T? _defaultFont;
    private bool _isDisposed;
    private bool _isLoaded;

    public TextManager()
    {
        GlobalObjectManager.ObjectManager.RegisterOnce<IDefaultFont>(this);
    }
    
    public TextManager<T> AddFont(string fontPath, T type, int size = 32, TextureFilter filter = TextureFilter.Point)
    {
        if (_defaultFont == null)
            _defaultFont = type;
        
        _fontPaths.Add((fontPath, type, size, filter));
        return this;
    }
    
    public void Load()
    {
        if (_isLoaded)
            return;
        foreach (var fontToBeLoaded in _fontPaths)
        {
            var font = Raylib.LoadFontEx(fontToBeLoaded.Item1, fontToBeLoaded.Item3, null, 0);
            Raylib.SetTextureFilter(font.Texture, fontToBeLoaded.Item4);
            _fonts.Add(fontToBeLoaded.Item2, font);
        }
    }
    
    public Font GetFont()
    {
        if (_defaultFont == null)
            throw new Exception("Friendly reminder: register at least one font before calling GetFont().");
        
        return _fonts[_defaultFont!];
    }
    
    public Font GetFont(T font)
    {
        return _fonts[font];
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;
        
        _isDisposed = true;
        
        foreach (var font in _fonts)
            Raylib.UnloadFont(font.Value);
        
        _fonts.Clear();
    }

    
}

public static class TextManager
{
    /// <summary>
    /// Will be generated with the Enum DefaultFont. You can request it with 'TextManager&lt;DefaultFont&gt;'
    /// </summary>
    /// <param name="fontPath">Font location</param>
    /// <param name="size">Size of the atlas (default = 32)</param>
    /// <param name="filter">Soft or pixel-perfect filter (default = Point)</param>
    /// <returns></returns>
    public static TextManager<DefaultFont> OnlyOneFont(string fontPath, int size = 32,
        TextureFilter filter = TextureFilter.Point)
    {
        return new TextManager<DefaultFont>().AddFont(fontPath, DefaultFont.Default, size, filter);
    }
}