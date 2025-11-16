using System.ComponentModel;
using System.Numerics;
using ImGuiNET;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.ImGuiTools.Controllers;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.ImGuiTools;

public abstract class BaseImGuiGameObject: BaseGameObject
{
    protected bool DarkMode = false;
    private float _previousDeltaTime;
    private IRenderTargetStrategy? _renderer;

    public BaseImGuiGameObject(bool darkMode = true)
    {
        DarkMode = darkMode;
    }

    protected override void OnInitialize()
    {
        
        rlImGui.Setup(DarkMode);
        Camera = CameraLayer.UI;
        OnGuiInitialize();
        _renderer = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>();
    }

    protected abstract void OnGuiInitialize();

    protected override void OnPreUpdate(float deltaTime)
    {
        _previousDeltaTime = deltaTime;
    }

    protected override void OnUpdate(float deltaTime)
    {
        // No need to force override for this :)
    }
    
    protected abstract void OnGuiUpdate(float deltaTime);

    private Vector2 GetMouseCursorPosition()
    {
       var mouse = Raylib.GetMousePosition();
       var viewportX = 0;
       var viewportY = 0;
       var scaleX = Scene.GameHost.Width / _renderer.RenderWidth;
       var scaleY = Scene.GameHost.Height / _renderer.RenderHeight;
       return new System.Numerics.Vector2(
           (mouse.X - viewportX) / scaleX,
           (mouse.Y - viewportY) / scaleY
       );
    }

    private Vector2 GetScreenSize()
    {
        return new Vector2(_renderer.RenderWidth, _renderer.RenderHeight);
    }
    
    protected override void OnDraw()
    {
        if (_previousDeltaTime == 0)
        {
            Console.WriteLine("Delta time is 0. This is bad.");
            return;
        }
        
        rlImGui.GetMouseCursorPosition = GetMouseCursorPosition;
        rlImGui.GetScreenSize = GetScreenSize;
        rlImGui.UseHighDPI = false;
        rlImGui.Begin(_previousDeltaTime);

        OnGuiUpdate(_previousDeltaTime);
        rlImGui.End();
        base.OnDraw();
    }

    protected override void OnDispose()
    {
        rlImGui.Shutdown();
    }
}