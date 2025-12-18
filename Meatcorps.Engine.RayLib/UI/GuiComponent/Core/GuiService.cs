using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.Utilities;

namespace Meatcorps.Engine.RayLib.UI.GuiComponent.Core;

public class GuiService : IBackgroundService
{
    public List<BaseGuiItem> Containers { get; } = new();
    public BaseGuiItem? CurrentContainer { get; private set; }
    public List<BaseGuiItem> Items { get; } = new();
    public GuiServiceComponent CurrentComponent { get; set; } = null!;

    public ResourcePool<List<BaseGuiItem>> Pool { get; } =
        new(10, true, () => new List<BaseGuiItem>(), list => list.Clear());

    public void PreUpdate(float deltaTime)
    {
        Start();
    }

    public void Update(float deltaTime)
    {
    }

    public void LateUpdate(float deltaTime)
    {
        foreach (var item in Items) item.FinalizeLayout();
    }

    public void AddContainer(BaseGuiItem container)
    {
        Containers.Add(container);
    }

    private void Start()
    {
        Containers.Clear();
        Items.Clear();
        Pool.ReleaseAll();
    }

    public void AddItem(BaseGuiItem item)
    {
        Items.Add(item);
        item.Initialize(this);

        for (var i = Containers.Count - 1; i >= 0; i--)
            if (i == Containers.Count - 1)
                Containers[i].ChildGuiItemAdded(item);
            else
                Containers[i].MutateGuiItem(item);

        if (item.IsContainer)
        {
            CurrentContainer = item;
            Containers.Add(item);
            item.ContainerStart();
        }
    }

    public void Stop()
    {
        if (Containers.Count == 0)
            return;
        var item = Containers[^1];

        item.ContainerStop();

        Containers.Remove(item);
        CurrentContainer = Containers.Count > 0 ? Containers[^1] : null;
    }

    public void RegisterDraw(Action draw)
    {
        CurrentComponent.RegisterDraw(draw);
    }
}