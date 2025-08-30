namespace Meatcorps.Engine.Core.Enums;

public enum EaseType
{
    Linear,
    EaseIn,        // Quadratic
    EaseOut,       // Quadratic
    EaseInOut,     // Smoothstep

    EaseInCubic,
    EaseOutCubic,
    EaseInOutCubic,

    EaseInQuart,
    EaseOutQuart,
    EaseInOutQuart,

    EaseInQuint,
    EaseOutQuint,
    EaseInOutQuint,

    EaseInSine,
    EaseOutSine,
    EaseInOutSine,

    EaseInBack,
    EaseOutBack,
    EaseInOutBack,
    
    EaseInBounce,
    EaseOutBounce,
    EaseInOutBounce,

    EaseInElastic,
    EaseOutElastic,
    EaseInOutElastic
}