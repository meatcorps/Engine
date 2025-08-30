using System.Numerics;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;

namespace Meatcorps.Engine.RayLib.UI;

public static class UIAnchorHelper
{
    public static Vector2 ResolveAnchorPixel(Anchor anchor)
    {
        var renderTarget = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()
                           ?? throw new InvalidOperationException("IRenderTargetStrategy not registered.");
        return ResolveAnchorPixel(anchor, Vector2.Zero, renderTarget.RenderWidth, renderTarget.RenderHeight);
    }
    
    /// <summary>
    /// Resolve an anchor to pixel coordinates using the current IRenderTargetStrategy (screen-space).
    /// </summary>
    public static Vector2 ResolveAnchorPixel(Anchor anchor, Vector2 offsetPixels)
    {
        var renderTarget = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()
                           ?? throw new InvalidOperationException("IRenderTargetStrategy not registered.");
        return ResolveAnchorPixel(anchor, offsetPixels, renderTarget.RenderWidth, renderTarget.RenderHeight);
    }
    
    public static Vector2 ResolveAnchorPixel(Anchor anchor, int width, int height)
    {
        return ResolveAnchorPixel(anchor, Vector2.Zero, width, height);
    }

    /// <summary>
    /// Resolve an anchor to pixel coordinates using an explicit render width/height.
    /// </summary>
    public static Vector2 ResolveAnchorPixel(Anchor anchor, Vector2 offsetPixels, int width, int height)
    {
        var x = anchor switch
        {
            Anchor.TopLeft or Anchor.CenterLeft or Anchor.BottomLeft => 0f,
            Anchor.Top or Anchor.Center or Anchor.Bottom => width * 0.5f,
            Anchor.TopRight or Anchor.CenterRight or Anchor.BottomRight => width,
            _ => width * 0.5f
        };

        var y = anchor switch
        {
            Anchor.TopLeft or Anchor.Top or Anchor.TopRight => 0f,
            Anchor.CenterLeft or Anchor.Center or Anchor.CenterRight => height * 0.5f,
            Anchor.BottomLeft or Anchor.Bottom or Anchor.BottomRight => height,
            _ => height * 0.5f
        };

        return new Vector2(x, y) + offsetPixels;
    }
    
    public static Vector2 ResolveAnchorPixel(Anchor anchor, float width, float height)
    {
        return ResolveAnchorPixel(anchor, Vector2.Zero, width, height);
    }

    /// <summary>
    /// Resolve an anchor to pixel coordinates using an explicit render width/height.
    /// </summary>
    public static Vector2 ResolveAnchorPixel(Anchor anchor, Vector2 offsetPixels, float width, float height)
    {
        var x = anchor switch
        {
            Anchor.TopLeft or Anchor.CenterLeft or Anchor.BottomLeft => 0f,
            Anchor.Top or Anchor.Center or Anchor.Bottom => width * 0.5f,
            Anchor.TopRight or Anchor.CenterRight or Anchor.BottomRight => width,
            _ => width * 0.5f
        };

        var y = anchor switch
        {
            Anchor.TopLeft or Anchor.Top or Anchor.TopRight => 0f,
            Anchor.CenterLeft or Anchor.Center or Anchor.CenterRight => height * 0.5f,
            Anchor.BottomLeft or Anchor.Bottom or Anchor.BottomRight => height,
            _ => height * 0.5f
        };

        return new Vector2(x, y) + offsetPixels;
    }

    public static Vector2 AnchorToDirectionVector2(Anchor anchor, float multiply = 1)
    {
        var x = anchor switch
        {
            Anchor.TopLeft or Anchor.CenterLeft or Anchor.BottomLeft => -1,
            Anchor.Top or Anchor.Center or Anchor.Bottom => 0,
            Anchor.TopRight or Anchor.CenterRight or Anchor.BottomRight => 1,
            _ => 0
        };

        var y = anchor switch
        {
            Anchor.TopLeft or Anchor.Top or Anchor.TopRight => -1,
            Anchor.CenterLeft or Anchor.Center or Anchor.CenterRight => 0,
            Anchor.BottomLeft or Anchor.Bottom or Anchor.BottomRight => 1,
            _ => 0
        };

        return new Vector2(x, y) * multiply;
    }

    /// <summary>
    /// Given an element (rectangle) size, returns the TOP-LEFT position so the rectangle is aligned to the anchor.
    /// For example: Center → the rect will be centered on the anchor; BottomRight → the rect's bottom-right will sit on the anchor.
    /// </summary>
    public static Vector2 ResolveAlignedRectTopLeftPixel(Anchor anchor, Vector2 rectSizePixels, Vector2 offsetPixels)
    {
        var renderTarget = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()
                           ?? throw new InvalidOperationException("IRenderTargetStrategy not registered.");
        return ResolveAlignedRectTopLeftPixel(anchor, rectSizePixels, offsetPixels, renderTarget.RenderWidth,
            renderTarget.RenderHeight);
    }

    /// <summary>
    /// Same as ResolveAlignedRectTopLeftPx, but with explicit render width/height.
    /// </summary>
    public static Vector2 ResolveAlignedRectTopLeftPixel(Anchor anchor, Vector2 rectSizePixels, Vector2 offsetPixels,
        int renderWidth, int renderHeight)
    {
        var anchorPoint = ResolveAnchorPixel(anchor, Vector2.Zero, renderWidth, renderHeight);
        var alignment = GetAlignmentFactors(anchor);
        var topLeft = anchorPoint - new Vector2(alignment.X * rectSizePixels.X, alignment.Y * rectSizePixels.Y) +
                      offsetPixels;
        return topLeft;
    }

    public static Anchor InvertAnchor(Anchor anchor)
    {
        return anchor switch
        {
            Anchor.TopLeft => Anchor.BottomRight,
            Anchor.Top => Anchor.Bottom,
            Anchor.TopRight => Anchor.BottomLeft,
            Anchor.CenterLeft => Anchor.CenterRight,
            Anchor.Center => Anchor.Center,
            Anchor.CenterRight => Anchor.CenterLeft,
            Anchor.BottomLeft => Anchor.TopRight,
            Anchor.Bottom => Anchor.Top,
            Anchor.BottomRight => Anchor.TopLeft,
            _ => throw new ArgumentOutOfRangeException(nameof(anchor), anchor, null)
        };
    }

    /// <summary>
    /// Resolve the alignment factors for an anchor:
    /// Left/Top = 0, Center = 0.5, Right/Bottom = 1.
    /// </summary>
    private static Vector2 GetAlignmentFactors(Anchor anchor)
    {
        var horizontal = anchor switch
        {
            Anchor.TopLeft or Anchor.CenterLeft or Anchor.BottomLeft => 0f,
            Anchor.Top or Anchor.Center or Anchor.Bottom => 0.5f,
            Anchor.TopRight or Anchor.CenterRight or Anchor.BottomRight => 1f,
            _ => 0.5f
        };

        var vertical = anchor switch
        {
            Anchor.TopLeft or Anchor.Top or Anchor.TopRight => 0f,
            Anchor.CenterLeft or Anchor.Center or Anchor.CenterRight => 0.5f,
            Anchor.BottomLeft or Anchor.Bottom or Anchor.BottomRight => 1f,
            _ => 0.5f
        };

        return new Vector2(horizontal, vertical);
    }
}