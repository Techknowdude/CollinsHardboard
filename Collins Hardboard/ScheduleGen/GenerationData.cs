using System;
using System.Collections.Generic;
using System.Linq;
using Configuration_windows;
using ModelLib;
using ScheduleGen;
using StaticHelpers;

/// <summary>
/// Holds the state of the generation data.
/// </summary>
public class GenerationData
{
    public Dictionary<string,double> LastWidth { get; set; } = new Dictionary<string, double>();
    public Dictionary<string,double> LastThickness { get; set; } = new Dictionary<string, double>();
    public Dictionary<string, ConfigurationGroup> LastRunConfigurationGroups { get; set; } = new Dictionary<string, ConfigurationGroup>();
    public Dictionary<Machine, ConfigTime> LastRunMachine { get; set; } = new Dictionary<Machine, ConfigTime>();
    public List<MakeOrder> SalesList { get; set; } = new List<MakeOrder>();
    public List<MakeOrder> PredictionList { get; set; } = new List<MakeOrder>();
    public List<PriorityItem> PriorityList { get; set; } = new List<PriorityItem>();
    public Dictionary<string, bool> ScheduledItem = new Dictionary<string, bool>();
    public double CurrentWaste { get; set; }

    /// <summary>
    /// Resets all data
    /// </summary>
    public void Reset()
    {
        LastWidth.Clear();
        LastThickness.Clear();
        LastRunConfigurationGroups.Clear();
        SalesList.Clear();
        PredictionList.Clear();
        LastRunMachine.Clear();
        PriorityList.Clear();
        foreach (var line in StaticFactoryValuesManager.CoatingLines)
        {
            ScheduledItem[line] = false;
        }
        CurrentWaste = StaticFactoryValuesManager.CurrentWaste;
    }

    /// <summary>
    /// Resets the data for a line. Sales, machines and prediction are not included.
    /// </summary>
    /// <param name="line"></param>
    public void Reset(string line)
    {
        LastWidth[line] = -1;
        LastThickness[line] = -1;
        LastRunConfigurationGroups[line] = null;
        ScheduledItem[line] = false;
    }

    /// <summary>
    /// Set the state of the generation data to be ready for the next shift to schedule
    /// </summary>
    public void ResetForNextShift()
    {
        foreach (var keyValuePair in ScheduledItem)
        {
            ScheduledItem[keyValuePair.Key] = false;
        }
    }

    /// <summary>
    /// Update schedule generation data with the last scheduled item on the line
    /// </summary>
    /// <param name="item"></param>
    /// <param name="lineIndex"></param>
    public void MarkItemScheduled(ProductMasterItem item, int lineIndex, double unitsMade)
    {
        var line = StaticFactoryValuesManager.CoatingLines[lineIndex];

        LastWidth[line] = item.Width;
        LastThickness[line] = item.Thickness;

        var prediction = PredictionList.FirstOrDefault(p => p.MasterID == item.MasterID);
        if (prediction != null)
            PredictionList.Remove(prediction);

        CurrentWaste += item.Waste * unitsMade;
    }

    /// <summary>
    /// Adds a prediction listing for the item to be scheduled.
    /// </summary>
    /// <param name="master"></param>
    public void AddPredictionOrder(ProductMasterItem master)
    {
        var order = PredictionList.FirstOrDefault(p => p.MasterID == master.MasterID);
        if (order == null)
        {
            PredictionList.Add(new MakeOrder(master.MasterID, master.TargetSupply * master.PiecesPerUnit));
        }
        else
        {
            order.PiecesToMake += (int)(master.PiecesPerUnit * master.TargetSupply);
        }
    }

    public void CreatePriorityList(List<ProductMasterItem> masterItemsAvailableToMake, string coatingLine)
    {
        if(PriorityList == null)
            PriorityList = new List<PriorityItem>();

        PriorityList.Clear();

        foreach (var productMasterItem in masterItemsAvailableToMake)
        {
            PriorityList.Add(Evaluator.Evaluate(productMasterItem, coatingLine));
        }

        PriorityList.Sort(PriorityItem.Comparer);
    }
}