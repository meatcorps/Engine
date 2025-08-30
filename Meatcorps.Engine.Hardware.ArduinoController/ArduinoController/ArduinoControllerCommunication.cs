using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Meatcorps.Engine.Core.Interfaces.Hardware;

namespace Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;

public class ArduinoControllerCommunication : IDisposable
{
    private readonly ISerialPort _serialPort;
    
    public ControllerInputEnum ControllerState1 { get; private set; }
    public ControllerInputEnum ControllerState2 { get; private set; }

    private readonly ConcurrentQueue<(ButtonLightsEnum, ButtonLightsEnum)> _buttonLightsQue = new();
    private CancellationTokenSource _cts = new();

    private ButtonLightsEnum _previousButtonLights1;
    private ButtonLightsEnum _previousButtonLights2;
    
    public ArduinoControllerCommunication(string port)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            _serialPort = new RjcpSerialPort();
        else
            _serialPort = new DotNetSerialPort();
        
        _serialPort.Open(port);

        _ = Task.Run(Read);
    }

    private async Task Read()
    {
        while (!_cts.IsCancellationRequested)
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Reconnect();
                await Task.Delay(100);
                continue;
            }

            try
            {
                if (_buttonLightsQue.TryDequeue(out var buttonLights))
                {
                    _serialPort.Write([(byte)buttonLights.Item1, (byte)buttonLights.Item2]);
                    await Task.Delay(10);
                }
            }
            catch (Exception e)
            {
                _serialPort.Reconnect();
                Console.WriteLine(e);
                await Task.Delay(100);
                continue;
            }

            try
            {
                var data = _serialPort.Read();
                
                // ReSharper disable once PossibleMultipleEnumeration
                if (data.Count() != 3) 
                    continue;
                var counter = 0;
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var item in data)
                {
                    counter++;
                    if (counter == 0)
                        ControllerState1 = ~(ControllerInputEnum)item;
                    if (counter == 1)
                        ControllerState1 = ~(ControllerInputEnum)item;
                }
            } 
            catch (Exception e)
            {
                _serialPort.Reconnect();
                Console.WriteLine(e);
                await Task.Delay(100);
            }
            //await Task.Delay(2);
        }
    }

    public void SetButtonLights(ButtonLightsEnum? buttonLights1 = null, ButtonLightsEnum? buttonLights2 = null)
    {
        buttonLights1 ??= _previousButtonLights1;
        buttonLights2 ??= _previousButtonLights2;
        
        _buttonLightsQue.Enqueue((buttonLights1.Value, buttonLights2.Value));
        _previousButtonLights1 = buttonLights1.Value;
        _previousButtonLights2 = buttonLights2.Value;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _serialPort.Dispose();
    }
}