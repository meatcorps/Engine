using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.GameObjects;
using Meatcorps.Game.Pacman.Data;
using Meatcorps.Game.Pacman.GameEnums;
using Meatcorps.Game.Pacman.GameObjects.Abstractions;
using Raylib_cs;

namespace Meatcorps.Game.Pacman.GameObjects;

public class Background : ResourceGameObject
{
    private PersistentCanvas _canvas = null!;
    private TimerOn _timer = new(2000);
    private Dictionary<PointInt, float> _multiplier = new();
    private Dictionary<PointInt, bool> _playSound = new();
    private FixedTimer _smokeBackground = new(10000);
    private FixedTimer _smokeBackground2 = new(20000);
    private EdgeDetector _animationDone = new();
    private List<Vector2> _cars = new List<Vector2>();

    protected override void OnInitialize()
    {
        base.OnInitialize();
        _canvas = Scene.GetGameObject<PersistentCanvas>()!;

        for (var i = 0; i < 10; i++)
        {
            _cars.Add(new Vector2(Raylib.GetRandomValue(260, 420), Raylib.GetRandomValue(0, 720)));
        }
        
        for (var x = 0; x < LevelData.LevelWidth; x++)
        {
            for (var y = 0; y < LevelData.LevelHeight; y++)
            {
                _multiplier.Add(new PointInt(x, y), 1 + ((float)Raylib.GetRandomValue(0, 100) / 100f));
            }
        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        _timer.Update(true, deltaTime);
        _smokeBackground.Update(deltaTime);
        _smokeBackground2.Update(deltaTime);
        _animationDone.Update(_timer.Output);

        if (_animationDone.IsRisingEdge)
            Sounds.Play(GameSounds.Bang, 0.3f);

        for (var i = 0; i < _cars.Count; i++)
        {
            _cars[i] += new Vector2(0, 100 + (i * 10)) * deltaTime;

            if (_cars[i].Y > 720)
            {
                _cars[i] = new Vector2(Raylib.GetRandomValue(260, 420), Raylib.GetRandomValue(-64, -164));
            }
        }
    }

    protected override void OnDraw()
    {
        var backgroundOffset = new Vector2(0, -38);
        Sprites.Draw(GameSprites.CityBackground, backgroundOffset, Color.White);
        var counter = 0f;
        foreach (var car in _cars)
        {
            counter += 360f / _cars.Count;
            var position = car;
            Sprites.Draw(GameSprites.CarGlow, position + new Vector2(-8, 0), Color.White);
            Sprites.Draw(GameSprites.Car, position, Raylib.ColorFromHSV(counter, 0.5f, 1f));
            Sprites.Draw(GameSprites.CarLights, position, Color.White);
        }
        
        var offset = new Vector2(-640, 0) * _smokeBackground.NormalizedElapsed;
        Sprites.Draw(GameSprites.SmokeLarge, new Vector2(0, 0) + offset + backgroundOffset, Color.DarkGreen);
        Sprites.Draw(GameSprites.SmokeLarge, new Vector2(640, 0) + offset + backgroundOffset, Color.DarkGreen);
        Sprites.Draw(GameSprites.SmokeLarge, new Vector2(0, 360) + offset + backgroundOffset, Color.DarkGreen);
        Sprites.Draw(GameSprites.SmokeLarge, new Vector2(640, 360) + offset + backgroundOffset, Color.DarkGreen);
        offset = new Vector2(-640, 0) * _smokeBackground2.NormalizedElapsed;
        Sprites.Draw(GameSprites.SmokeLarge, new Vector2(0, 0) + offset + backgroundOffset, Color.Green, flippedY: true);
        Sprites.Draw(GameSprites.SmokeLarge, new Vector2(640, 0) + offset + backgroundOffset, Color.Green, flippedY: true);
        Sprites.Draw(GameSprites.SmokeLarge, new Vector2(0, 360) + offset + backgroundOffset, Color.Green, flippedY: true);
        Sprites.Draw(GameSprites.SmokeLarge, new Vector2(640, 360) + offset + backgroundOffset, Color.Green, flippedY: true);

        
        Raylib.DrawTexturePro(
            _canvas.Texture,
            new Rectangle(0, 0, _canvas.Texture.Width, -_canvas.Texture.Height), // <-- negative height = Y-flip
            new Rectangle(0, 0, _canvas.Texture.Width, _canvas.Texture.Height),
            Vector2.Zero,
            0f,
            Color.White
        );
    }

    protected override void OnDispose()
    {
    }
}