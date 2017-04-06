using ModelLib;
using ScheduleGen;

public class PriorityItem
{
    public ProductMasterItem Item { get; set; }
    public int Priority { get; set; } = 0;

    public PriorityItem(ProductMasterItem item, int priority)
    {
        Item = item;
        Priority = priority;
    }

    public static int Comparer(PriorityItem x, PriorityItem y)
    {
        if (x == null || y == null) return 0;

        return y.Priority - x.Priority;
    }
}