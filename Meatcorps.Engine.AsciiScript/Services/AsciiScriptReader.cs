using System.Text;
using Meatcorps.Engine.AsciiScript.Data;
using Meatcorps.Engine.AsciiScript.Enums;

namespace Meatcorps.Engine.AsciiScript.Services;

public class AsciiScriptReader
{
    private readonly string[] _blocks;
    private readonly string[] _commands;
    private readonly string[] _variables;
    private StringBuilder _data = new StringBuilder();
    private List<AsciiScriptItem> _items = new();
    private int _lineNumber;
    
    public AsciiScriptReader(string[] blocks, string[] commands, string[] variables)
    {
        _blocks = blocks;
        _commands = commands;
        _variables = variables;
    }

    public void LoadFromFileAndParse(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException();
        _items.Clear();
        Reset();
        Read(File.ReadAllText(path));
    }
    
    public void LoadFromStringAndParse(string data)
    {
        _items.Clear();
        Reset();
        Read(data);
    }

    public void Reset()
    {
        _lineNumber = 0;
    }
    
    public bool ReadNext(out AsciiScriptItem item)
    {
        if (_lineNumber >= _items.Count)
        {
            item = default;
            return false;
        }
        item = _items[_lineNumber];
        _lineNumber++;
        return true;
    }

    public void Read(string data)
    {
        var lines = data.Replace("\r", "").Split("\n");
        var isBlock = false;
        var blockName = "";
        var currentLineNumber = 0;
        foreach (var line in lines.Select(line => line.TrimEnd()))
        {
            currentLineNumber++;
            if (line.Length > 2 && line.StartsWith("//"))
                continue;
            
            if (line.EndsWith(":"))
            {
                var name = line[..^1]; // without trailing ':'
                if (_blocks.Contains(name, StringComparer.Ordinal))
                {
                    isBlock = true;
                    blockName = name;
                    _data.Clear();
                    continue;
                }
            }
            else if (isBlock) // Block end
            {
                if (line == "END" + blockName)
                {
                    _items.Add(new AsciiScriptItem
                    {
                        Type = AsciiScriptItemType.Block,
                        Command = blockName.Trim(),
                        Value = _data.ToString(),
                        LineNumber = currentLineNumber
                    });
                    isBlock = false;
                    _data.Clear();
                }
                else
                {
                    _data.AppendLine(line);
                }
                continue;
            }

            if (_commands.Contains(line, StringComparer.Ordinal))
            {
                _items.Add(new AsciiScriptItem
                {
                    Type = AsciiScriptItemType.Command,
                    Command = line.Trim(),
                    Value = "",
                    LineNumber = currentLineNumber
                });
                continue;
            }

            var idx = line.IndexOf('=');
            if (idx > 0)
            {
                var key = line[..idx];
                var val = line[(idx + 1)..];
                
                if (_variables.Contains(key, StringComparer.Ordinal))
                    _items.Add(new AsciiScriptItem
                    {
                        Type = AsciiScriptItemType.Variable, 
                        Command = key, 
                        Value = val,
                        LineNumber = currentLineNumber
                    });
            }
        }
        
        if (isBlock)
            throw new Exception("Block not closed");
    }
}