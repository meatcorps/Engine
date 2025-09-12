using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Engine.Arcade.Leaderboard.GameEnums;
using Raylib_cs;

namespace Meatcorps.Engine.Arcade.Leaderboard.Resources;

public static class GameSpriteFactory
{
    public static Texture2DItem<GameSprites> Load()
    {
        return new Texture2DItem<GameSprites>("Assets/GameSprites.png")
            .WithFilter(TextureFilter.Point)
            .WithGridSize(new System.Drawing.Point(16, 16))
            .WithSpriteFromGrid(GameSprites.Explosion1, new Rect(0, 4, 2, 2))
            .WithSpriteFromGrid(GameSprites.Explosion2, new Rect(2, 4, 2, 2))
            .WithSpriteFromGrid(GameSprites.Explosion3, new Rect(4, 4, 2, 2))
            .WithSpriteFromGrid(GameSprites.Explosion4, new Rect(6, 4, 2, 2))
            .WithSpriteFromGrid(GameSprites.Explosion5, new Rect(8, 4, 2, 2))
            .WithSpriteFromGrid(GameSprites.Explosion6, new Rect(10, 4, 2, 2))
            .WithSpriteFromGrid(GameSprites.ArcadeButtonUp, new Rect(0, 7, 2, 2))
            .WithSpriteFromGrid(GameSprites.ArcadeButtonDown, new Rect(2, 7, 2, 2))
            .WithSpriteFromGrid(GameSprites.ArcadeButtonUpPlayerOne, new Rect(4, 7, 2, 2))
            .WithSpriteFromGrid(GameSprites.ArcadeButtonDownPlayerOne, new Rect(6, 7, 2, 2))
            .WithSpriteFromGrid(GameSprites.ArcadeButtonUpPlayerTwo, new Rect(8, 7, 2, 2))
            .WithSpriteFromGrid(GameSprites.ArcadeButtonDownPlayerTwo, new Rect(10, 7, 2, 2))
            .WithSpriteFromGrid(GameSprites.MiniArcadeButtonUp, new PointInt(0, 9))
            .WithSpriteFromGrid(GameSprites.MiniArcadeButtonDown, new PointInt(1, 9))
            .WithSpriteFromGrid(GameSprites.MiniArcadeButtonUpPlayerOne, new PointInt(2, 9))
            .WithSpriteFromGrid(GameSprites.MiniArcadeButtonDownPlayerOne, new PointInt(3, 9))
            .WithSpriteFromGrid(GameSprites.MiniArcadeButtonUpPlayerTwo, new PointInt(4, 9))
            .WithSpriteFromGrid(GameSprites.MiniArcadeButtonDownPlayerTwo, new PointInt(5, 9))
            .WithSpriteFromGrid(GameSprites.ArcadeStickCenter, new Rect(0, 10, 2, 2))
            .WithSpriteFromGrid(GameSprites.ArcadeStickUp, new Rect(2, 10, 2, 2))
            .WithSpriteFromGrid(GameSprites.ArcadeStickLeft, new Rect(4, 10, 2, 2))
            .WithSpriteFromGrid(GameSprites.ArcadeStickRight, new Rect(6, 10, 2, 2))
            .WithSpriteFromGrid(GameSprites.ArcadeStickDown, new Rect(8, 10, 2, 2))
            .WithSpriteFromGrid(GameSprites.ArcadeStickArrowUp, new Rect(10, 10, 2, 2))
            .WithSpriteFromGrid(GameSprites.ArcadeStickArrowLeft, new Rect(12, 10, 2, 2))
            .WithSpriteFromGrid(GameSprites.ArcadeStickArrowDown, new Rect(14, 10, 2, 2))
            .WithSpriteFromGrid(GameSprites.ArcadeStickArrowRight, new Rect(16, 10, 2, 2))
            .WithSpriteFromGrid(GameSprites.ArcadeStickArrowUpLeft, new Rect(18, 10, 2, 2))
            .WithSpriteFromGrid(GameSprites.ArcadeStickArrowDownLeft, new Rect(20, 10, 2, 2))
            .WithSpriteFromGrid(GameSprites.ArcadeStickArrowDownRight, new Rect(22, 10, 2, 2))
            .WithSpriteFromGrid(GameSprites.ArcadeStickArrowUpRight, new Rect(24, 10, 2, 2))

            // Animations
            .WithSpriteAnimation(GameSprites.ExplosionAnimation, new[]
            {
                GameSprites.Explosion1,
                GameSprites.Explosion2,
                GameSprites.Explosion3,
                GameSprites.Explosion4,
                GameSprites.Explosion5,
                GameSprites.Explosion6,
            })
            .WithSpriteAnimation(GameSprites.ArcadeButtonAnimation, new[]
            {
                GameSprites.ArcadeButtonUp,
                GameSprites.ArcadeButtonDown
            })
            .WithSpriteAnimation(GameSprites.ArcadeButtonPlayerOneAnimation, new[]
            {
                GameSprites.ArcadeButtonUpPlayerOne,
                GameSprites.ArcadeButtonDownPlayerOne
            })
            .WithSpriteAnimation(GameSprites.ArcadeButtonPlayerTwoAnimation, new[]
            {
                GameSprites.ArcadeButtonUpPlayerTwo,
                GameSprites.ArcadeButtonDownPlayerTwo
            })
            .WithSpriteAnimation(GameSprites.MiniArcadeButtonAnimation, new[]
            {
                GameSprites.MiniArcadeButtonUp,
                GameSprites.MiniArcadeButtonDown
            })
            .WithSpriteAnimation(GameSprites.MiniArcadeButtonPlayerOneAnimation, new[]
            {
                GameSprites.MiniArcadeButtonUpPlayerOne,
                GameSprites.MiniArcadeButtonDownPlayerOne
            })
            .WithSpriteAnimation(GameSprites.MiniArcadeButtonPlayerTwoAnimation, new[]
            {
                GameSprites.MiniArcadeButtonUpPlayerTwo,
                GameSprites.MiniArcadeButtonDownPlayerTwo
            })
            .WithSpriteAnimation(GameSprites.ArcadeStickAnimationLeftRight, new[]
            {
                GameSprites.ArcadeStickLeft,
                GameSprites.ArcadeStickCenter,
                GameSprites.ArcadeStickRight,
                GameSprites.ArcadeStickCenter
            })
            .WithSpriteAnimation(GameSprites.ArcadeStickAnimationTopDown, new[]
            {
                GameSprites.ArcadeStickUp,
                GameSprites.ArcadeStickCenter,
                GameSprites.ArcadeStickDown,
                GameSprites.ArcadeStickCenter
            })
            .WithSpriteAnimation(GameSprites.ArcadeStickAnimationRotate, new[]
            {
                GameSprites.ArcadeStickLeft,
                GameSprites.ArcadeStickUp,
                GameSprites.ArcadeStickRight,
                GameSprites.ArcadeStickDown,
            });
    }
}