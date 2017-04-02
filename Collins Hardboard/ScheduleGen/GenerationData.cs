using System;
using System.Collections.Generic;
using System.Linq;
using Configuration_windows;
using ImportLib;
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
    public DateTime CurrentDay { get; set; } = DateTime.Today;
    public List<MakeOrder> PredictionList { get; set; } = new List<MakeOrder>();
    public List<PriorityItem> PriorityList { get; set; } = new List<PriorityItem>();
    public Dictionary<string, bool> ScheduledItem = new Dictionary<string, bool>();
    private List<MakeOrder> _salesList = new List<MakeOrder>();
    public double CurrentWaste { get; set; }
    public List<InventoryItem> CurrentInventory { get; set; }
    public List<PrereqMakeOrder> PrereqMakeOrders { get; set; }

    public SalesPrediction.SalesDurationEnum SalesOutlookDuration;

    public List<MakeOrder> SalesList
    {
        get { return _salesList; }
        set
        {
            _salesList = value;
            if(_salesList != null)
                _salesList.Sort(MakeOrder.DueDateComparerByDay);
        }
    }

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

        if (CurrentInventory == null)
            CurrentInventory = new List<InventoryItem>();
        CurrentInventory.Clear();

        if (PrereqMakeOrders == null)
            PrereqMakeOrders = new List<PrereqMakeOrder>();
        PrereqMakeOrders.Clear();
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
        int pcsMade = (int) (unitsMade * item.PiecesPerUnit);
        var line = StaticFactoryValuesManager.CoatingLines[lineIndex];

        LastWidth[line] = item.Width;
        LastThickness[line] = item.Thickness;

        // remove prediction
        var prediction = PredictionList.FirstOrDefault(p => p.MasterID == item.MasterID);
        if (prediction != null)
            PredictionList.Remove(prediction);
        
        // remove sale
        var sale = SalesList.FirstOrDefault(s => s.MasterID == item.MasterID);
        int pcsToRemove = pcsMade;

        while (sale != null)
        {
            if (sale.PiecesToMake > pcsToRemove)
            {
                sale.PiecesToMake -= pcsToRemove;
            }
            else
            {
               // made more or equal to the amount for this sale
                pcsToRemove -= sale.PiecesToMake;
                SalesList.Remove(sale);
                sale = SalesList.FirstOrDefault(s => s.MasterID == item.MasterID);
            }
        }

        // update inventory
        var invItem = CurrentInventory.FirstOrDefault(i => i.MasterID == item.MasterID);
        if (invItem != null)
        {
            invItem.Units += unitsMade;
        }
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

    public void SetInventory()
    {
        foreach (var allInventoryItem in StaticInventoryTracker.AllInventoryItems)
        {
            CurrentInventory.Add(new InventoryItem(allInventoryItem.ProductCode, allInventoryItem.Units,
                allInventoryItem.PiecesPerUnit, allInventoryItem.Grade, allInventoryItem.MasterID,
                allInventoryItem.InventoryItemID));
        }
    }

    public void RemoveFilledSales()
    {
        List<MakeOrder> filledSales = new List<MakeOrder>();

        // Sales list already sorted by due date. No issue with filling in order.
        foreach (var saleOrder in SalesList)
        {
            var invItem = CurrentInventory.FirstOrDefault(i => saleOrder.MasterID == i.MasterID);
            if (invItem != null)
            {
                // If enough in inventory, fill order and remove from inventory
                if (invItem.TotalPieces >= saleOrder.PiecesToMake)
                {
                    invItem.Units -= (saleOrder.PiecesToMake / invItem.PiecesPerUnit);
                    filledSales.Add(saleOrder);
                }
                else
                {
                    // not enough inventory. Remove what we can from inventory and reduce the sale order needed.
                    saleOrder.PiecesToMake -= invItem.TotalPieces;
                    invItem.Units = 0;
                }
            }
        }

        foreach (var filledSale in filledSales)
        {
            SalesList.Remove(filledSale);
        }
    }

    /// <summary>
    /// decrement inventory and update prediction data
    /// </summary>
    /// <param name="dayDif"></param>
    /// <param name="duration"></param>
    public void DecrementInventory(int dayDif)
    {
        // update inventory by lowering it by the number of days past
        foreach (var inventoryItem in CurrentInventory)
        {
            var forecast = StaticInventoryTracker.ForecastItems.FirstOrDefault(f => f.MasterID == inventoryItem.MasterID);
            ProductMasterItem master = StaticInventoryTracker.ProductMasterList.FirstOrDefault(m => m.MasterID == inventoryItem.MasterID);
            if (forecast != null && master != null)
            {
                try
                {
                    double turnUnits = 0;
                    switch (SalesOutlookDuration)
                    {
                        case SalesPrediction.SalesDurationEnum.LastMonth:
                            inventoryItem.Units -= forecast.AvgOneMonth / 30 * dayDif;
                            turnUnits = forecast.AvgOneMonth;
                            break;
                        case SalesPrediction.SalesDurationEnum.Last3Months:
                            inventoryItem.Units -= forecast.AvgThreeMonths / 30 * dayDif;
                            turnUnits = forecast.AvgThreeMonths;
                            break;
                        case SalesPrediction.SalesDurationEnum.Last6Months:
                            inventoryItem.Units -= forecast.AvgSixMonths / 30 * dayDif;
                            turnUnits = forecast.AvgSixMonths;
                            break;
                        case SalesPrediction.SalesDurationEnum.Last12Months:
                            inventoryItem.Units -= forecast.AvgTwelveMonths / 30 * dayDif;
                            turnUnits = forecast.AvgTwelveMonths;
                            break;
                        case SalesPrediction.SalesDurationEnum.LastYear:
                            inventoryItem.Units -= forecast.AvgPastYear / 30 * dayDif;
                            turnUnits = forecast.AvgPastYear;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    // check if the master item should be triggered to be scheduled.
                    if (master.TurnType == "T")
                    {
                        if (turnUnits <= 0)
                            turnUnits = 1;

                        double turns = inventoryItem.Units / turnUnits;
                        if (master.MinSupply < turns)
                        {
                            AddPredictionOrder(master);
                        }
                    }
                    else
                    {
                        if (inventoryItem.Units < master.MinSupply)
                        {
                            AddPredictionOrder(master);

                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

    }


    private List<MakeOrder> GetPredictions()
    {
        var predictions = new List<MakeOrder>();

        foreach (var master in StaticInventoryTracker.ProductMasterList)//.Where(p => p.MadeIn.Equals("Coating")))
        {
            double pieces = master.PiecesPerUnit * master.TargetSupply;

            StaticFunctions.OutputDebugLine("Creating new prediction for " + master);
            MakeOrder newOrder = new MakeOrder(master.MasterID, pieces) { DueDay = CurrentDay }; // assume the current day is the due date unless we have inventory data (Could have no inventory)
                                                                                                 // forecast out when the order should be due
            var inv = CurrentInventory.FirstOrDefault(i => i.MasterID == master.MasterID);
            if (inv != null)
            {
                double currentInv = inv.Units;
                double usedPerDay = GetAvgUnitsPerDay(master) * 30;
                int daysTillOut = (int)Math.Floor(currentInv / usedPerDay);
                newOrder.DueDay = CurrentDay.AddDays(daysTillOut);
                StaticFunctions.OutputDebugLine("Found inventory of " + currentInv + " for prediction " + master + " predicted to run out in " + daysTillOut + " days");
            }
            predictions.Add(newOrder);
        }

        return predictions;
    }
    private double GetAvgUnitsPerDay(ProductMasterItem nextItem)
    {
        double unitUsage = 0;
        var forecast = StaticInventoryTracker.ForecastItems.FirstOrDefault(f => f.MasterID == nextItem.MasterID);
        if (forecast != null)
        {
            switch (SalesOutlookDuration)
            {
                case SalesPrediction.SalesDurationEnum.LastMonth:
                    unitUsage = forecast.AvgOneMonth / 30;
                    break;
                case SalesPrediction.SalesDurationEnum.Last3Months:
                    unitUsage = forecast.AvgThreeMonths / 30;
                    break;
                case SalesPrediction.SalesDurationEnum.Last6Months:
                    unitUsage = forecast.AvgSixMonths / 30;
                    break;
                case SalesPrediction.SalesDurationEnum.Last12Months:
                    unitUsage = forecast.AvgTwelveMonths / 30;
                    break;
                case SalesPrediction.SalesDurationEnum.LastYear:
                    unitUsage = forecast.AvgPastYear / 30;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return unitUsage;
    }

    /// <summary>
    /// Initialize the generation data for the next generation 
    /// </summary>
    /// <param name="saleOrders"></param>
    /// <param name="generationSettings"></param>
    public void InitializeData(List<MakeOrder> saleOrders, GenerationSettings generationSettings)
    {
        // reset the state of the data
        Reset();
        // get a list of all sales orders
        SalesList = saleOrders;
        // initialize with current waste, line, and width
        CurrentDay = generationSettings.StartGen;
        // get snapshot of the current inventory
        SetInventory();
        RemoveFilledSales();
    }
}