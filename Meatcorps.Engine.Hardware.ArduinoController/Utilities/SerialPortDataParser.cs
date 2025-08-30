using System.Runtime.CompilerServices;
using System.Text;

namespace Meatcorps.Engine.Hardware.ArduinoController.Utilities;

internal static class SerialPortDataParser
{
    public static IEnumerable<int> Parse(string data)
    {
        var value = 0;
        foreach (var charItem in data)
        {
            if (!IsDigitOrDash(charItem))
                continue;

            if (charItem == '-')
            {
                yield return value;
                value = 0; 
                continue;
            }
            value = value * 10 + (charItem - '0');
        }
        yield return value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsDigitOrDash(char c) => c == '-' || (c >= '0' && c <= '9');
}