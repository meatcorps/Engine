using System.Numerics;
using Meatcorps.Engine.Collision.Data;
using Meatcorps.Engine.Collision.Enums;
using Meatcorps.Engine.Collision.Interfaces;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.TileRenderer;
using Meatcorps.Engine.RayLib.UI.Data;
using Meatcorps.Game.Pacman.GameEnums;
using Meatcorps.Game.Pacman.GameObjects.Abstractions;
using Raylib_cs;

namespace Meatcorps.Game.Pacman.GameObjects;

public class Map: ResourceGameObject, ICollisionEventsFiltered
{
    private TileRuleSettings<GameTileGroup> _tileRuleSettings;
    private TileRuleSet<GameSprites, GameTileGroup> _tileRuleSet;
    private bool _oneTheBridge;
    private SmoothValue _bridgeOffset = new SmoothValue(0, 0.2f);
    private Dictionary<PointInt, bool> _placed = new();
    private int _totalDronesCalled;
    private int _totalDronesCalledDone;
    private bool _started;
    private SmoothValue _borderColorTimer = new SmoothValue(0, 4);
    private FixedTimer _lineBorderBlinkTimer = new(333);
    
    protected override void OnInitialize()
    {
        base.OnInitialize();
        _tileRuleSet = GlobalObjectManager.ObjectManager.Get<TileRuleSet<GameSprites, GameTileGroup>>()!;
        
        Layer = 2;
        _tileRuleSet.ClearCache();
        _tileRuleSettings = new();
        _tileRuleSettings
            .WithIsAllowed((position, group) =>
            {
                if (!LevelData.Map.Entities.TryGetValue(position, out var mapItem))
                    return false;
                return mapItem.Walkable && !mapItem.GhostHome;
            })
            .Use4Neighbors()
            .WithBounds(new Rect(0, 0, LevelData.LevelWidth, LevelData.LevelHeight), true);
        WorldService.AddCollisionEvents(this);
    }

    private void ReleaseTheDrones()
    {
        var delay = 0;
        _totalDronesCalled = 0;
        foreach (var mapItem in LevelData.Map.Entities.Values)
        {
            var droneCalled = false;
            var position = LevelData.ToWorldPosition(mapItem.Position);
            if (mapItem.Walkable || mapItem.GhostHome)
            {
                var result = _tileRuleSet.GetTile(_tileRuleSettings, mapItem.Position, out var tile);

                    Scene.AddGameObject(new Drone(position, result ? tile : GameSprites.WalkableTopBottomLeftRight, () =>
                    {
                        _placed[mapItem.Position] = true;
                        _totalDronesCalledDone++;
                    }, delay));
                    droneCalled = true;
                
            }
            
            if (mapItem.GhostHome && !mapItem.OneWay)
            {
                Scene.AddGameObject(new Drone(position, GameSprites.GhostCharger, () =>
                {
                    _placed[mapItem.Position] = true;
                    _totalDronesCalledDone++;
                }, delay));
                droneCalled = true;
            }
            
            if (mapItem.OneWay)
            {
                Scene.AddGameObject(new Drone(position, GameSprites.WalkableBridge, () =>
                {
                    _placed[mapItem.Position] = true;
                    _totalDronesCalledDone++;
                    
                }, delay));
                droneCalled = true;
            }

            if (droneCalled)
            {
                _placed[mapItem.Position] = false;
                delay += 10;
                _totalDronesCalled++;
            }
        }
    }

    public void LevelLoaded()
    {
        _tileRuleSet.UpdateCache(_tileRuleSettings);
        ReleaseTheDrones();

    }
    
    protected override void OnUpdate(float deltaTime)
    {
        _lineBorderBlinkTimer.Update(deltaTime);
        _borderColorTimer.Update(deltaTime);
        _borderColorTimer.RealValue = LevelData.GhostScared ? 1f : 0f;
        _bridgeOffset.Update(deltaTime);
        var canStart = _totalDronesCalled > 0;
        foreach (var mapItem in _placed)
        {
            if (!mapItem.Value)
            {
                canStart = false;
                break;
            }
                
        }
        if (canStart && !_started)
        {
            _started = true;
            
            Scene.AddGameObject(new Drone(LevelData.Ghosts[0].Body.Position, GameSprites.GhostBlinky1, () =>
            {
                LevelData.Ghosts[0].Enabled = true;
            }, 2500));
            Scene.AddGameObject(new Drone(LevelData.Ghosts[1].Body.Position, GameSprites.GhostPinky1, () =>
            {
                LevelData.Ghosts[1].Enabled = true;
            }, 2510));
            Scene.AddGameObject(new Drone(LevelData.Ghosts[2].Body.Position, GameSprites.GhostInky1, () =>
            {
                LevelData.Ghosts[2].Enabled = true;
            }, 2520));
            Scene.AddGameObject(new Drone(LevelData.Ghosts[3].Body.Position, GameSprites.GhostClyde1, () =>
            {
                LevelData.Ghosts[3].Enabled = true;
            }, 2530));
            var counter = 0;
            foreach (var pacMan in Scene.GetGameObjects<PacMan>())
            {
                Scene.AddGameObject(new Drone(pacMan.Position, GameSprites.PacmanDown1, () =>
                {
                    counter++;
                    pacMan.Enabled = true;
                    var preset = UIMessagePresets.Default(Fonts.GetFont());
                    preset.AnchorFrom = Anchor.CenterLeft;
                    preset.AnchorTo = Anchor.Bottom;
                    preset.AnchorAfter = Anchor.CenterRight;
                    Music.Resume();
                    Scene.AddGameObject(new ScreenFlash(Color.White));
                    if (counter == 1)
                    {
                        MessageUI.Show("GO! GET ALL THE MEAT!", preset);
                    }
                }, 1000));
            }
        }
    }

    protected override void OnLateUpdate(float deltaTime)
    {
        _bridgeOffset.RealValue = 16f;
        base.OnLateUpdate(deltaTime);
    }

    protected override void OnDraw()
    {
        foreach (var mapItem in LevelData.Map.Entities.Values)
        {
            if (!_placed.TryGetValue(mapItem.Position, out var value))
                continue;
            
            if (!value)
                continue;
            
            var position = LevelData.ToWorldPosition(mapItem.Position);
            if (mapItem.Walkable || mapItem.GhostHome)
            {
                var result = _tileRuleSet.GetTile(_tileRuleSettings, mapItem.Position, out var tile);
                
                Sprites.Draw(result ? tile : GameSprites.WalkableTopBottomLeftRight, position);
            }
            
            if (mapItem.GhostHome && !mapItem.OneWay)
            {
                Sprites.Draw(GameSprites.WalkableTopBottomLeftRight, position);
                Sprites.Draw(GameSprites.GhostCharger, position, Color.DarkGray);
            }
            
            if (mapItem.OneWay)
            {
                Sprites.Draw(GameSprites.WalkableBridge, position + new Vector2(0, _bridgeOffset.DisplayValue), Color.Gray);
            }
        }

        if (!_started)
            return;
        
        foreach (var mapItem in LevelData.Map.Entities.Values)
        {
            
            if (!mapItem.Walkable)
            {
                var rect = LevelData.ToWorldRectangle(mapItem.Position);

                foreach (var direction in mapItem.Directions)
                {
                    var startPos1 = rect.Center;
                    var startPos2 = rect.Center;
                    if (direction.X == 0)
                    {
                        startPos1.X = rect.Left;
                        startPos1.Y += direction.Y * (LevelData.GridSize / 2) - 1;
                        startPos2.X = rect.Right;
                        startPos2.Y += direction.Y * (LevelData.GridSize / 2) - 1;
                        Raylib.DrawLineV(startPos1.ToVector2(), startPos2.ToVector2(), 
                            Raylib.ColorLerp(Raylib.ColorAlpha(Color.Purple, 0.5f), 
                                _lineBorderBlinkTimer.NormalizedElapsed < 0.5f ? Color.Red : new Color(0,0,0,0), _borderColorTimer.DisplayValue));
                    }
                }
            }
        }

        foreach (var mapItem in LevelData.Map.Entities.Values)
        {
            
            if (!mapItem.Walkable)
            {
                var rect = LevelData.ToWorldRectangle(mapItem.Position);
               
                foreach (var direction in mapItem.Directions)
                {
                    var startPos1 = rect.Center;
                    var startPos2 = rect.Center;
                    if (direction.Y == 0)
                    {
                        startPos1.X += direction.X * (LevelData.GridSize / 2) - 1;
                        startPos1.Y = rect.Top;
                        startPos2.X += direction.X * (LevelData.GridSize / 2) - 1;
                        startPos2.Y = rect.Bottom;
                        Raylib.DrawLineV(startPos1.ToVector2(), startPos2.ToVector2(), 
                            Raylib.ColorLerp(Raylib.ColorAlpha(Color.Purple, 0.5f), 
                                _lineBorderBlinkTimer.NormalizedElapsed < 0.5f ? Color.Red : new Color(0,0,0,0), _borderColorTimer.DisplayValue));
                    }
                }
            }
        }
    }

    protected override void OnDispose()
    {
    }

    public void OnContact(ContactPhase phase, in ContactPair pair, in ContactManifold manifold)
    {
    }

    public void OnTrigger(ContactPhase phase, in ContactPair pair)
    {
        _bridgeOffset.RealValue = 0f;
    }
}