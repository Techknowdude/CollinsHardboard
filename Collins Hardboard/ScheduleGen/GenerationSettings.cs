using System;

public class GenerationSettings
{

    public DateTime SalesOutlook { get; set; }
    public DateTime StartGen { get; set; }
    public DateTime EndGen { get; set; }
    public int SalesWeight { get; set; } = 100;
    public int GroupWeight { get; set; } = 70;
    public int ProjectionWeight { get; set; } = 50;
    public int WidthWeight { get; set; } = 60;
    public int WasteWeight { get; set; } = 40;
    public int ThicknessWeight { get; set; } = 5;

    public GenerationSettings()
    {
        StartGen = DateTime.Today;
        EndGen = DateTime.Today.AddDays(7);
        SalesOutlook = StartGen.AddDays(21);
    }
}