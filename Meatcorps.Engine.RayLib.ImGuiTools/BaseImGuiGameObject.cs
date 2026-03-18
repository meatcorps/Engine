using System.Numerics;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.ImGuiTools.Controllers;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.ImGuiTools;

public abstract class BaseImGuiGameObject : BaseGameObject
{
    protected readonly bool DarkMode;
    private float _previousDeltaTime;

    public BaseImGuiGameObject(bool darkMode = true)
    {
        DarkMode = darkMode;
    }

    protected override void OnInitialize()
    {
        rlImGui.Setup(DarkMode);
        Camera = CameraLayer.UI;
        OnGuiInitialize();
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
        var scaleX = Scene.GameHost.Width / RenderTarget!.RenderWidth;
        var scaleY = Scene.GameHost.Height / RenderTarget!.RenderHeight;
        return new Vector2(
            mouse.X / scaleX,
            mouse.Y / scaleY
        );
    }

    private Vector2 GetScreenSize()
    {
        return new Vector2(RenderTarget!.RenderWidth, RenderTarget!.RenderHeight);
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