using System;
using System.Collections.Generic;
using System.Linq;
using Configuration_windows;
using ModelLib;
using ScheduleGen;

static class Evaluator
{
    private static int WidthWeight = 70;
    private static int ConfigGroupingWeight = 50;
    private static int ProjectedSalesWeight = 40;
    private static int SalesWeight = 100;

    /// <summary>
    /// Gathers the weight that the item should be scheduled. Based on any registered evaluators.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="line"></param>
    /// <returns></returns>
    public static PriorityItem Evaluate(ProductMasterItem item, string line)
    {
        int weight = 0;
        weight += EvaluateSales(item);
        weight += EvaluateWidth(item, line);
        weight += EvaluateProjection(item);
        weight += EvaluateGrouping(item,line);
        weight += EvaluateThickness(item, line);

        return new PriorityItem(item,weight);
    }

    private static int EvaluateThickness(ProductMasterItem item, string line)
    {
        if (ScheduleGenerator.Instance.GenerationData.LastWidth.ContainsKey(line) && ScheduleGenerator.Instance.GenerationData.LastWidth[line] != 0)
        {
            return 5 -
                   ((int) Math.Abs(ScheduleGenerator.Instance.GenerationData.LastThickness[line] - item.Thickness) * 8);
                // 2*.25 = 8*1
        }
        return 0;
    }


    /// <summary>
    /// Get the weight value of scheduling the item
    /// </summary>
    /// <param name="item">Item to potentially schedule</param>
    /// <returns>weight that the item should be scheduled</returns>
    private static int EvaluateSales(ProductMasterItem item)
    {
        // Sales calculation: weight - 5(days till due)

        int daysTillDue = -1;
        //check for due date
        List<MakeOrder> sales = ScheduleGenerator.Instance.GenerationData.SalesList.Where(s => s.MasterID == item.MasterID).ToList();
        if(sales.Any())
        {
            var sale = sales.OrderBy(s => s.DueDay).First();
            if (sale != null)
                daysTillDue = (int)(sale.DueDay - ScheduleGenerator.Instance.CurrentDay).TotalDays;

            if(daysTillDue != -1)
                return SalesWeight - (5 * daysTillDue);
        }
        return 0;
    }


    /// <summary>
    /// Get the weight value of scheduling the item
    /// </summary>
    /// <param name="item">Item to potentially schedule</param>
    /// <returns>weight that the item should be scheduled</returns>
    private static int EvaluateWidth(ProductMasterItem item, string line)
    {
        if(ScheduleGenerator.Instance.GenerationData.LastWidth.ContainsKey(line) && ScheduleGenerator.Instance.GenerationData.LastWidth[line] != 0)
        {
            // Linear progression of weight with the dif in current working width.
            return  WidthWeight - (int)Math.Abs(ScheduleGenerator.Instance.GenerationData.LastWidth[line] - item.Width);
        }
        return 0;
    }

    /// <summary>
    /// Get the weight value of scheduling the item
    /// </summary>
    /// <param name="item">Item to potentially schedule</param>
    /// <returns>weight that the item should be scheduled</returns>
    private static int EvaluateProjection(ProductMasterItem item)
    {
        if (ScheduleGenerator.Instance.GenerationData.PredictionList == null) return 0;

        return ScheduleGenerator.Instance.GenerationData.PredictionList.Any(p => p.MasterID == item.MasterID) ? ProjectedSalesWeight : 0;
    }

    /// <summary>
    /// Get the weight value of scheduling the item
    /// </summary>
    /// <param name="item">Item to potentially schedule</param>
    /// <returns>weight that the item should be scheduled</returns>
    private static int EvaluateGrouping(ProductMasterItem item, string line)
    {
        if (ScheduleGenerator.Instance.GenerationData.LastRunConfigurationGroups.ContainsKey(line))
        {
            var possibleGroups = MachineHandler.Instance.AllConfigGroups.Where(confGroup => confGroup.CanMake(item));
            if (possibleGroups.Contains(ScheduleGenerator.Instance.GenerationData.LastRunConfigurationGroups[line]))
                return ConfigGroupingWeight;
        }
        return 0;
    }

}