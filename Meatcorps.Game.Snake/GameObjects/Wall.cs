using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Game.Snake.Data;
using Meatcorps.Game.Snake.GameObjects.Abstractions;
using Meatcorps.Game.Snake.Resources;
using Raylib_cs;

namespace Meatcorps.Game.Snake.GameObjects;

public class Wall: SnakeGameObject
{
    private PointInt _position = new(-1, -1);
    private readonly bool _placeDirect;
    public SnakeSprites Sprite { get; private set; }
    private bool _movingToPosition = false;
    private bool _waitingToPosition = true;
    private Vector2 _flyFromPosition = Vector2.Zero;
    private Vector2 _flyToPosition = Vector2.Zero;
    private Vector2 _flyPosition = Vector2.Zero;
    private FixedTimer _flyTimer; 
    private FixedTimer _warningPulse; 
    private TimerOn _waitForPlacementTimer;
    private CameraControllerGameObject _cameraController;

    public Wall(float waitUntilWallIsPlaced, PointInt position, bool placeDirect = false, SnakeSprites sprite = SnakeSprites.Wall)
    {
        Sprite = sprite;
        _waitForPlacementTimer = new TimerOn(waitUntilWallIsPlaced);
        _flyTimer = new FixedTimer(Raylib.GetRandomValue(250, 750));
        _warningPulse = new FixedTimer(500);
        _position = position;
        _placeDirect = placeDirect;
    }
    
    protected override void OnInitialize()
    {
        base.OnInitialize();
        
        _cameraController = Scene.GetGameObject<CameraControllerGameObject>()!;
        
        _flyToPosition = LevelData.ToWorldPosition(_position);
        _flyFromPosition = new Vector2(Raylib.GetRandomValue(0, LevelData.LevelWidth * LevelData.GridSize), -20);

        if (_placeDirect)
        { 
            _waitingToPosition = false;
            _movingToPosition = false;
            LevelData.WallGrid.Register(_position, this);
        }
    }
    
    protected override void OnUpdate(float deltaTime)
    {
        if (_waitingToPosition)
        {
            Layer = 4;
            _warningPulse.Update(deltaTime);
            _waitForPlacementTimer.Update(true, deltaTime);

            if (_waitForPlacementTimer.Output && !LevelData.SnakeGrid.IsOccupied(_position))
            {
                _waitingToPosition = false;
                _movingToPosition = true;
            }
        } else if (_movingToPosition)
        {
            if (LevelData.SnakeGrid.IsOccupied(_position))
                return;
            
            Layer = 4;
            _flyTimer.Update(deltaTime);
    
            _flyPosition = Vector2.Lerp(_flyFromPosition, _flyToPosition, Tween.ApplyEasing(_flyTimer.NormalizedElapsed, EaseType.EaseInCubic));

            if (_flyTimer.Output)
            {
                Sounds.Play(SnakeSounds.Wallplaced);
                _movingToPosition = false;
                LevelData.WallGrid.Register(_position, this);
                _cameraController.Shake(2, 5);
            }
        }
        else
        {
            Layer = 2;
        }
        
    }

    protected override void OnDraw()
    {
        if (_waitingToPosition)
        {
            var normalToUpDown = Tween.NormalToUpDown(_warningPulse.NormalizedElapsed);
            Sprites.Draw(SnakeSprites.Warning, LevelData.ToWorldPosition(_position), Raylib.ColorAlpha(Color.Red, Tween.ApplyEasing(normalToUpDown, EaseType.EaseInOut)));
        } 
        else if (_movingToPosition)
        {
            Sprites.Draw(Sprite, _flyPosition, Color.White);
            
            Sprites.Draw(SnakeSprites.Warning, LevelData.ToWorldPosition(_position), Raylib.ColorAlpha(Color.Red, 1 - _flyTimer.NormalizedElapsed));
        }
        else
        {
            Sprites.Draw(Sprite, LevelData.ToWorldPosition(_position), Color.White);
        }
        base.OnDraw();
    }

    protected override void OnDispose()
    {
        if (_position != new PointInt(-1, -1))
            LevelData.WallGrid.Remove(_position);
    }
}