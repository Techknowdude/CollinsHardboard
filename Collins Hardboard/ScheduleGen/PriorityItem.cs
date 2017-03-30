using ModelLib;

class PriorityItem
{
    public ProductMasterItem Item { get; set; }
    public int Priority { get; set; } = 0;

    public PriorityItem(ProductMasterItem item, int priority)
    {
        Item = item;
        Priority = priority;
    }
}