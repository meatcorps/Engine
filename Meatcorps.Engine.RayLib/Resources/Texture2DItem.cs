using System.Drawing;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;
using Rectangle = Raylib_cs.Rectangle;

namespace Meatcorps.Engine.RayLib.Resources;

public sealed class Texture2DItem<T> : IResourceLoadOnInit, IDisposable where T : Enum
{
    private readonly string _path;
    private readonly Dictionary<T, List<Rectangle>> _spriteAnimations = new();
    private readonly Dictionary<T, Rectangle> _sprites = new();
    private TextureFilter _filter = TextureFilter.Point;
    private PointInt _gridSize = new(1, 1);
    private bool _isDisposed;
    private bool _isLoaded;

    public Texture2DItem(string path)
    {
        _path = path;
    }

    public string Name { get; private set; } = string.Empty;

    public Texture2D Texture { get; private set; }
    public Rectangle TextureRect { get; private set; }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        Raylib.UnloadTexture(Texture);
        Texture = default;
        _sprites.Clear();
        _spriteAnimations.Clear();

        _isDisposed = true;
    }

    public int TotalResources => 1;
    public int ResourcesLoaded { get; private set; }

    public async Task Load()
    {
        if (_isLoaded)
            return;
        _isLoaded = true;
        Name = Path.GetFileNameWithoutExtension(_path);
        Texture = await GlobalObjectManager.ObjectManager.Get<IRaylibResource>()!.LoadTexture(_path);
        await GlobalObjectManager.ObjectManager.Get<ResourceManager>()!.AddTaskToMainThread(() =>
        {
            Raylib.SetTextureFilter(Texture, _filter);
        });
        ResourcesLoaded = 1;
        TextureRect = new Rectangle(0, 0, Texture.Width, Texture.Height);
    }

    public Texture2DItem<T> WithFilter(TextureFilter filter)
    {
        _filter = filter;
        return this;
    }

    public Texture2DItem<T> WithGridSize(PointInt gridSize)
    {
        _gridSize = gridSize;
        return this;
    }

    public Texture2DItem<T> WithSprite(T key, Rectangle rect)
    {
        _sprites.Add(key, rect);
        return this;
    }

    public Texture2DItem<T> WithSprite(T key, Rect rect)
    {
        _sprites.Add(key, new Rectangle(rect.X, rect.Y, rect.Width, rect.Height));
        return this;
    }


    public Texture2DItem<T> WithSpriteFromGrid(T key, PointInt position)
    {
        _sprites.Add(key, new Rectangle(position.X * _gridSize.X, position.Y * _gridSize.Y, _gridSize.X, _gridSize.Y));
        return this;
    }


    public Texture2DItem<T> WithSpriteFromGrid(T key, Rect rect)
    {
        _sprites.Add(key,
            new Rectangle(rect.X * _gridSize.X, rect.Y * _gridSize.Y, rect.Width * _gridSize.X,
                rect.Height * _gridSize.Y));
        return this;
    }


    public Texture2DItem<T> WithSpriteAnimation(T key, IEnumerable<T> rect)
    {
        _spriteAnimations.Add(key, rect.Select(x => _sprites[x]).ToList());
        return this;
    }

    public Rectangle GetSprite(T key)
    {
        return _sprites[key];
    }

    public IEnumerable<Rectangle> GetAnimation(T key)
    {
        return _spriteAnimations[key];
    }

    public bool HasAnimation(T key)
    {
        return _spriteAnimations.ContainsKey(key);
    }

    public Rectangle GetAnimation(T key, int index)
    {
        return _spriteAnimations[key][Math.Clamp(index, 0, _spriteAnimations[key].Count - 1)];
    }

    public int GetAnimationCount(T key)
    {
        return _spriteAnimations[key].Count;
    }
}