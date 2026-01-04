using System.Numerics;
using Meatcorps.Engine.RayLib.Abstractions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Debug;

public class VisualDebugger : BaseGameObject
{
    public static VisualDebugger? Instance;

    private readonly List<Action> _drawCommands = new();
    private readonly List<(string message, Color color)> _logs = new();
    private bool _showFps = true;

    private bool _showGizmos = true;
    private bool _showLog = true;

    public VisualDebugger()
    {
        Instance = this;
        Enabled = true;
        Visible = true;
    }

    // --------- Public API ---------

    public void Log(string message, Color color)
    {
        _logs.Add((message, color));
    }

    public void DrawLine(Vector2 start, Vector2 end, Color color)
    {
        _drawCommands.Add(() => Raylib.DrawLineV(start, end, color));
    }

    public void DrawCircle(Vector2 center, float radius, Color color)
    {
        _drawCommands.Add(() => Raylib.DrawCircleV(center, radius, color));
    }

    public void DrawRect(Vector2 position, Vector2 size, Color color)
    {
        _drawCommands.Add(() =>
            Raylib.DrawRectangleLines((int)position.X, (int)position.Y, (int)size.X, (int)size.Y, color));
    }

    public void DrawPoint(Vector2 position, Color color)
    {
        _drawCommands.Add(() => Raylib.DrawCircleV(position, 2, color));
    }

    public void DrawText(Vector2 position, string text, Color color, int fontSize = 10)
    {
        _drawCommands.Add(() => Raylib.DrawText(text, (int)position.X, (int)position.Y, fontSize, color));
    }

    // --------- Game Loop Hooks ---------

    protected override void OnInitialize()
    {
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.F1)) _showGizmos = !_showGizmos;
        if (Raylib.IsKeyPressed(KeyboardKey.F2)) _showLog = !_showLog;
        if (Raylib.IsKeyPressed(KeyboardKey.F3)) _showFps = !_showFps;
    }

    protected override void OnDraw()
    {
        if (_showGizmos)
            foreach (var command in _drawCommands)
                command();

        if (_showLog)
        {
            var y = 5;
            foreach (var (message, color) in _logs)
            {
                Raylib.DrawText(message, 5, y, 10, color);
                y += 14;
            }
        }

        if (_showFps) Raylib.DrawText($"FPS: {Raylib.GetFPS()}", 5, Raylib.GetScreenHeight() - 20, 10, Color.White);

        _drawCommands.Clear();
        _logs.Clear();
    }

    protected override void OnDispose()
    {
    }
}