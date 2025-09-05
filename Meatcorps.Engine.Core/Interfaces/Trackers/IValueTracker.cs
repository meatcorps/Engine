namespace Meatcorps.Engine.Core.Interfaces.Trackers;

public interface IValueTracker<TGroup> where TGroup : Enum
{
    void Change<T>(string name, T value);
    T Get<T>(string name);
}