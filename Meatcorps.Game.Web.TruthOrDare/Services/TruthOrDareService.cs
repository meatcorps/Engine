using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Game.Web.TruthOrDare.Data;

namespace Meatcorps.Game.Web.TruthOrDare.Services;

public class TruthOrDareService
{
    private List<TruthDareItem>  _truthDareItems = new ();
    private Dictionary<(TruthDareType, string), Queue<TruthDareItem>> _truthDareItemsQueue = new ();
    
    public TruthOrDareService()
    {
        ParseData();
    }

    public string[] GetAllCategories(TruthDareType type)
    {
        return _truthDareItems.Where(x => x.Type == type).Select(x => x.Category).Distinct().ToArray();
    }

    public TruthDareItem? GetRandomTruthDareItem(TruthDareType type, string category)
    {
        var key = (type, category);
        if (!_truthDareItemsQueue.ContainsKey(key))
            return null;
        var item = _truthDareItemsQueue[key].Dequeue();
        FillQueue();
        return item;
    }
    
    private void ParseData()
    {
        var file = FileUtilities.GetFullPath("Assets", "truth_or_dare_full.txt");
        var lines = File.ReadLines(file).ToList();

        foreach (var line in lines)
        {
            var columns = line.Split('|');
            var item = new TruthDareItem
            {
                ItemId = int.Parse(columns[0]),
                Type = columns[1].Trim().Equals("truth", StringComparison.OrdinalIgnoreCase) ? TruthDareType.Truth : TruthDareType.Dare,
                Category = columns[2].Trim().ToCapitalize(),
                Description = columns[3],
            };
            
            _truthDareItems.Add(item);
            var key = (item.Type, item.Category);
            
            if (!_truthDareItemsQueue.ContainsKey(key))
                _truthDareItemsQueue[key] = new Queue<TruthDareItem>();
        }

        FillQueue();
    }

    private void FillQueue()
    {
        foreach (var queue in _truthDareItemsQueue)
        {
            if (queue.Value.Count > 0)
                continue;
            
            var items = _truthDareItems.Where(x => x.Type == queue.Key.Item1 && x.Category == queue.Key.Item2).ToList();
            
            foreach (var item in items)
                item.RandomValue = Random.Shared.Next(0, int.MaxValue);
            
            items.Sort((a, b) => a.RandomValue.CompareTo(b.RandomValue));
            foreach (var item in items)
                queue.Value.Enqueue(item);
            
        }
    }
}