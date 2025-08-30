using Meatcorps.Engine.AsciiScript.Data;
using Meatcorps.Engine.AsciiScript.Enums;
using Meatcorps.Engine.AsciiScript.Interfaces;
using Meatcorps.Engine.AsciiScript.Services;

namespace Meatcorps.Engine.AsciiScript.Commands;

public class BlockGridCommand : IAsciiScriptCommand
{
    private readonly Action<List<List<char>>> _action;
    public AsciiScriptItemType ScriptType => AsciiScriptItemType.Block;
    public string Command { get; }
    public List<List<char>> Grid { get; private set; }
    
    public BlockGridCommand(string command, Action<List<List<char>>> action)
    {
        _action = action;
        Command = command;
    }
    
    public bool Accept(AsciiScriptItem scriptItem)
    {
        return true;
    }

    public void Initialize(AsciiScriptItem scriptItem, AsciiScriptParser parser)
    {
        Grid = new List<List<char>>();
        var lines = scriptItem.Value.Replace("\r", "").Split('\n');

        foreach (var raw in lines)
        {
            var line = raw.TrimEnd();                // avoid trailing spaces becoming cells
            if (line.Length == 0) continue;         // skip empty last line
            Grid.Add(line.ToCharArray().ToList());
        }
    }

    public void Execute(AsciiScriptItem scriptItem, AsciiScriptParser parser, int runs)
    {
        _action(Grid);
    }
}