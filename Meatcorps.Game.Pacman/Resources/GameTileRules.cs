using Meatcorps.Engine.RayLib.TileRenderer;
using Meatcorps.Game.Pacman.GameEnums;

namespace Meatcorps.Game.Pacman.Resources;

public class GameTileRules
{
    public static TileRuleSet<GameSprites, GameTileGroup> Create()
    {
        return new TileRuleSet<GameSprites, GameTileGroup>()
                .For(GameSprites.WalkableTopBottomLeftRight)
                .Require(GameTileGroup.IsWalkable, TileRuleCheck.Create(true, true, true, true)).End()
                .For(GameSprites.WalkableBottomLeftRight)
                .Require(GameTileGroup.IsWalkable, TileRuleCheck.Create(true, true, false, true)).End()
                .For(GameSprites.WalkableTopLeftRight)
                .Require(GameTileGroup.IsWalkable, TileRuleCheck.Create(true, true, true, false)).End()
                .For(GameSprites.WalkableTopBottomRight)
                .Require(GameTileGroup.IsWalkable, TileRuleCheck.Create(false, true, true, true)).End()
                .For(GameSprites.WalkableTopBottomLeft)
                .Require(GameTileGroup.IsWalkable, TileRuleCheck.Create(true, false, true, true)).End()
                .For(GameSprites.WalkableTopRight)
                .Require(GameTileGroup.IsWalkable, TileRuleCheck.Create(false, true, true, false)).End()
                .For(GameSprites.WalkableTopLeft)
                .Require(GameTileGroup.IsWalkable, TileRuleCheck.Create(true, false, true, false)).End()
                .For(GameSprites.WalkableBottomRight)
                .Require(GameTileGroup.IsWalkable, TileRuleCheck.Create(false, true, false, true)).End()
                .For(GameSprites.WalkableBottomLeft)
                .Require(GameTileGroup.IsWalkable, TileRuleCheck.Create(true, false, false, true)).End()
                .For(GameSprites.WalkableLeftRight)
                .Require(GameTileGroup.IsWalkable, TileRuleCheck.Create(true, true, false, false)).End()
                .For(GameSprites.WalkableTopBottom)
                .Require(GameTileGroup.IsWalkable, TileRuleCheck.Create(false, false, true, true)).End()
                .For(GameSprites.WalkableTopBottomLeftRight)
                .Require(GameTileGroup.IsWalkable, TileRuleCheck.Create(true, false, false, false)).End()
                .For(GameSprites.WalkableTopBottomLeftRight)
                .Require(GameTileGroup.IsWalkable, TileRuleCheck.Create(false, true, false, false)).End()
                .For(GameSprites.WalkableTopBottomLeftRight)
                .Require(GameTileGroup.IsWalkable, TileRuleCheck.Create(false, false, true, false)).End()
                .For(GameSprites.WalkableTopBottomLeftRight)
                .Require(GameTileGroup.IsWalkable, TileRuleCheck.Create(true, false, false, true)).End()


            ;
    }
}