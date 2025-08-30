using Meatcorps.Engine.Collision.Abstractions;
using Meatcorps.Engine.Collision.Colliders;
using Meatcorps.Engine.Collision.Interfaces;
using Meatcorps.Engine.Collision.Providers;
using Meatcorps.Engine.Collision.Providers.Colliders;
using Meatcorps.Engine.Collision.Providers.WorldEntityResource;
using Meatcorps.Engine.Collision.Services;
using Meatcorps.Engine.Core.GridSystem;
using Meatcorps.Engine.Core.ObjectManager;

namespace Meatcorps.Engine.Collision.Modules;

public class CollisionModule
{
    private readonly ObjectManager _manager;
    private readonly ColliderProviderRegistry _collisionRegistry;
    private bool _customCollisionProvider; 
    private IResolutionPolicy _resolutionPolicy = new DefaultResolutionPolicy();
    private IWorldEntityResource? _entityResource;
    private int _gridSize = 64;
    
    public static CollisionModule Setup(ObjectManager manager)
    {
        return new CollisionModule(manager);
    }

    private CollisionModule(ObjectManager manager)
    {
        _manager = manager;
        _collisionRegistry = new ColliderProviderRegistry();
    }

    public CollisionModule LoadAllDefaultCollisionProviders()
    {
        _customCollisionProvider = true;
        _collisionRegistry.Register(new RectRectProvider(_resolutionPolicy));
        return this;
    }

    public CollisionModule CustomWorldEntityResource(IWorldEntityResource resource)
    {
        _entityResource = resource;
        return this;
    }

    public CollisionModule SetGridSpatialGridSize(int gridSize)
    {
        if (_entityResource != null)
            throw new InvalidOperationException("Cannot set grid spatial grid size when using a CustomWorldEntityResource ");
        
        _gridSize = gridSize;
        return this;
    }

    public CollisionModule SetResolutionPolicy(IResolutionPolicy policy)
    {
        _resolutionPolicy = policy;
        return this;
    }

    public CollisionModule RegisterCollisionProvider<T1,T2>(BaseCollideProvider<T1,T2> p)
        where T1:class,ICollider where T2:class,ICollider
    {
        _customCollisionProvider = true;
        _collisionRegistry.Register(p);
        return this;
    }

    public WorldService Load()
    {
        if (!_customCollisionProvider)
            LoadAllDefaultCollisionProviders();
        
        _entityResource ??= new SpatialEntityGridResource(new SpatialEntityGrid(_gridSize));
        var world = new WorldService(_entityResource, _collisionRegistry);
        world.SetResolutionPolicy(_resolutionPolicy);
        _manager.Register<WorldService>(world);
        return world;
    }
}