namespace Meatcorps.Game.Snake.Data;

public class SnakePerkManager
{
    private readonly Player _player;
    private List<IConsumableItem> _perks = new();
    private List<int> _perkIds = new();

    public IReadOnlyList<IConsumableItem> Perks => _perks;

    public SnakePerkManager(Player player)
    {
        _player = player;
    }

    public void AddPerk(IConsumableItem perk)
    {
        if (perk.Rule != ConsumableRule.Refresh)
            if (!perk.Once(_player))
                return;

        switch (perk.Rule)
        {
            case ConsumableRule.Replace:
                for (var i = _perks.Count - 1; i >= 0; i--)
                {
                    if (_perks[i].Group == perk.Group)
                    {
                        _perks[i].End(_player);
                        _perks.RemoveAt(i);
                    }
                }

                _perks.Add(perk);
                break;
            case ConsumableRule.Refresh:
                var totalRefreshed = 0;
                foreach (var item in _perks)
                {
                    if (item.Group == perk.Group)
                    {
                        totalRefreshed++;
                        item.Refresh(_player, perk);
                    }
                }

                if (totalRefreshed > 0)
                {
                    if (perk.Once(_player))
                        _perks.Add(perk);
                }

                break;
            case ConsumableRule.Stack:
                _perks.Add(perk);
                break;
        }
    }

    public void Reset()
    {
        foreach (var perk in _perks)
        {
            perk.End(_player);
        }

        _perks.Clear();
    }

    public void Update(float deltaTime)
    {
        _player.Modifiers.Reset();
        _perkIds.Clear();

        for (var i = 0; i < _perks.Count; i++)
        {
            if (!_perks[i].Update(deltaTime, _player))
            {
                _perkIds.Add(i);
            }
        }

        for (var i = _perkIds.Count - 1; i >= 0; i--)
        {
            var idx = _perkIds[i];
            _perks[idx].End(_player);
            _perks.RemoveAt(idx);
        }

        _perkIds.Clear();
    }
}