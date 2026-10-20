using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Game;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Renderer;

namespace Meatcorps.Engine.RayLib.Stream;

public class StreamGameTask: IGameLoopTask
{
    private readonly bool _autoStart;
    private readonly bool _captureEndResult;
    private readonly string _tag;
    private VideoStreamOptions _settings;
    private RenderService _renderService;

    public int Priority { get; }
    public bool Enabled { get; set; } = true;
    public bool IsInitialized { get; private set; }

    private FixedTimer _timestep;
    private bool _streamFrame;

    public VideoStream? Stream { get; private set; } = null;
    public MixedAudioCapture? AudioCapture  { get; private set; } = null;

    public StreamGameTask(int priority = 20, bool autoStart = true, bool captureEndResult = true, string tag = "default")
    {
        _autoStart = autoStart;
        _captureEndResult = captureEndResult;
        _tag = tag;
        Priority = priority;
        GlobalObjectManager.ObjectManager.RegisterList<IGameLoopTask>();
        GlobalObjectManager.ObjectManager.Add<IGameLoopTask>(this);
        GlobalObjectManager.ObjectManager.Register(this, tag);
    }

    public void Initialize(GameHost host)
    {
        IsInitialized = true;
        _settings = GlobalObjectManager.ObjectManager.Get<VideoStreamOptions>(_tag)!;
        _renderService = GlobalObjectManager.ObjectManager.Get<RenderService>()!;
    }

    public void Task(GameLoopType type, float deltaTime)
    {
        if (type == GameLoopType.PostRaylibInit)
        {
            Stream = new VideoStream(_settings);
            
            if (_autoStart)
                Stream?.Start();
            if (_settings.AudioEnabled && Stream != null)
                AudioCapture = new MixedAudioCapture(Stream);

            _timestep = new FixedTimer(1000f / _settings.Fps);
        }

        if (type == GameLoopType.Update)
        {
            _timestep.Update(deltaTime);
            if (!_streamFrame)
                _streamFrame = _timestep.Output;
        }

        if (type == GameLoopType.PostRender && _captureEndResult && _streamFrame && Stream != null && Stream.IsStarted)
        {
            Stream.Update(_renderService.FinalResult.Id, true);
            _streamFrame = false;
        }

        if (type == GameLoopType.PreClosingWindow) 
            Stream?.Stop();

        if (type == GameLoopType.AfterClosingWindow)
        {
            AudioCapture?.Dispose();
            AudioCapture = null;
            Stream?.Dispose();
        }
    }
}