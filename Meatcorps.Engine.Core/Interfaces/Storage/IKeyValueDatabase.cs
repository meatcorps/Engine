namespace Meatcorps.Engine.Core.Interfaces.Storage;

public interface IKeyValueDatabase<T> : IDictionary<T, object>
{
    bool Dirty { get; set; }
}