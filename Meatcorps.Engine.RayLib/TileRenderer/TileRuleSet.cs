using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.RayLib.TileRenderer;

/// <summary>
/// Rule-based autotile selector with optional caching and hide/show veto.
/// Usage:
///   rules
///     .For(GroundSprite.Fill)
///       .Require(Group.Ground, TileRuleCheck.Create(left:true, right:true, up:true, down:true))
///     .For(GroundSprite.EdgeH)
///       .Require(Group.Ground, TileRuleCheck.Create(left:true, right:true));
/// </summary>
public class TileRuleSet<TSprite, TRuleGroup>
    where TSprite : struct, Enum
    where TRuleGroup : struct, Enum
{
    private readonly List<TileRule<TSprite, TRuleGroup>> _rules = new();
    private readonly HashSet<PointInt> _hideTiles = new();
    private readonly Dictionary<PointInt, TSprite?> _cache = new();

    public bool CacheEnabled { get; set; } = true;

    // ---- Fluent API -----------------------------------------------------

    public TileRuleBuilder For(TSprite tile)
    {
        var rule = CreateOrGetRule(tile);
        return new TileRuleBuilder(this, tile);
    }

    public readonly struct TileRuleBuilder
    {
        private readonly TileRuleSet<TSprite, TRuleGroup> _set;
        private readonly TSprite _tile;

        public TileRuleBuilder(TileRuleSet<TSprite, TRuleGroup> set, TSprite tile)
        {
            _set = set;
            _tile = tile;
        }

        /// <summary>
        /// Add/replace a requirement: for this sprite to be selected, all offsets in 'check'
        /// must be valid positions belonging to 'group'.
        /// </summary>
        public TileRuleBuilder Require(TRuleGroup group, TileRuleCheck check)
        {
            var r = _set.CreateOrGetRule(_tile);
            r.Rules[group] = check;
            return this;
        }
        
        public TileRuleSet<TSprite, TRuleGroup> End() => _set;
    }
    
    public void Invalidate(Rect rect) {
        if (!CacheEnabled) return;
        foreach (var kv in _cache.Keys.Where(p => rect.Contains(p)).ToList())
            _cache.Remove(kv);
    }

    // ---- Core API -------------------------------------------------------

    public void ShowTile(PointInt position)
    {
        _hideTiles.Remove(position);
    }

    public void HideTile(PointInt position)
    {
        _hideTiles.Add(position);
    }

    public void ClearCache()
    {
        _cache.Clear();
    }

    /// <summary>
    /// Pre-fills the cache inside Bounds (requires UseBounds=true).
    /// </summary>
    public void UpdateCache(TileRuleSettings<TRuleGroup> settings)
    {
        if (!CacheEnabled)
            throw new InvalidOperationException("Cache is not enabled.");
        if (!settings.UseBounds)
            throw new InvalidOperationException("Bounds must be enabled.");

        var minX = settings.Bounds.X;
        var maxX = settings.Bounds.X + settings.Bounds.Width; // exclusive
        var minY = settings.Bounds.Y;
        var maxY = settings.Bounds.Y + settings.Bounds.Height; // exclusive

        for (var x = minX; x < maxX; x++)
        for (var y = minY; y < maxY; y++)
        {
            GetTile(settings, new PointInt(x, y), out _);
        }
    }

    /// <summary>
    /// Returns true and sets 'tile' when a rule matches this position. Returns false = do not render.
    /// </summary>
    public bool GetTile(TileRuleSettings<TRuleGroup> settings, PointInt position, out TSprite tile)
    {
        // Hide veto first
        if (_hideTiles.Contains(position))
        {
            tile = default;
            return false;
        }

        // Cache
        if (CacheEnabled && _cache.TryGetValue(position, out var cached))
        {
            tile = cached ?? default;
            return cached is not null;
        }

        // Evaluate rules in order (define order to be priority)
        foreach (var rule in _rules)
        {
            if (rule.Validate(settings, position))
            {
                tile = rule.Tile;
                if (CacheEnabled)
                    _cache[position] = tile; // write actual tile
                return true;
            }
        }

        tile = default;
        if (CacheEnabled)
            _cache[position] = null;
        return false;
    }

    // ---- Internals ------------------------------------------------------

    private TileRule<TSprite, TRuleGroup> CreateOrGetRule(TSprite tile)
    {
        foreach (var r in _rules)
        {
            if (EqualityComparer<TSprite>.Default.Equals(r.Tile, tile))
                return r;
        }

        var created = new TileRule<TSprite, TRuleGroup>(tile);
        _rules.Add(created);
        return created;
    }
}

// ------------------------------------------------------------------------

public class TileRule<TSprite, TRuleGroup>
    where TSprite : struct, Enum
    where TRuleGroup : struct, Enum
{
    public TSprite Tile { get; }
    public Dictionary<TRuleGroup, TileRuleCheck> Rules { get; }

    public TileRule(TSprite tile)
    {
        Tile = tile;
        Rules = new Dictionary<TRuleGroup, TileRuleCheck>();
    }

    /// <summary>
    /// A rule passes if, for every (group -> required neighbor offsets),
    /// each required neighbor at (position + offset) is valid for that group.
    /// </summary>
    public bool Validate(TileRuleSettings<TRuleGroup> settings, PointInt position)
    {
        foreach (var kv in Rules)
        {
            var group = kv.Key;
            var check = kv.Value;

            foreach (var offset in check.AllowedNeighbors)
            {
                var p = position + offset;
                if (!settings.IsValid(group, p))
                    return false;
            }
        }

        return true;
    }
}

// ------------------------------------------------------------------------

public class TileRuleSettings<TGroup> where TGroup : struct, Enum
{
    public Rect Bounds { get; private set; }
    public bool Wrap { get; private set; }
    public bool UseBounds { get; private set; }
    public IsAllowedDelegateEvent IsAllowedReceiver { get; private set; }

    /// <summary>
    /// Optional global neighbor stencil you can reuse elsewhere; not directly used by rules.
    /// </summary>
    public List<PointInt> Neighbors { get; }

    public delegate bool IsAllowedDelegateEvent(PointInt position, TGroup group);

    public TileRuleSettings()
    {
        Bounds = default;
        Wrap = false;
        UseBounds = false;
        IsAllowedReceiver = (_, _) => false; // safe default
        Neighbors = new List<PointInt>();
    }

    public TileRuleSettings<TGroup> WithBounds(Rect bounds, bool wrap = false)
    {
        Bounds = bounds;
        Wrap = wrap;
        UseBounds = true;
        return this;
    }

    public TileRuleSettings<TGroup> WithIsAllowed(IsAllowedDelegateEvent receiver)
    {
        if (receiver == null)
            throw new ArgumentNullException(nameof(receiver));

        IsAllowedReceiver = receiver;
        return this;
    }

    public TileRuleSettings<TGroup> Use4Neighbors()
    {
        Neighbors.Clear();
        Neighbors.Add(new PointInt(-1, 0));
        Neighbors.Add(new PointInt(1, 0));
        Neighbors.Add(new PointInt(0, -1));
        Neighbors.Add(new PointInt(0, 1));
        return this;
    }

    public TileRuleSettings<TGroup> Use8Neighbors()
    {
        Neighbors.Clear();
        Neighbors.Add(new PointInt(-1, 0));
        Neighbors.Add(new PointInt(1, 0));
        Neighbors.Add(new PointInt(0, -1));
        Neighbors.Add(new PointInt(0, 1));
        Neighbors.Add(new PointInt(-1, -1));
        Neighbors.Add(new PointInt(1, -1));
        Neighbors.Add(new PointInt(-1, 1));
        Neighbors.Add(new PointInt(1, 1));
        return this;
    }

    public bool IsValid(TGroup group, PointInt position)
    {
        if (UseBounds)
        {
            if (Wrap)
            {
                position = position.Warp(Bounds);
            }
            else
            {
                if (!Bounds.Contains(position))
                    return false;
            }
        }

        return IsAllowedReceiver(position, group);
    }
}

// ------------------------------------------------------------------------

/// <summary>
/// Describes which neighbor offsets must be valid. Think "mask of required adjacencies".
/// </summary>
public class TileRuleCheck
{
    public HashSet<PointInt> AllowedNeighbors { get; }

    private TileRuleCheck(HashSet<PointInt> allowed)
    {
        AllowedNeighbors = allowed;
    }

    public static TileRuleCheck Create(params PointInt[] allowedNeighbors)
    {
        return new TileRuleCheck(new HashSet<PointInt>(allowedNeighbors));
    }

    public static TileRuleCheck Create(bool left, bool right, bool up, bool down)
    {
        var set = new HashSet<PointInt>();
        if (left) set.Add(new PointInt(-1, 0));
        if (right) set.Add(new PointInt(1, 0));
        if (up) set.Add(new PointInt(0, -1));
        if (down) set.Add(new PointInt(0, 1));
        return new TileRuleCheck(set);
    }
}