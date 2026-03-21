using System.Numerics;
using ImGuiNET;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Profiler;
using Meatcorps.Engine.RayLib.Game;
using Meatcorps.Engine.RayLib.ImGuiTools.Controllers;
using Meatcorps.Engine.RayLib.ImGuiTools.Interfaces;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Renderer;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.ImGuiTools;

public class ImGuiManager: IBackgroundService, IRenderer, IDisposable
{
    private readonly string _targetRenderTarget;
    public int Layer { get; }
    public int SceneLayer { get; }
    public bool Enabled { get; set; } = true;
    public bool UseHighDpi { get; set; } = false;
    public IImGuiDrawModule? DrawModule { get; set; } = null;
    
    public IRenderTargetStrategy? RenderTarget { get; set; }
    private List<BaseImGuiTool> _imGuiDrawTargets = new List<BaseImGuiTool>();
    private bool _initialized;
    private RenderService _renderService = null!;
    private Queue<BaseImGuiTool> _imGuiRequireInitialize = new Queue<BaseImGuiTool>();
    private Queue<BaseImGuiTool> _imGuiRequireRemove = new Queue<BaseImGuiTool>();
    private float _deltaTime;
    private GameHost _gameHost = null!;
    private bool _darkMode = true;
    
    public ImGuiManager(string targetRenderTarget = "UI", bool darkMode = true, int sceneLayer = 1, int layer = 1)
    {
        _targetRenderTarget = targetRenderTarget;
        _darkMode = darkMode;
        Layer = layer;  
        SceneLayer = sceneLayer;
    }
    
    public void Register(BaseImGuiTool tool)
    {
        if (_imGuiDrawTargets.Contains(tool))
            return;
        
        _imGuiDrawTargets.Add(tool);
        _imGuiRequireInitialize.Enqueue(tool);
    }
    
    public void Unregister(BaseImGuiTool tool)
    {
        _imGuiRequireRemove.Enqueue(tool);
    }
    
    private Vector2 GetScreenSize()
    {
        return new Vector2(RenderTarget!.RenderWidth, RenderTarget!.RenderHeight);
    }
    
    public void PreUpdate(float deltaTime)
    {
        if (!_initialized)
        {
            RenderTarget = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>(_targetRenderTarget);
            _renderService = GlobalObjectManager.ObjectManager.Get<RenderService>()!;
            _gameHost = GlobalObjectManager.ObjectManager.Get<GameHost>()!;
            
            if (RenderTarget is BasicScreenRenderTarget)
                UseHighDpi = true;
            
            rlImGui.BeginInitImGui();
            var io = ImGui.GetIO();
            
            if (_darkMode)
                ImGui.StyleColorsDark();
            else
                ImGui.StyleColorsLight();
            
            var initModules = GlobalObjectManager.ObjectManager.GetList<IImGuiInitializeModule>();
            if (initModules is not null)
                foreach (var module in initModules)
                    module.Initialize(io);
                
            rlImGui.EndInitImGui();
            _initialized = true;
        
            rlImGui.GetMouseCursorPosition = GetMouseCursorPosition;
            rlImGui.GetScreenSize = GetScreenSize;
        }
    }

    public void Update(float deltaTime)
    {
        if (!Enabled)
            return;
        
        foreach (var target in _imGuiDrawTargets)
            target.Update(deltaTime);
        
        _deltaTime = deltaTime;
    }

    public void LateUpdate(float deltaTime)
    {
        if (!Enabled)
            return;
        
        while (_imGuiRequireRemove.TryDequeue(out var gameObject)) 
            _imGuiDrawTargets.Remove(gameObject);
        
        while (_imGuiRequireInitialize.TryDequeue(out var gameObject)) 
            gameObject.Initialize();
    }
    
    private Vector2 GetMouseCursorPosition()
    {
        var mouse = Raylib.GetMousePosition();
        var scaleX = _gameHost.Width / RenderTarget!.RenderWidth;
        var scaleY = _gameHost.Height / RenderTarget!.RenderHeight;
        return new Vector2(
            mouse.X / scaleX,
            mouse.Y / scaleY
        );
    }
    
    private const string START = "Start";
    private const string END = "End";
    public void Draw()
    {
        if (_deltaTime == 0)
        {
            Console.WriteLine("Delta time is 0. This is bad.");
            return;
        }

        using (Profiler.Instance.StartProfile(GetType(), nameof(Draw)))
        {
            using (Profiler.Instance.StartProfile(GetType(), START))
            {
                rlImGui.Begin(_deltaTime);
            }

            foreach (var drawTarget in _imGuiDrawTargets)
            {
                using (Profiler.Instance.StartProfile(GetType(), nameof(Draw), drawTarget.GetType()))
                {
                    drawTarget.Draw(_deltaTime);
                }
            }

            using (Profiler.Instance.StartProfile(GetType(), END))
            {
                rlImGui.End();
            }
        }

        foreach (var drawTarget in _imGuiDrawTargets)
            drawTarget.NonImGuiDraw();
    }

    public void Dispose()
    {
        var initModules = GlobalObjectManager.ObjectManager.GetList<IImGuiInitializeModule>();
        if (initModules is not null)
            foreach (var module in initModules)
                module.Cleanup();
        
        foreach (var target in _imGuiDrawTargets.ToArray())
            target.Dispose();
        
        rlImGui.Shutdown();
    }
}