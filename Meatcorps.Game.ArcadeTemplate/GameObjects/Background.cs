using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.GameObjects;
using Meatcorps.Game.ArcadeTemplate.Data;
using Meatcorps.Game.ArcadeTemplate.GameEnums;
using Meatcorps.Game.ArcadeTemplate.GameObjects.Abstractions;
using Raylib_cs;

namespace Meatcorps.Game.ArcadeTemplate.GameObjects;

public class Background : ResourceGameObject
{
    private PersistentCanvas _canvas = null!;
    private TimerOn _timer = new(2000);
    private Dictionary<PointInt, float> _multiplier = new();
    private Dictionary<PointInt, bool> _playSound = new();
    private EdgeDetector _animationDone = new();
    
    protected override void OnInitialize()
    {
        base.OnInitialize();
        _canvas = Scene.GetGameObject<PersistentCanvas>()!;

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
        _animationDone.Update(_timer.Output);
        
        if (_animationDone.IsRisingEdge)
            Sounds.Play(GameSounds.Bang, 0.3f);
        
        if (_timer.Output)
            return;
        
        var soundPlayed = false;
        foreach (var (key, value) in _playSound)
        {
            if (!value)
            {
                if (soundPlayed == false)
                {
                    Sounds.Stop(GameSounds.Backgroundplaced);
                    Sounds.Play(GameSounds.Backgroundplaced, 0.3f, pitch: (float)Raylib.GetRandomValue(10, 50) / 100);
                    soundPlayed = true;
                }

                _playSound[key] = true;
            }
        }
    }

    protected override void OnDraw()
    {
        var color = Color.White;
        var ended = false;
        for (var x = 0; x < LevelData.LevelWidth; x++)
        {
            for (var y = 0; y < LevelData.LevelHeight; y++)
            {
                ended = false;
                if (!_timer.Output)
                {
                    ended = MathF.Min(1, _timer.NormalizedElapsed * _multiplier[new PointInt(x, y)]).EqualsSafe(1);
                    color = ended
                        ? Color.White
                        : Color.Black;
                    
                    if (!_playSound.ContainsKey(new PointInt(x, y)) && ended)
                        _playSound.Add(new PointInt(x, y), false);
                    
                }
                //TODO Draw background?
                //Sprites.Draw(GameSprites.Background, new Vector2(x * LevelData.GridSize, y * LevelData.GridSize), color);
            }
        }
        
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