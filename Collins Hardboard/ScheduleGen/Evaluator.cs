using System;
using System.Collections.Generic;
using System.Linq;
using Configuration_windows;
using ModelLib;
using ScheduleGen;
using StaticHelpers;

static class Evaluator
{
    /// <summary>
    /// Gathers the weight that the item should be scheduled. Based on any registered evaluators.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="line"></param>
    /// <returns></returns>
    public static PriorityItem Evaluate(ProductMasterItem item, string line, GenerationSettings settings)
    {
        int weight = 0;
        weight += EvaluateSales(settings,item);
        weight += EvaluateWidth(settings, item, line);
        weight += EvaluateProjection(settings, item);
        weight += EvaluateGrouping(settings, item,line);
        weight += EvaluateThickness(settings, item, line);
        weight += EvaluateWaste(settings, item);

        return new PriorityItem(item,weight);
    }

    private static int EvaluateThickness(GenerationSettings settings, ProductMasterItem item, string line)
    {
        if (ScheduleGenerator.Instance.GenerationData.LastWidth.ContainsKey(line) && ScheduleGenerator.Instance.GenerationData.LastWidth[line] != 0)
        {
            return settings .ThicknessWeight -
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
    private static int EvaluateSales(GenerationSettings settings, ProductMasterItem item)
    {
        // Sales calculation: weight - 5(days till due)

        int daysTillDue = -1;
        //check for due date
        List<MakeOrder> sales = ScheduleGenerator.Instance.GenerationData.SalesList.Where(s => s.MasterID == item.MasterID).ToList();
        if(sales.Any())
        {
            var sale = sales.OrderBy(s => s.DueDay).First();
            if (sale != null)
                daysTillDue = (int)(sale.DueDay - ScheduleGenerator.Instance.GenerationData.CurrentDay).TotalDays;

            if(daysTillDue != -1)
                return settings .SalesWeight - (5 * daysTillDue);
        }
        return 0;
    }


    /// <summary>
    /// Get the weight value of scheduling the item
    /// </summary>
    /// <param name="item">Item to potentially schedule</param>
    /// <returns>weight that the item should be scheduled</returns>
    private static int EvaluateWidth(GenerationSettings settings, ProductMasterItem item, string line)
    {
        if (ScheduleGenerator.Instance.GenerationData.LastWidth.ContainsKey(line) && ScheduleGenerator.Instance.GenerationData.LastWidth[line] != 0)
        {
            // Linear progression of weight with the dif in current working width.
            return settings .WidthWeight - (int)Math.Abs(ScheduleGenerator.Instance.GenerationData.LastWidth[line] - item.Width);
        }
        return 0;
    }

    /// <summary>
    /// Get the weight value of scheduling the item
    /// </summary>
    /// <param name="item">Item to potentially schedule</param>
    /// <returns>weight that the item should be scheduled</returns>
    private static int EvaluateWaste(GenerationSettings settings, ProductMasterItem item)
    {
        var wasteGeneratedEstimate = item.Waste * item.TargetSupply;
        var scheduledWaste = ScheduleGenerator.Instance.GenerationData.CurrentWaste + wasteGeneratedEstimate;
        if (scheduledWaste < StaticFactoryValuesManager.WasteMin || scheduledWaste > StaticFactoryValuesManager.WasteMax)
            return -settings .WasteWeight;
        return 0;
    }

    /// <summary>
    /// Get the weight value of scheduling the item
    /// </summary>
    /// <param name="item">Item to potentially schedule</param>
    /// <returns>weight that the item should be scheduled</returns>
    private static int EvaluateProjection(GenerationSettings settings, ProductMasterItem item)
    {
        if (ScheduleGenerator.Instance.GenerationData.PredictionList == null) return 0;

        return ScheduleGenerator.Instance.GenerationData.PredictionList.Any(p => p.MasterID == item.MasterID) ? settings.ProjectionWeight : 0;
    }

    /// <summary>
    /// Get the weight value of scheduling the item
    /// </summary>
    /// <param name="item">Item to potentially schedule</param>
    /// <returns>weight that the item should be scheduled</returns>
    private static int EvaluateGrouping(GenerationSettings settings, ProductMasterItem item, string line)
    {
        if (ScheduleGenerator.Instance.GenerationData.LastRunConfigurationGroups.ContainsKey(line))
        {
            var possibleGroups = MachineHandler.Instance.AllConfigGroups.Where(confGroup => confGroup.CanMake(item));
            if (possibleGroups.Contains(ScheduleGenerator.Instance.GenerationData.LastRunConfigurationGroups[line]))
                return settings.GroupWeight;
        }
        return 0;
    }

}