// dotnet add package Raylib-cs

using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using Raylib_cs;

class Program
{
    static void Main(string[] args)
    {
        var imagePath = args.Length > 0 ? args[0] : "Assets/GameSprites.png";
        var gridSize = ParseIntArg(args, "grid", 16);
        var scale = ParseIntArg(args, "scale", 2);
        var enumName = ParseStrArg(args, "enum", "GameSprites");

        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(1280, 800, "Sprite Atlas Picker");
        Raylib.SetTargetFPS(120);

        if (!File.Exists(imagePath))
        {
            Console.WriteLine($"File not found: {imagePath}");
            return;
        }

        var tex = Raylib.LoadTexture(imagePath);
        Raylib.SetTextureFilter(tex, TextureFilter.Point);
        var canvas = Raylib.LoadRenderTexture(tex.Width, tex.Height);

        // ----- State -----
        bool showGrid = true;
        bool draggingMouse = false;

        Vector2 startCell = Vector2.Zero; // rect anchor (grid coords)
        Vector2 endCell = Vector2.Zero; // rect moving edge (grid coords)
        Vector2 hovered = Vector2.Zero; // current hover (grid coords)

        string typedName = "";
        string lastOutput = "";

        // Panning (in screen space)
        int panX = 10, panY = 10; // top-left draw position (pixels)
        bool middleDragging = false;
        Vector2 middleDragStartMouse = Vector2.Zero;
        (int x, int y) middleDragStartPan = (0, 0);

        void Emit(string line)
        {
            lastOutput = line;
            Console.WriteLine(line);
            Raylib.SetClipboardText(line);
        }

        while (!Raylib.WindowShouldClose())
        {
            // UI toggles
            if (Raylib.IsKeyPressed(KeyboardKey.Equal)) scale = Math.Clamp(scale + 1, 1, 32);
            if (Raylib.IsKeyPressed(KeyboardKey.Minus)) scale = Math.Clamp(scale - 1, 1, 32);
            if (Raylib.IsKeyPressed(KeyboardKey.G)) showGrid = !showGrid;
            if (Raylib.IsKeyPressed(KeyboardKey.C) && !string.IsNullOrEmpty(lastOutput))
                Raylib.SetClipboardText(lastOutput);
            if (Raylib.IsKeyPressed(KeyboardKey.Home))
            {
                panX = 10;
                panY = 10;
            } // reset pan

            HandleTyping(ref typedName);

            // Where image is drawn (top-left, integer scale) — includes pan
            var drawRect = new Rectangle(panX, panY, tex.Width * scale, tex.Height * scale);
            var mouse = Raylib.GetMousePosition();
            bool mouseOnImage = Raylib.CheckCollisionPointRec(mouse, drawRect);

            // Middle-mouse drag pan
            if (Raylib.IsMouseButtonPressed(MouseButton.Middle))
            {
                middleDragging = true;
                middleDragStartMouse = mouse;
                middleDragStartPan = (panX, panY);
            }

            if (middleDragging)
            {
                var delta = mouse - middleDragStartMouse;
                panX = middleDragStartPan.x + (int)delta.X;
                panY = middleDragStartPan.y + (int)delta.Y;
                if (Raylib.IsMouseButtonReleased(MouseButton.Middle))
                    middleDragging = false;
            }

            // Arrow keys pan (grid-step or fine-step with Ctrl)
            int gridStep = gridSize * scale;
            int fineStep = 1 * scale;
            bool ctrl = Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl);
            int step = ctrl ? fineStep : gridStep;

            if (Raylib.IsKeyPressed(KeyboardKey.Right)) panX -= step; // move image left relative to window
            if (Raylib.IsKeyPressed(KeyboardKey.Left)) panX += step; // move image right
            if (Raylib.IsKeyPressed(KeyboardKey.Down)) panY -= step; // move image up
            if (Raylib.IsKeyPressed(KeyboardKey.Up)) panY += step; // move image down

            // Mouse -> grid cell conversion (texture space)
            if (mouseOnImage)
            {
                var localX = (mouse.X - drawRect.X) / drawRect.Width * tex.Width;
                var localY = (mouse.Y - drawRect.Y) / drawRect.Height * tex.Height;
                hovered = new Vector2(
                    MathF.Floor(localX / gridSize),
                    MathF.Floor(localY / gridSize)
                );
                hovered.X = Math.Clamp(hovered.X, 0, tex.Width / gridSize - 1);
                hovered.Y = Math.Clamp(hovered.Y, 0, tex.Height / gridSize - 1);
            }

            // Mouse drag for rect select
            if (mouseOnImage && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                draggingMouse = true;
                startCell = hovered;
                endCell = hovered;
            }

            if (draggingMouse)
            {
                endCell = hovered;
                if (Raylib.IsMouseButtonReleased(MouseButton.Left))
                {
                    draggingMouse = false;
                    var rect = CellsToRect(startCell, endCell);
                    EmitRectOrPoint(enumName, typedName, rect);
                }
            }

            // Emit single cell (RMB or Enter)
            if ((mouseOnImage && Raylib.IsMouseButtonPressed(MouseButton.Right)) ||
                Raylib.IsKeyPressed(KeyboardKey.Enter))
            {
                if (draggingMouse)
                {
                    var rect = CellsToRect(startCell, endCell);
                    EmitRectOrPoint(enumName, typedName, rect);
                }
                else
                {
                    EmitPoint(enumName, typedName, (int)hovered.X, (int)hovered.Y);
                }
            }

            // ---- Render to canvas (unscaled) ----
            Raylib.BeginTextureMode(canvas);
            Raylib.ClearBackground(Color.Blank);
            Raylib.DrawTexture(tex, 0, 0, Color.White);

            if (showGrid)
                DrawGrid(tex.Width, tex.Height, gridSize, new Color(255, 255, 255, 60));

            // Highlight hovered cell
            var hoverPx = new System.Drawing.Rectangle(
                (int)hovered.X * gridSize, (int)hovered.Y * gridSize,
                gridSize, gridSize
            );
            DrawRectOutline(hoverPx, 2, Color.Yellow);

            // Selection rect if dragging
            if (draggingMouse)
            {
                var sel = CellsToRect(startCell, endCell);
                var pxRect = new System.Drawing.Rectangle(sel.X * gridSize, sel.Y * gridSize, sel.W * gridSize,
                    sel.H * gridSize);
                DrawRectOutline(pxRect, 2, Color.Orange);
            }

            Raylib.EndTextureMode();


            // ---- Present (scaled, with pan) ----
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(16, 16, 16, 255));

            Raylib.DrawTexturePro(
                canvas.Texture,
                new Rectangle(0, 0, canvas.Texture.Width, -canvas.Texture.Height),
                new Rectangle(panX, panY, tex.Width * scale, tex.Height * scale),
                Vector2.Zero,
                0f,
                Color.White
            );

            Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), 32, Color.Black);
            DrawHudLabel(2, 2, $"Image: {Path.GetFileName(imagePath)} | Grid: {gridSize}   Scale: {scale}x | Cell: {(int)hovered.X}, {(int)hovered.Y} | Name: {typedName}");
            if (!string.IsNullOrEmpty(lastOutput))
                DrawHudLabel(2, 16, $"Last: {lastOutput}");

            var hint =
                "[Arrows]=Pan  [Ctrl+Arrows]=Fine Pan  [MMB drag]=Pan  [LMB drag]=Rect  [RMB]=Point  [Enter]=Emit  [+/-]=Scale  [G]=Grid  [Home]=Reset Pan";
            Raylib.DrawText(hint, 2, Raylib.GetScreenHeight() - 24, 18, new Color(200, 200, 200, 255));

            Raylib.EndDrawing();
        }

        // Cleanup
        Raylib.UnloadRenderTexture(canvas);
        Raylib.UnloadTexture(tex);
        Raylib.CloseWindow();

        // ---------- helpers ----------
        static (int X, int Y, int W, int H) CellsToRect(Vector2 a, Vector2 b)
        {
            var x0 = (int)MathF.Min(a.X, b.X);
            var y0 = (int)MathF.Min(a.Y, b.Y);
            var x1 = (int)MathF.Max(a.X, b.X);
            var y1 = (int)MathF.Max(a.Y, b.Y);
            return (x0, y0, x1 - x0 + 1, y1 - y0 + 1);
        }

        static void DrawGrid(int texW, int texH, int cell, Color c)
        {
            for (int x = 0; x <= texW; x += cell) Raylib.DrawLine(x, 0, x, texH, c);
            for (int y = 0; y <= texH; y += cell) Raylib.DrawLine(0, y, texW, y, c);
        }

        static void DrawRectOutline(System.Drawing.Rectangle r, int thickness, Color col) =>
            Raylib.DrawRectangleLinesEx(new Rectangle(r.X, r.Y, r.Width, r.Height), thickness, col);

        static void DrawLabel(int x, int y, string text)
        {
            Raylib.DrawText(text, x + 1, y + 1, 12, Color.Black);
            Raylib.DrawText(text, x, y, 12, Color.White);
        }

        static void DrawHudLabel(int x, int y, string text)
        {
            Raylib.DrawTextPro(Raylib.GetFontDefault(), text, new Vector2(x + 1, y + 1), Vector2.Zero, 0, 16, 2, Color.Black);
            Raylib.DrawTextPro(Raylib.GetFontDefault(), text, new Vector2(x, y), Vector2.Zero, 0, 16, 2, Color.White);
        }

        // Emit either a PointInt or a Rect depending on width/height
        void EmitRectOrPoint(string enumType, string name, (int X, int Y, int W, int H) rect)
        {
            if (rect.W == 1 && rect.H == 1)
                EmitPoint(enumType, name, rect.X, rect.Y);
            else
                EmitRect(enumType, name, rect.X, rect.Y, rect.W, rect.H);
        }

// Emit a single cell (PointInt)
        void EmitPoint(string enumType, string name, int gx, int gy)
        {
            var label = string.IsNullOrWhiteSpace(name) ? "XXX" : name;
            Emit($".WithSpriteFromGrid({enumType}.{label}, new PointInt({gx}, {gy}))");
        }

// Emit a rectangle of cells (Rect)
        void EmitRect(string enumType, string name, int gx, int gy, int gw, int gh)
        {
            var label = string.IsNullOrWhiteSpace(name) ? "XXX" : name;
            Emit($".WithSpriteFromGrid({enumType}.{label}, new Rect({gx}, {gy}, {gw}, {gh}))");
        }
    }

    static int ParseIntArg(string[] args, string key, int def)
    {
        foreach (var a in args)
        {
            var kv = a.Split('=', 2, StringSplitOptions.TrimEntries);
            if (kv.Length == 2 && kv[0].Equals(key, StringComparison.OrdinalIgnoreCase)
                               && int.TryParse(kv[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                return v;
        }

        return def;
    }

    static string ParseStrArg(string[] args, string key, string def)
    {
        foreach (var a in args)
        {
            var kv = a.Split('=', 2, StringSplitOptions.TrimEntries);
            if (kv.Length == 2 && kv[0].Equals(key, StringComparison.OrdinalIgnoreCase))
                return kv[1];
        }

        return def;
    }

    static void HandleTyping(ref string buffer)
    {
        int key = Raylib.GetCharPressed();
        while (key > 0)
        {
            if (char.IsLetterOrDigit((char)key) || (char)key == '_')
                buffer += (char)key;
            key = Raylib.GetCharPressed();
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && buffer.Length > 0)
            buffer = buffer[..^1];
    }
}