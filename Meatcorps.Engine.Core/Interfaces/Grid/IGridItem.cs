using System.Drawing;
using System.Numerics;
using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Core.Interfaces.Grid;

public interface IGridItem
{
    RectF BoundingBox { get; }
    object Owner { get; } 
}