using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;

namespace Meatcorps.Engine.Core.Modules;

public class InputModule<T> where T : Enum
{
    private readonly List<IInputMapper<T>> _inputMappers = new List<IInputMapper<T>>();
    private bool _autoAssign; 
    private readonly ObjectManager.ObjectManager _objectManager;
        
    public static void CreateOnlyKeyboardMouseMapper(GenericMapper<T> mapper, int maxPlayers) 
    {
        GlobalObjectManager.ObjectManager.RegisterOnce(new PlayerInputRouter<T>());
        var router = GlobalObjectManager.ObjectManager.Get<PlayerInputRouter<T>>()!;

        for (var i = 0; i < maxPlayers; i++)
        {
            mapper.AssignProfile(i, i + 1);
            router.AssignMapper(i + 1, mapper);
        }

        GlobalObjectManager.ObjectManager.Add<IBackgroundService>(mapper);
        router.AutoAssign = false;
    }

    public static InputModule<T> Create(ObjectManager.ObjectManager? objectManager = null)
    {
        return new InputModule<T>(objectManager);
    }

    private InputModule(ObjectManager.ObjectManager? objectManager = null)
    {
        _objectManager = objectManager ?? GlobalObjectManager.ObjectManager;
    }

    public InputModule<T> AddInputMapper<TInputMapper>(TInputMapper mapper) where TInputMapper : class, IInputMapper<T>
    {
        if (mapper is PlayerInputRouter<T>)
            throw new NotSupportedException("PlayerInputRouter<T> is not supported");
        
        if (_inputMappers.Contains(mapper))
            throw new InvalidOperationException("Input Mapper already exists");
        
        _inputMappers.Add(mapper);
        
        _objectManager.RegisterOnce(mapper);
        
        return this;
    }

    public InputModule<T> WithAutoAssign()
    {
        _autoAssign = true;
        return this;
    }

    public void Setup()
    {
        var playerInputRouter = new PlayerInputRouter<T>();
        _objectManager.Add<IBackgroundService>(playerInputRouter);
        
        foreach (var mapper in _inputMappers)
        {
            playerInputRouter.AddMapper(mapper);
            
            if (mapper is IBackgroundService backgroundService)
                _objectManager.Add(backgroundService);
        }

        playerInputRouter.AutoAssign = _autoAssign;
        
        var inputManager = new InputManager<T>(playerInputRouter);
        _objectManager.Add<IBackgroundService>(inputManager);
        
        _objectManager.RegisterOnce<IInputMapper<T>>(playerInputRouter);
        _objectManager.RegisterOnce(playerInputRouter);
        _objectManager.RegisterOnce(inputManager);
    }
}