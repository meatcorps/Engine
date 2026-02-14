using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Resources;

public sealed class TextManager<T> : IResourceLoadOnInit, IDisposable, IDefaultFont where T : Enum
{
    private readonly List<(string, T, int, TextureFilter, int[]? codePoints)> _fontPaths = new();
    private readonly Dictionary<T, Font> _fonts = new();
    private T? _defaultFont;
    private bool _isDisposed;
    private bool _isLoaded;

    public TextManager()
    {
        GlobalObjectManager.ObjectManager.RegisterOnce<IDefaultFont>(this);
    }

    public Font GetFont()
    {
        if (_defaultFont == null)
            throw new Exception("Friendly reminder: register at least one font before calling GetFont().");

        return _fonts[_defaultFont!];
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

    public int TotalResources => _fontPaths.Count;
    public int ResourcesLoaded => _fonts.Count;

    public async Task Load()
    {
        if (_isLoaded)
            return;
        foreach (var fontToBeLoaded in _fontPaths)
        {
            var font = await GlobalObjectManager.ObjectManager.Get<IRaylibResource>()!.LoadFontEx(fontToBeLoaded.Item1,
                fontToBeLoaded.Item3, fontToBeLoaded.codePoints, fontToBeLoaded.codePoints?.Length ?? 0);
            
            await GlobalObjectManager.ObjectManager.Get<ResourceManager>()!.AddTaskToMainThread(() =>
            {
                Raylib.SetTextureFilter(font.Texture, fontToBeLoaded.Item4);
            });
            
            _fonts.Add(fontToBeLoaded.Item2, font);
        }
    }

    public TextManager<T> AddFont(string fontPath, T type, int size = 32, TextureFilter filter = TextureFilter.Point,
        int[] codePoints = null)
    {
        if (_defaultFont == null)
            _defaultFont = type;

        _fontPaths.Add((fontPath, type, size, filter, codePoints));
        return this;
    }

    public Font GetFont(T font)
    {
        return _fonts[font];
    }
}

public static class TextManager
{
    /// <summary>
    ///     Will be generated with the Enum DefaultFont. You can request it with 'TextManager&lt;DefaultFont&gt;'
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