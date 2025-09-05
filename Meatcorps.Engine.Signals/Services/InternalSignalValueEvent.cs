using Meatcorps.Engine.Signals.Abstractions;

namespace Meatcorps.Engine.Signals.Services;

public class InternalSignalValueEvent<TGroup>: BaseSignalValueEvent<TGroup> where TGroup : Enum
{
    private readonly TGroup _group;

    public InternalSignalValueEvent(TGroup group)
    {
        _group = group;
    }
    
    public override TGroup GetGroup()
    {
        return _group;
    }
}