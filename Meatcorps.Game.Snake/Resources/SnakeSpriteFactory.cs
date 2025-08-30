using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Resources;
using Raylib_cs;

namespace Meatcorps.Game.Snake.Resources;

public static class SnakeSpriteFactory
{
    public static Texture2DItem<SnakeSprites> Load()
    {
        return new Texture2DItem<SnakeSprites>("Assets/SnakeSprites.png")
            .WithFilter(TextureFilter.Point)
            .WithGridSize(new System.Drawing.Point(16, 16))

            // Row 0
            .WithSpriteFromGrid(SnakeSprites.SnakeBodyLeftRight, new PointInt(0, 0))
            .WithSpriteFromGrid(SnakeSprites.SnakeBodyTopBottom, new PointInt(0, 1))
            .WithSpriteFromGrid(SnakeSprites.SnakeHead1, new PointInt(3, 1))
            .WithSpriteFromGrid(SnakeSprites.SnakeHead2, new PointInt(4, 1))
            .WithSpriteFromGrid(SnakeSprites.SnakeHead3, new PointInt(5, 1))
            .WithSpriteFromGrid(SnakeSprites.SnakeHead4, new PointInt(6, 1))
            .WithSpriteFromGrid(SnakeSprites.SnakeHead5, new PointInt(7, 1))

            // Row 1
            .WithSpriteFromGrid(SnakeSprites.SnakeBodyCornerRightBottom, new PointInt(1, 0))
            .WithSpriteFromGrid(SnakeSprites.SnakeBodyCornerLeftBottom, new PointInt(2, 0))
            .WithSpriteFromGrid(SnakeSprites.SnakeBodyCornerRightTop, new PointInt(1, 1))
            .WithSpriteFromGrid(SnakeSprites.SnakeBodyCornerLeftTop, new PointInt(2, 1))
            .WithSpriteFromGrid(SnakeSprites.SnakeProcessing, new PointInt(3, 0))
            .WithSpriteFromGrid(SnakeSprites.SnakeTail, new PointInt(2, 2))
            .WithSpriteFromGrid(SnakeSprites.Meat1, new PointInt(4, 0))
            .WithSpriteFromGrid(SnakeSprites.Meat2, new PointInt(5, 0))
            .WithSpriteFromGrid(SnakeSprites.Background, new PointInt(0, 2))
            .WithSpriteFromGrid(SnakeSprites.Wall, new PointInt(1, 2))
            .WithSpriteFromGrid(SnakeSprites.Warning, new PointInt(3, 2))
            .WithSpriteFromGrid(SnakeSprites.Fly1, new PointInt(0, 3))
            .WithSpriteFromGrid(SnakeSprites.Fly2, new PointInt(1, 3))
            .WithSpriteFromGrid(SnakeSprites.Explosion1, new Rect(0, 4, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.Explosion2, new Rect(2, 4, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.Explosion3, new Rect(4, 4, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.Explosion4, new Rect(6, 4, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.Explosion5, new Rect(8, 4, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.Explosion6, new Rect(10, 4, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.Score2X, new PointInt(0, 6))
            .WithSpriteFromGrid(SnakeSprites.Score3X, new PointInt(1, 6))
            .WithSpriteFromGrid(SnakeSprites.Score4X, new PointInt(2, 6))
            .WithSpriteFromGrid(SnakeSprites.SnakeFaster, new PointInt(3, 6))
            .WithSpriteFromGrid(SnakeSprites.SnakeSlower, new PointInt(4, 6))
            .WithSpriteFromGrid(SnakeSprites.ThroughWalls, new PointInt(5, 6))
            .WithSpriteFromGrid(SnakeSprites.RotProof, new PointInt(6, 6))
            .WithSpriteFromGrid(SnakeSprites.WorldFaster, new PointInt(7, 6))
            .WithSpriteFromGrid(SnakeSprites.WorldSlower, new PointInt(8, 6))
            
            .WithSpriteFromGrid(SnakeSprites.ArcadeButtonUp, new Rect(0, 7, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.ArcadeButtonDown, new Rect(2, 7, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.ArcadeButtonUpPlayerOne, new Rect(4, 7, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.ArcadeButtonDownPlayerOne, new Rect(6, 7, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.ArcadeButtonUpPlayerTwo, new Rect(8, 7, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.ArcadeButtonDownPlayerTwo, new Rect(10, 7, 2, 2))
            
            .WithSpriteFromGrid(SnakeSprites.MiniArcadeButtonUp, new PointInt(0, 9))
            .WithSpriteFromGrid(SnakeSprites.MiniArcadeButtonDown, new PointInt(1, 9))
            .WithSpriteFromGrid(SnakeSprites.MiniArcadeButtonUpPlayerOne, new PointInt(2, 9))
            .WithSpriteFromGrid(SnakeSprites.MiniArcadeButtonDownPlayerOne, new PointInt(3, 9))
            .WithSpriteFromGrid(SnakeSprites.MiniArcadeButtonUpPlayerTwo, new PointInt(4, 9))
            .WithSpriteFromGrid(SnakeSprites.MiniArcadeButtonDownPlayerTwo, new PointInt(5, 9))
            
            .WithSpriteFromGrid(SnakeSprites.ArcadeStickCenter, new Rect(0, 10, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.ArcadeStickUp, new Rect(2, 10, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.ArcadeStickLeft, new Rect(4, 10, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.ArcadeStickRight, new Rect(6, 10, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.ArcadeStickDown, new Rect(8, 10, 2, 2))
            
            .WithSpriteFromGrid(SnakeSprites.ArcadeStickArrowUp, new Rect(10, 10, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.ArcadeStickArrowLeft, new Rect(12, 10, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.ArcadeStickArrowDown, new Rect(14, 10, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.ArcadeStickArrowRight, new Rect(16, 10, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.ArcadeStickArrowUpLeft, new Rect(18, 10, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.ArcadeStickArrowDownLeft, new Rect(20, 10, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.ArcadeStickArrowDownRight, new Rect(22, 10, 2, 2))
            .WithSpriteFromGrid(SnakeSprites.ArcadeStickArrowUpRight, new Rect(24, 10, 2, 2))


            // Animations
            .WithSpriteAnimation(SnakeSprites.SnakeHeadAnimation, new[]
            {
                SnakeSprites.SnakeHead1,
                SnakeSprites.SnakeHead2,
                SnakeSprites.SnakeHead3,
                SnakeSprites.SnakeHead4,
                SnakeSprites.SnakeHead5
            })
            .WithSpriteAnimation(SnakeSprites.FlyAnimation, new[]
            {
                SnakeSprites.Fly1,
                SnakeSprites.Fly2
            })
            .WithSpriteAnimation(SnakeSprites.ExplosionAnimation, new[]
            {
                SnakeSprites.Explosion1,
                SnakeSprites.Explosion2,
                SnakeSprites.Explosion3,
                SnakeSprites.Explosion4,
                SnakeSprites.Explosion5,
                SnakeSprites.Explosion6,
            })
            
            .WithSpriteAnimation(SnakeSprites.ArcadeButtonAnimation, new[]
            {
                SnakeSprites.ArcadeButtonUp,
                SnakeSprites.ArcadeButtonDown
            })
            .WithSpriteAnimation(SnakeSprites.ArcadeButtonPlayerOneAnimation, new[]
            {
                SnakeSprites.ArcadeButtonUpPlayerOne,
                SnakeSprites.ArcadeButtonDownPlayerOne
            })
            .WithSpriteAnimation(SnakeSprites.ArcadeButtonPlayerTwoAnimation, new[]
            {
                SnakeSprites.ArcadeButtonUpPlayerTwo,
                SnakeSprites.ArcadeButtonDownPlayerTwo
            })
            
            .WithSpriteAnimation(SnakeSprites.MiniArcadeButtonAnimation, new[]
            {
                SnakeSprites.MiniArcadeButtonUp,
                SnakeSprites.MiniArcadeButtonDown
            })
            .WithSpriteAnimation(SnakeSprites.MiniArcadeButtonPlayerOneAnimation, new[]
            {
                SnakeSprites.MiniArcadeButtonUpPlayerOne,
                SnakeSprites.MiniArcadeButtonDownPlayerOne
            })
            .WithSpriteAnimation(SnakeSprites.MiniArcadeButtonPlayerTwoAnimation, new[]
            {
                SnakeSprites.MiniArcadeButtonUpPlayerTwo,
                SnakeSprites.MiniArcadeButtonDownPlayerTwo
            })
            .WithSpriteAnimation(SnakeSprites.ArcadeStickAnimationLeftRight, new[]
            {
                SnakeSprites.ArcadeStickLeft,
                SnakeSprites.ArcadeStickCenter,
                SnakeSprites.ArcadeStickRight,
                SnakeSprites.ArcadeStickCenter
            })
            .WithSpriteAnimation(SnakeSprites.ArcadeStickAnimationTopDown, new[]
            {
                SnakeSprites.ArcadeStickUp,
                SnakeSprites.ArcadeStickCenter,
                SnakeSprites.ArcadeStickDown,
                SnakeSprites.ArcadeStickCenter
            })
            .WithSpriteAnimation(SnakeSprites.ArcadeStickAnimationRotate, new[]
            {
                SnakeSprites.ArcadeStickLeft,
                SnakeSprites.ArcadeStickUp,
                SnakeSprites.ArcadeStickRight,
                SnakeSprites.ArcadeStickDown,
            });
        
    }
}