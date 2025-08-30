using Meatcorps.Engine.Core.Interfaces.Hardware;
using Meatcorps.Engine.Hardware.ArduinoController.Utilities;
using RJCP.IO.Ports;

namespace Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;

public class RjcpSerialPort : ISerialPort
{
    private SerialPortStream? _serialPort;
    
    public void Open(string portName)
    {
        _serialPort = new SerialPortStream(portName, 19200);
        try
        {
            _serialPort.Open();
        } catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void Reconnect()
    {
        try
        {
            if (_serialPort?.IsOpen ?? false)
                _serialPort.Close();
            _serialPort?.Open();
        } catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public bool IsOpen => _serialPort?.IsOpen ?? false;
    
    public void Write(byte[] data)
    {
        _serialPort?.Write(data, 0, data.Length);
    }

    public IEnumerable<int> Read()
    {
        if (!_serialPort?.IsOpen ?? false)
            return [];
        
        return SerialPortDataParser.Parse(_serialPort?.ReadLine()!);
    }
    
    
    public void Dispose()
    {
        _serialPort?.Dispose();
    }
}