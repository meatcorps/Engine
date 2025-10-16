using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Game.KillTheSkulls.GameEnums;
using Raylib_cs;

namespace Meatcorps.Game.KillTheSkulls.Resources;

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
            .WithSprite(GameSprites.Background2, new Rectangle(0, 208, 640, 240))
            .WithSprite(GameSprites.PipeOverlay, new Rectangle(0, 448, 640, 120))
            .WithSprite(GameSprites.Skull, new Rectangle(0, 576, 128, 128))
            .WithSprite(GameSprites.SkullCharged, new Rectangle(128, 576, 128, 128))
            .WithSprite(GameSprites.LedBar, new Rectangle(288, 608, 80, 32))
            .WithSprite(GameSprites.LedBarBackground, new Rectangle(288, 576, 80, 32))
            .WithSprite(GameSprites.Thunder1, new Rectangle(0, 704, 128, 80))
            .WithSprite(GameSprites.Thunder2, new Rectangle(128, 704, 128, 80))
            .WithSprite(GameSprites.Thunder3, new Rectangle(256, 704, 128, 80))
            .WithSprite(GameSprites.Thunder4, new Rectangle(384, 704, 128, 80))
            .WithSprite(GameSprites.Thunder5, new Rectangle(512, 704, 128, 80))
            .WithSprite(GameSprites.ThunderCharge1, new Rectangle(0, 784, 128, 80))
            .WithSprite(GameSprites.ThunderCharge2, new Rectangle(128, 784, 128, 80))
            .WithSprite(GameSprites.ThunderCharge3, new Rectangle(256, 784, 128, 80))
            .WithSprite(GameSprites.ThunderCharge4, new Rectangle(384, 784, 128, 80))
            .WithSprite(GameSprites.ThunderCharge5, new Rectangle(512, 784, 128, 80))
            .WithSprite(GameSprites.SmokeSmall, new Rectangle(0, 0, 48, 48))
        
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
            })
            .WithSpriteAnimation(GameSprites.ThunderAnimation, new[]
            {
                GameSprites.Thunder1,
                GameSprites.Thunder2,
                GameSprites.Thunder3,
                GameSprites.Thunder4,
                GameSprites.Thunder5,
            })
            .WithSpriteAnimation(GameSprites.ThunderChargeAnimationOn, new[]
            {
                GameSprites.ThunderCharge1,
                GameSprites.ThunderCharge2,
                GameSprites.ThunderCharge3,
                GameSprites.ThunderCharge4,
                GameSprites.ThunderCharge5,
            })
            .WithSpriteAnimation(GameSprites.ThunderChargeAnimationOff, new[]
            {
                GameSprites.ThunderCharge5,
                GameSprites.ThunderCharge4,
                GameSprites.ThunderCharge3,
                GameSprites.ThunderCharge2,
                GameSprites.ThunderCharge1,
            });
    }
}