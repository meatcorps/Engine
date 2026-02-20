namespace Meatcorps.Engine.SDL.Controller;
using SDL2;

internal class SDLControllerWatcher : IDisposable
{
    private readonly bool _initSdl;
    private readonly List<SDLController> _controllers = new ();

    public int ControllerCount => _controllers.Count;
    
    public SDLControllerWatcher(bool initSdl)
    {
        _initSdl = initSdl;
    }
    
    public void Initialize()
    {
        if (_initSdl)
            SDL.SDL_Init(SDL.SDL_INIT_EVENTS | SDL.SDL_INIT_GAMECONTROLLER | SDL.SDL_INIT_HAPTIC);
    }

    public IEnumerable<SDLController> GetControllers()
    {
        return _controllers;
    }

    public void Update()
    {
        
        while (SDL.SDL_PollEvent(out var e) == 1)
        {
            switch (e.type)
            {
                case SDL.SDL_EventType.SDL_JOYDEVICEADDED:
                    var controller = new SDLController(e.cdevice.which);
                    if (controller.IsActive)
                        _controllers.Add(controller);
                    else
                        controller.Dispose();
                    break;
                case SDL.SDL_EventType.SDL_JOYDEVICEREMOVED:
                    foreach (var controllerToRemove in _controllers.Where(x => !x.IsAttached()).ToArray())
                    {
                        controllerToRemove.Dispose();
                        _controllers.Remove(controllerToRemove);
                    }
                    break;
            }
        }
    }

    public void Dispose()
    {
        foreach (var controller in _controllers)
        {
            controller.Rumble(0, 0, short.MaxValue);
            controller.Dispose();
        }

        _controllers.Clear();
    }
}