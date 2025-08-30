namespace Meatcorps.Engine.Core.Interfaces.Storage;

public interface IKeyValueSaver<T>
{
    void SetTarget(IKeyValueDatabase<T> target);
    void Save();
}