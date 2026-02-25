using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Game.CyberMaze.GameEnums;
using Raylib_cs;

namespace Meatcorps.Game.CyberMaze.Resources;

public static class GameSpriteFactory
{
    public static Texture2DItem<GameSprites> Load()
    {
        return new Texture2DItem<GameSprites>("Assets/GameSprites.png")
            .WithFilter(TextureFilter.Point)
            .WithGridSize(new PointInt(16, 16))
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

            // CyberPlayer
            .WithSpriteFromGrid(GameSprites.CyberPlayerRight1, new PointInt(0, 0))
            .WithSpriteFromGrid(GameSprites.CyberPlayerRight2, new PointInt(1, 0))
            .WithSpriteFromGrid(GameSprites.CyberPlayerRight3, new PointInt(2, 0))
            .WithSpriteFromGrid(GameSprites.CyberPlayerRight4, new PointInt(3, 0))
            .WithSpriteFromGrid(GameSprites.CyberPlayerLeft1, new PointInt(0, 1))
            .WithSpriteFromGrid(GameSprites.CyberPlayerLeft2, new PointInt(1, 1))
            .WithSpriteFromGrid(GameSprites.CyberPlayerLeft3, new PointInt(2, 1))
            .WithSpriteFromGrid(GameSprites.CyberPlayerLeft4, new PointInt(3, 1))
            .WithSpriteFromGrid(GameSprites.CyberPlayerDown1, new PointInt(0, 2))
            .WithSpriteFromGrid(GameSprites.CyberPlayerDown2, new PointInt(1, 2))
            .WithSpriteFromGrid(GameSprites.CyberPlayerDown3, new PointInt(2, 2))
            .WithSpriteFromGrid(GameSprites.CyberPlayerDown4, new PointInt(3, 2))
            .WithSpriteFromGrid(GameSprites.CyberPlayerUp1, new PointInt(0, 3))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerRight1, new PointInt(4, 0))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerRight2, new PointInt(5, 0))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerRight3, new PointInt(6, 0))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerRight4, new PointInt(7, 0))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerLeft1, new PointInt(4, 1))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerLeft2, new PointInt(5, 1))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerLeft3, new PointInt(6, 1))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerLeft4, new PointInt(7, 1))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerDown1, new PointInt(5, 2))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerDown2, new PointInt(6, 2))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerDown3, new PointInt(7, 2))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerDown4, new PointInt(8, 2))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerUp1, new PointInt(4, 3))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerUp2, new PointInt(5, 3))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerEffect1, new PointInt(8, 0))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerEffect2, new PointInt(9, 0))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerEffect3, new PointInt(10, 0))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerEffect4, new PointInt(11, 0))

            // Ghosts
            .WithSpriteFromGrid(GameSprites.GhostAttacker1, new PointInt(12, 0))
            .WithSpriteFromGrid(GameSprites.GhostAttacker2, new PointInt(13, 0))
            .WithSpriteFromGrid(GameSprites.GhostAttacker3, new PointInt(14, 0))
            .WithSpriteFromGrid(GameSprites.GhostAttacker4, new PointInt(15, 0))
            .WithSpriteFromGrid(GameSprites.GhostAttacker5, new PointInt(16, 0))
            .WithSpriteFromGrid(GameSprites.GhostAnother1, new PointInt(12, 1))
            .WithSpriteFromGrid(GameSprites.GhostAnother2, new PointInt(13, 1))
            .WithSpriteFromGrid(GameSprites.GhostAnother3, new PointInt(14, 1))
            .WithSpriteFromGrid(GameSprites.GhostAnother4, new PointInt(15, 1))
            .WithSpriteFromGrid(GameSprites.GhostAnother5, new PointInt(16, 1))
            .WithSpriteFromGrid(GameSprites.GhostStrange1, new PointInt(12, 2))
            .WithSpriteFromGrid(GameSprites.GhostStrange2, new PointInt(13, 2))
            .WithSpriteFromGrid(GameSprites.GhostStrange3, new PointInt(14, 2))
            .WithSpriteFromGrid(GameSprites.GhostStrange4, new PointInt(15, 2))
            .WithSpriteFromGrid(GameSprites.GhostStrange5, new PointInt(16, 2))
            .WithSpriteFromGrid(GameSprites.GhostEmo1, new PointInt(12, 3))
            .WithSpriteFromGrid(GameSprites.GhostEmo2, new PointInt(13, 3))
            .WithSpriteFromGrid(GameSprites.GhostEmo3, new PointInt(14, 3))
            .WithSpriteFromGrid(GameSprites.GhostEmo4, new PointInt(15, 3))
            .WithSpriteFromGrid(GameSprites.GhostEmo5, new PointInt(16, 3))
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
            .WithSpriteFromGrid(GameSprites.CollectibleCheese, new PointInt(19, 6))
            .WithSpriteFromGrid(GameSprites.DutchFlag, new PointInt(19, 7))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerPowerUp1, new PointInt(16, 7))
            .WithSpriteFromGrid(GameSprites.SuperCyberPlayerPowerUp2, new PointInt(17, 7))
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
            .WithSpriteFromGrid(GameSprites.ShoutOut, new Rect(0, 16, 14, 2))
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
            .WithSpriteAnimation(GameSprites.CyberPlayerRightAnimation, new[]
            {
                GameSprites.CyberPlayerRight1,
                GameSprites.CyberPlayerRight2,
                GameSprites.CyberPlayerRight3,
                GameSprites.CyberPlayerRight4,
            })
            .WithSpriteAnimation(GameSprites.CyberPlayerLeftAnimation, new[]
            {
                GameSprites.CyberPlayerLeft1,
                GameSprites.CyberPlayerLeft2,
                GameSprites.CyberPlayerLeft3,
                GameSprites.CyberPlayerLeft4,
            })
            .WithSpriteAnimation(GameSprites.CyberPlayerDownAnimation, new[]
            {
                GameSprites.CyberPlayerDown1,
                GameSprites.CyberPlayerDown2,
                GameSprites.CyberPlayerDown3,
                GameSprites.CyberPlayerDown4,
            })
            .WithSpriteAnimation(GameSprites.CyberPlayerUpAnimation, new[]
            {
                GameSprites.CyberPlayerUp1,
            })
            .WithSpriteAnimation(GameSprites.SuperCyberPlayerRightAnimation, new[]
            {
                GameSprites.SuperCyberPlayerRight1,
                GameSprites.SuperCyberPlayerRight2,
                GameSprites.SuperCyberPlayerRight3,
                GameSprites.SuperCyberPlayerRight4,
            })
            .WithSpriteAnimation(GameSprites.SuperCyberPlayerLeftAnimation, new[]
            {
                GameSprites.SuperCyberPlayerLeft1,
                GameSprites.SuperCyberPlayerLeft2,
                GameSprites.SuperCyberPlayerLeft3,
                GameSprites.SuperCyberPlayerLeft4,
            })
            .WithSpriteAnimation(GameSprites.SuperCyberPlayerDownAnimation, new[]
            {
                GameSprites.SuperCyberPlayerDown1,
                GameSprites.SuperCyberPlayerDown2,
                GameSprites.SuperCyberPlayerDown3,
                GameSprites.SuperCyberPlayerDown4,
            })
            .WithSpriteAnimation(GameSprites.SuperCyberPlayerUpAnimation, new[]
            {
                GameSprites.SuperCyberPlayerUp1,
                GameSprites.SuperCyberPlayerUp2,
            })
            .WithSpriteAnimation(GameSprites.SuperCyberPlayerEffectAnimation, new[]
            {
                GameSprites.SuperCyberPlayerEffect1,
                GameSprites.SuperCyberPlayerEffect2,
                GameSprites.SuperCyberPlayerEffect3,
                GameSprites.SuperCyberPlayerEffect4,
            })
            .WithSpriteAnimation(GameSprites.GhostAttackerAnimation, new[]
            {
                GameSprites.GhostAttacker2,
                GameSprites.GhostAttacker3,
                GameSprites.GhostAttacker4,
                GameSprites.GhostAttacker5,
            })
            .WithSpriteAnimation(GameSprites.GhostAnotherAnimation, new[]
            {
                GameSprites.GhostAnother2,
                GameSprites.GhostAnother3,
                GameSprites.GhostAnother4,
                GameSprites.GhostAnother5,
            })
            .WithSpriteAnimation(GameSprites.GhostStrangeAnimation, new[]
            {
                GameSprites.GhostStrange2,
                GameSprites.GhostStrange3,
                GameSprites.GhostStrange4,
                GameSprites.GhostStrange5,
            })
            .WithSpriteAnimation(GameSprites.GhostEmoAnimation, new[]
            {
                GameSprites.GhostEmo2,
                GameSprites.GhostEmo3,
                GameSprites.GhostEmo4,
                GameSprites.GhostEmo5,
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