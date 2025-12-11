using Meatcorps.Engine.Core.Interfaces.Components;
using Meatcorps.Engine.RayLib.Abstractions;

namespace Meatcorps.Engine.RayLib.Interfaces;

public interface IRaylibGameComponent: IGameComponent
{
    public void SetOwner(BaseGameObject owner);
}