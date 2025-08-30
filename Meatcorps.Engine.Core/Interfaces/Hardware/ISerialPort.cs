namespace Meatcorps.Engine.Core.Interfaces.Hardware;

public interface ISerialPort : IDisposable
{
    public void Open(string portName);
    public void Reconnect();
    public bool IsOpen { get; }
    public void Write(byte[] data);
    public IEnumerable<int> Read();
}