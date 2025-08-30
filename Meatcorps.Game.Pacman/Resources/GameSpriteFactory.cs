using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Game.Pacman.GameEnums;
using Raylib_cs;

namespace Meatcorps.Game.Pacman.Resources;

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

            // Pacman
            .WithSpriteFromGrid(GameSprites.PacmanRight1, new PointInt(0, 0))
            .WithSpriteFromGrid(GameSprites.PacmanRight2, new PointInt(1, 0))
            .WithSpriteFromGrid(GameSprites.PacmanRight3, new PointInt(2, 0))
            .WithSpriteFromGrid(GameSprites.PacmanRight4, new PointInt(3, 0))
            .WithSpriteFromGrid(GameSprites.PacmanLeft1, new PointInt(0, 1))
            .WithSpriteFromGrid(GameSprites.PacmanLeft2, new PointInt(1, 1))
            .WithSpriteFromGrid(GameSprites.PacmanLeft3, new PointInt(2, 1))
            .WithSpriteFromGrid(GameSprites.PacmanLeft4, new PointInt(3, 1))
            .WithSpriteFromGrid(GameSprites.PacmanDown1, new PointInt(0, 2))
            .WithSpriteFromGrid(GameSprites.PacmanDown2, new PointInt(1, 2))
            .WithSpriteFromGrid(GameSprites.PacmanDown3, new PointInt(2, 2))
            .WithSpriteFromGrid(GameSprites.PacmanDown4, new PointInt(3, 2))
            .WithSpriteFromGrid(GameSprites.PacmanUp1, new PointInt(0, 3))
            .WithSpriteFromGrid(GameSprites.SuperPacmanRight1, new PointInt(4, 0))
            .WithSpriteFromGrid(GameSprites.SuperPacmanRight2, new PointInt(5, 0))
            .WithSpriteFromGrid(GameSprites.SuperPacmanRight3, new PointInt(6, 0))
            .WithSpriteFromGrid(GameSprites.SuperPacmanRight4, new PointInt(7, 0))
            .WithSpriteFromGrid(GameSprites.SuperPacmanLeft1, new PointInt(4, 1))
            .WithSpriteFromGrid(GameSprites.SuperPacmanLeft2, new PointInt(5, 1))
            .WithSpriteFromGrid(GameSprites.SuperPacmanLeft3, new PointInt(6, 1))
            .WithSpriteFromGrid(GameSprites.SuperPacmanLeft4, new PointInt(7, 1))
            .WithSpriteFromGrid(GameSprites.SuperPacmanDown1, new PointInt(5, 2))
            .WithSpriteFromGrid(GameSprites.SuperPacmanDown2, new PointInt(6, 2))
            .WithSpriteFromGrid(GameSprites.SuperPacmanDown3, new PointInt(7, 2))
            .WithSpriteFromGrid(GameSprites.SuperPacmanDown4, new PointInt(8, 2))
            .WithSpriteFromGrid(GameSprites.SuperPacmanUp1, new PointInt(4, 3))
            .WithSpriteFromGrid(GameSprites.SuperPacmanUp2, new PointInt(5, 3))
            .WithSpriteFromGrid(GameSprites.SuperPacmanEffect1, new PointInt(8, 0))
            .WithSpriteFromGrid(GameSprites.SuperPacmanEffect2, new PointInt(9, 0))
            .WithSpriteFromGrid(GameSprites.SuperPacmanEffect3, new PointInt(10, 0))
            .WithSpriteFromGrid(GameSprites.SuperPacmanEffect4, new PointInt(11, 0))

            // Ghosts
            .WithSpriteFromGrid(GameSprites.GhostBlinky1, new PointInt(12, 0))
            .WithSpriteFromGrid(GameSprites.GhostBlinky2, new PointInt(13, 0))
            .WithSpriteFromGrid(GameSprites.GhostBlinky3, new PointInt(14, 0))
            .WithSpriteFromGrid(GameSprites.GhostBlinky4, new PointInt(15, 0))
            .WithSpriteFromGrid(GameSprites.GhostBlinky5, new PointInt(16, 0))
            .WithSpriteFromGrid(GameSprites.GhostPinky1, new PointInt(12, 1))
            .WithSpriteFromGrid(GameSprites.GhostPinky2, new PointInt(13, 1))
            .WithSpriteFromGrid(GameSprites.GhostPinky3, new PointInt(14, 1))
            .WithSpriteFromGrid(GameSprites.GhostPinky4, new PointInt(15, 1))
            .WithSpriteFromGrid(GameSprites.GhostPinky5, new PointInt(16, 1))
            .WithSpriteFromGrid(GameSprites.GhostInky1, new PointInt(12, 2))
            .WithSpriteFromGrid(GameSprites.GhostInky2, new PointInt(13, 2))
            .WithSpriteFromGrid(GameSprites.GhostInky3, new PointInt(14, 2))
            .WithSpriteFromGrid(GameSprites.GhostInky4, new PointInt(15, 2))
            .WithSpriteFromGrid(GameSprites.GhostInky5, new PointInt(16, 2))
            .WithSpriteFromGrid(GameSprites.GhostClyde1, new PointInt(12, 3))
            .WithSpriteFromGrid(GameSprites.GhostClyde2, new PointInt(13, 3))
            .WithSpriteFromGrid(GameSprites.GhostClyde3, new PointInt(14, 3))
            .WithSpriteFromGrid(GameSprites.GhostClyde4, new PointInt(15, 3))
            .WithSpriteFromGrid(GameSprites.GhostClyde5, new PointInt(16, 3))
            .WithSpriteFromGrid(GameSprites.GhostScared1, new PointInt(12, 4))
            .WithSpriteFromGrid(GameSprites.GhostScared2, new PointInt(13, 4))
            .WithSpriteFromGrid(GameSprites.GhostScared3, new PointInt(14, 4))
            .WithSpriteFromGrid(GameSprites.GhostScared4, new PointInt(15, 4))
            .WithSpriteFromGrid(GameSprites.GhostScared5, new PointInt(16, 4))
            .WithSpriteFromGrid(GameSprites.GhostAngry, new PointInt(12, 5))
            .WithSpriteFromGrid(GameSprites.GhostScared, new PointInt(13, 5))
            .WithSpriteFromGrid(GameSprites.GhostShutdown, new PointInt(14, 5))
            .WithSpriteFromGrid(GameSprites.CollectibleOff, new PointInt(16, 6))
            .WithSpriteFromGrid(GameSprites.CollectibleOn, new PointInt(17, 6))
            .WithSpriteFromGrid(GameSprites.CollectibleMeat, new PointInt(18, 6))
            .WithSpriteFromGrid(GameSprites.SuperPacmanPowerUp1, new PointInt(16, 7))
            .WithSpriteFromGrid(GameSprites.SuperPacmanPowerUp2, new PointInt(17, 7))
            .WithSpriteFromGrid(GameSprites.GhostCharger, new PointInt(16, 8))

            // Walkable's
            .WithSpriteFromGrid(GameSprites.WalkableBottomRight, new PointInt(12, 6))
            .WithSpriteFromGrid(GameSprites.WalkableBottomLeft, new PointInt(13, 6))
            .WithSpriteFromGrid(GameSprites.WalkableBottomLeftRight, new PointInt(14, 6))
            .WithSpriteFromGrid(GameSprites.WalkableTopLeftRight, new PointInt(15, 6))
            .WithSpriteFromGrid(GameSprites.WalkableTopRight, new PointInt(12, 7))
            .WithSpriteFromGrid(GameSprites.WalkableTopLeft, new PointInt(13, 7))
            .WithSpriteFromGrid(GameSprites.WalkableTopBottomRight, new PointInt(14, 7))
            .WithSpriteFromGrid(GameSprites.WalkableTopBottomLeft, new PointInt(15, 7))
            .WithSpriteFromGrid(GameSprites.WalkableLeftRight, new PointInt(12, 8))
            .WithSpriteFromGrid(GameSprites.WalkableTopBottom, new PointInt(13, 8))
            .WithSpriteFromGrid(GameSprites.WalkableTopBottomLeftRight, new PointInt(14, 8))
            .WithSpriteFromGrid(GameSprites.WalkableBridge, new PointInt(15, 8))

            // Cars & Drones
            .WithSpriteFromGrid(GameSprites.Car, new Rect(0, 12, 1, 3))
            .WithSpriteFromGrid(GameSprites.CarLights, new Rect(1, 12, 1, 3))
            .WithSpriteFromGrid(GameSprites.CarGlow, new Rect(2, 12, 2, 3))
            .WithSpriteFromGrid(GameSprites.Drone1, new PointInt(4, 12))
            .WithSpriteFromGrid(GameSprites.Drone2, new PointInt(5, 12))
            .WithSpriteFromGrid(GameSprites.SmokeSmall, new PointInt(0, 15))
            .WithSpriteFromGrid(GameSprites.CityBackground, new Rect(0, 19, 40, 47))
            .WithSprite(GameSprites.SmokeLarge, new Rectangle(0, 1072, 640, 360))

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
            .WithSpriteAnimation(GameSprites.PacmanRightAnimation, new[]
            {
                GameSprites.PacmanRight1,
                GameSprites.PacmanRight2,
                GameSprites.PacmanRight3,
                GameSprites.PacmanRight4,
            })
            .WithSpriteAnimation(GameSprites.PacmanLeftAnimation, new[]
            {
                GameSprites.PacmanLeft1,
                GameSprites.PacmanLeft2,
                GameSprites.PacmanLeft3,
                GameSprites.PacmanLeft4,
            })
            .WithSpriteAnimation(GameSprites.PacmanDownAnimation, new[]
            {
                GameSprites.PacmanDown1,
                GameSprites.PacmanDown2,
                GameSprites.PacmanDown3,
                GameSprites.PacmanDown4,
            })
            .WithSpriteAnimation(GameSprites.PacmanUpAnimation, new[]
            {
                GameSprites.PacmanUp1,
            })
            .WithSpriteAnimation(GameSprites.SuperPacmanRightAnimation, new[]
            {
                GameSprites.SuperPacmanRight1,
                GameSprites.SuperPacmanRight2,
                GameSprites.SuperPacmanRight3,
                GameSprites.SuperPacmanRight4,
            })
            .WithSpriteAnimation(GameSprites.SuperPacmanLeftAnimation, new[]
            {
                GameSprites.SuperPacmanLeft1,
                GameSprites.SuperPacmanLeft2,
                GameSprites.SuperPacmanLeft3,
                GameSprites.SuperPacmanLeft4,
            })
            .WithSpriteAnimation(GameSprites.SuperPacmanDownAnimation, new[]
            {
                GameSprites.SuperPacmanDown1,
                GameSprites.SuperPacmanDown2,
                GameSprites.SuperPacmanDown3,
                GameSprites.SuperPacmanDown4,
            })
            .WithSpriteAnimation(GameSprites.SuperPacmanUpAnimation, new[]
            {
                GameSprites.SuperPacmanUp1,
                GameSprites.SuperPacmanUp2,
            })
            .WithSpriteAnimation(GameSprites.SuperPacmanEffectAnimation, new[]
            {
                GameSprites.SuperPacmanEffect1,
                GameSprites.SuperPacmanEffect2,
                GameSprites.SuperPacmanEffect3,
                GameSprites.SuperPacmanEffect4,
            })
            .WithSpriteAnimation(GameSprites.GhostBlinkyAnimation, new[]
            {
                GameSprites.GhostBlinky2,
                GameSprites.GhostBlinky3,
                GameSprites.GhostBlinky4,
                GameSprites.GhostBlinky5,
            })
            .WithSpriteAnimation(GameSprites.GhostPinkyAnimation, new[]
            {
                GameSprites.GhostPinky2,
                GameSprites.GhostPinky3,
                GameSprites.GhostPinky4,
                GameSprites.GhostPinky5,
            })
            .WithSpriteAnimation(GameSprites.GhostInkyAnimation, new[]
            {
                GameSprites.GhostInky2,
                GameSprites.GhostInky3,
                GameSprites.GhostInky4,
                GameSprites.GhostInky5,
            })
            .WithSpriteAnimation(GameSprites.GhostClydeAnimation, new[]
            {
                GameSprites.GhostClyde2,
                GameSprites.GhostClyde3,
                GameSprites.GhostClyde4,
                GameSprites.GhostClyde5,
            })
            .WithSpriteAnimation(GameSprites.GhostScaredAnimation, new[]
            {
                GameSprites.GhostScared2,
                GameSprites.GhostScared3,
                GameSprites.GhostScared4,
                GameSprites.GhostScared5,
            })
            .WithSpriteAnimation(GameSprites.DroneAnimation, new[]
            {
                GameSprites.Drone1,
                GameSprites.Drone2,
            });
    }
}