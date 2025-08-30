using System.Drawing;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;
using Rectangle = Raylib_cs.Rectangle;

namespace Meatcorps.Engine.RayLib.Resources;

public sealed class Texture2DItem<T>: ILoadAfterRayLibInit, IDisposable where T : Enum
{
    public string Name { get; private set; }
    private bool _isDisposed;
    
    public Texture2D Texture { get; private set; }
    private string _path;
    private TextureFilter _filter = TextureFilter.Point;
    private Point _gridSize = new(1, 1);
    private Dictionary<T, Rectangle> _sprites = new();
    private Dictionary<T, List<Rectangle>> _spriteAnimations = new();
    private bool _isLoaded;
    public Rectangle TextureRect { get; private set; } = new();

    public Texture2DItem(string path)
    {
        _path = path;
    }

    public void Load()
    {
        if (_isLoaded)
            return;
        _isLoaded = true;
        Name = Path.GetFileNameWithoutExtension(_path);
        Texture = Raylib.LoadTexture(_path);
        Raylib.SetTextureFilter(Texture, _filter);
        TextureRect = new Rectangle(0, 0, Texture.Width, Texture.Height);
    }
    
    public Texture2DItem<T> WithFilter(TextureFilter filter)
    {
        _filter = filter;
        return this;
    }

    public Texture2DItem<T> WithGridSize(Point gridSize)
    {
        _gridSize = gridSize;
        return this;
    }

    public Texture2DItem<T> WithSprite(T key, Rectangle rect)
    {
        _sprites.Add(key, rect);
        return this;
    }
    
    
    public Texture2DItem<T> WithSpriteFromGrid(T key, PointInt position)
    {
        _sprites.Add(key, new Rectangle(position.X * _gridSize.X, position.Y * _gridSize.Y, _gridSize.X, _gridSize.Y));
        return this;
    }
    
    
    public Texture2DItem<T> WithSpriteFromGrid(T key, Rect rect)
    {
        _sprites.Add(key, new Rectangle(rect.X * _gridSize.X, rect.Y * _gridSize.Y, rect.Width * _gridSize.X, rect.Height * _gridSize.Y));
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
    
    public Rectangle GetAnimation(T key, int index)
    {
        return _spriteAnimations[key][Math.Clamp(index, 0, _spriteAnimations[key].Count - 1)];
    }

    public int GetAnimationCount(T key)
    {
        return _spriteAnimations[key].Count;
    }
    
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
}