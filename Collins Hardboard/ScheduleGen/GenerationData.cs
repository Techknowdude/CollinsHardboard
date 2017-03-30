using System.Collections.Generic;
using Configuration_windows;
using ScheduleGen;

/// <summary>
/// Holds the state of the generation data.
/// </summary>
class GenerationData
{
    public Dictionary<string,double> Width { get; set; } = new Dictionary<string, double>();
    public Dictionary<string,double> Thickness { get; set; } = new Dictionary<string, double>();
    public Dictionary<string, ConfigurationGroup> LastRunConfigurationGroups { get; set; } = new Dictionary<string, ConfigurationGroup>();
    public Dictionary<Machine, ConfigTime> LastRunMachine { get; set; } = new Dictionary<Machine, ConfigTime>();
    public List<MakeOrder> SalesList { get; set; } = new List<MakeOrder>();
    public List<MakeOrder> PredictionList { get; set; } = new List<MakeOrder>();
    public List<PriorityItem> PriorityList { get; set; } = new List<PriorityItem>();

    /// <summary>
    /// Resets all data
    /// </summary>
    public void Reset()
    {
        Width.Clear();
        Thickness.Clear();
        LastRunConfigurationGroups.Clear();
        SalesList.Clear();
        PredictionList.Clear();
        LastRunMachine.Clear();
        PriorityList.Clear();
    }

    /// <summary>
    /// Resets the data for a line. Sales, machines and prediction are not included.
    /// </summary>
    /// <param name="line"></param>
    public void Reset(string line)
    {
        Width[line] = -1;
        Thickness[line] = -1;
        LastRunConfigurationGroups[line] = null;
    }
}