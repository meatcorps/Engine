using System.Globalization;
using Meatcorps.Engine.AsciiScript.Data;
using Meatcorps.Engine.AsciiScript.Enums;
using Meatcorps.Engine.AsciiScript.Interfaces;
using Meatcorps.Engine.AsciiScript.Services;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Utilities;

namespace Meatcorps.Engine.AsciiScript.Commands;

public class DelayCommand : IAsciiScriptCommand
{
    public AsciiScriptItemType ScriptType => AsciiScriptItemType.Variable;
    public string Command { get; }
    private TimerOn _timer;
    private Action<TimerOn, bool> _action;
    
    public DelayCommand(string command = "DELAY", Action<TimerOn, bool>? action = null)
    {
        _action = action ?? ((x, y) => { });
        Command = command;
    }
    
    public bool Accept(AsciiScriptItem scriptItem)
    {
        return true; // The service will already check. No special acceptance is needed.
    }

    public void Initialize(AsciiScriptItem scriptItem, AsciiScriptParser parser)
    {
        if (!int.TryParse(scriptItem.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ms))
            throw new FormatException($"Invalid DELAY value: '{scriptItem.Value}'");
        _timer = new TimerOn(ms);
        _timer.Reset();
    }

    public void Execute(AsciiScriptItem scriptItem, AsciiScriptParser parser, int runs)
    {
        var firstTick = _timer.Elapsed.EqualsSafe(0);
        _timer.Update(true, parser.DeltaTime);
        _action(_timer, firstTick);
        parser.State = _timer.Output ? ScriptParserState.Running : ScriptParserState.Paused;
    }
}