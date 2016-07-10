using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using Configuration_windows;
using Microsoft.Win32;
using Microsoft.Office.Interop.Excel;
using StaticHelpers;
using Window = System.Windows.Window;

namespace ProductionScheduler
{
    /// <summary>
    /// Interaction logic for PressScheduleWindow.xaml
    /// </summary>
    public partial class PressScheduleWindow : Window
    {
        private static ObservableCollection<PressPlateConfigurationControl> _weekControls = new ObservableCollection<PressPlateConfigurationControl>();
        public static ObservableCollection<PressPlateConfigurationControl> WeekControls
        {
            get { return _weekControls; }
        }

        public PressScheduleWindow()
        {
            InitializeComponent();
            PressManager.Window = this;
            PressManager.Load();
            foreach (var plateConfiguration in PressManager.PlateConfigurations)
            {
                WeekControls.Add(new PressPlateConfigurationControl(plateConfiguration));
            }
            DataContext = this;
        }

        public void UpdateControls()
        {
            WeekControls.Clear();
            foreach (var plateConfiguration in PressManager.PlateConfigurations)
            {
                WeekControls.Add(new PressPlateConfigurationControl(plateConfiguration));
            }
        }

        private Int32 WeekInYear(DateTime time)
        {
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstDay,
                    CalendarControl.StartOfWeek);
        }

        private bool SameWeek(DateTime week, DateTime startDate)
        {
            if(week.Year != startDate.Year) return false;

            Int32 weekNumber = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(week, CalendarWeekRule.FirstDay,
                    CalendarControl.StartOfWeek);
            Int32 startNumber = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(startDate, CalendarWeekRule.FirstDay,
                    CalendarControl.StartOfWeek);

            return weekNumber == startNumber;
        }

        #region Possible way to schedule

        //public static double UnitsMade(ProductMasterItem item, DateTime day)
        //{
        //    double made = 0;
        //    if(!IsLoaded)
        //        Load(datFile,null);
        //    foreach (var week in WeekControls)
        //    {
        //        if(week.Week >= day && week.Week < day.AddDays(7)) // if correct week
        //        {
        //            DateTime current = week.Week;
        //            foreach (var control in week.ControlsList)
        //        {
        //                if(day)
        //        }}
        //    }

        //    return made;
        //}

        #endregion


        private void AddItemButton_OnClick(object sender, RoutedEventArgs e)
        {
            WeekControls.Add( new PressPlateConfigurationControl( PressManager.Instance.CreateNewConfig()));
        }

        //public void MoveUp(PressItemControl pressItemControl, PressWeekControl pressWeekControl)
        //{
        //    if (pressWeekControl == _weekControls[0])
        //    {
        //        DateTime prevTime = _weekControls[0].Week.AddDays(-7);
        //        _weekControls.Insert(0,new PressWeekControl(prevTime,this));
        //        _weekControls[0].AddItemToBottom(pressItemControl);
        //        pressWeekControl.RemoveItem(pressItemControl);
        //    }
        //    else
        //    {
        //        Int32 index = _weekControls.IndexOf(pressWeekControl);

        //        _weekControls[index - 1].AddItemToBottom(pressItemControl);
        //        pressWeekControl.RemoveItem(pressItemControl);
        //    }
        //}

        //public void MoveDown(PressItemControl pressItemControl, PressWeekControl pressWeekControl)
        //{
        //    if (pressWeekControl == _weekControls.Last())
        //    {
        //        DateTime prevTime = _weekControls.Last().Week.AddDays(7);
        //        _weekControls.Add(new PressWeekControl(prevTime, this));
        //        _weekControls.Last().AddItemToTop(pressItemControl);
        //        pressWeekControl.RemoveItem(pressItemControl);
        //    }
        //    else
        //    {
        //        Int32 index = _weekControls.IndexOf(pressWeekControl);

        //        _weekControls[index + 1].AddItemToTop(pressItemControl);
        //        pressWeekControl.RemoveItem(pressItemControl);
        //    }
            
        //}

        private void SaveMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            if (saveFileDialog.ShowDialog() == true)
            {
                PressManager.Save(saveFileDialog.FileName);
            }
        }

      //  private void Save(string fileName)
      //  {
      //      try
      //      {
      //          using (BinaryWriter writer = new BinaryWriter(new FileStream(fileName, FileMode.OpenOrCreate)))
      //          {
      //              writer.Write(WeekControls.Count);
      //              foreach (var pressWeekControl in WeekControls)
      //              {
      ////                  pressWeekControl.Save(writer);
      //              }
      //          }
      //      }
      //      catch (Exception)
      //      {
      //          MessageBox.Show("Save failed.");
      //      }
      //  }

        private void LoadMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog(){Multiselect = false};
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                PressManager.Load();
                WeekControls.Clear();
                foreach (var plateConfiguration in PressManager.PlateConfigurations)
                {
                    WeekControls.Add(new PressPlateConfigurationControl(plateConfiguration));
                }
            }
        }

        public static void Attach(PressScheduleWindow window)
        {
        }

        public static void Load(string fileName,PressScheduleWindow window)
        {

            try
            {
                using (BinaryReader reader = new BinaryReader(new FileStream(fileName, FileMode.Open)))
                {
                   
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Load failed.");
            }
        }

        private void SettingsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            PressSettingsWindow window = new PressSettingsWindow();

            window.ShowDialog();
        }

        

        private void ExportMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            // make this a thread
            // show message that export is happening
            //ExportToExcel();

        }

        //private void ExportToExcel()
        //{
        //    try
        //    {
        //        //Start Excel and get Application object.
        //        var oXL = new Microsoft.Office.Interop.Excel.Application();
        //        oXL.Visible = true;
        //        oXL.UserControl = false;
        //        oXL.StandardFont = "Arial";

        //        _Worksheet oSheet;
        //        Range oRng;

        //        //Get a new workbook.
        //        _Workbook oWB = oXL.Workbooks.Add(Missing.Value);


        //        oSheet = (_Worksheet)oWB.ActiveSheet;

        //        // fit to single page
        //        var page = oSheet.PageSetup;
        //        page.FitToPagesWide = 1;

        //        // format top rows
        //        oRng = oSheet.Range["A1", StaticFunctions.GetRangeIndex(8, 1)];
        //        oRng.RowHeight = 14.5;
        //        oRng = oSheet.Range[StaticFunctions.GetRangeIndex(1, 2), StaticFunctions.GetRangeIndex(8, 2)];
        //        oRng.RowHeight = 9;
                
	       //     oRng = oSheet.Range[StaticFunctions.GetRangeIndex(1, 3),StaticFunctions.GetRangeIndex(9,3)];
        //        oRng.Merge();
        //        oRng.RowHeight = 38.25;

        //        oSheet.Range["A4"].RowHeight = 9;
        //        oSheet.Range["A5"].RowHeight = 26.25;
        //        oSheet.Range["A6"].RowHeight = 17.25;

        //        // Set title as Arial Black
        //        StaticFunctions.SaveRichTextToCell(oRng, "Production Schedule & Plate Mix", PublicEnums.FontWeight.Bold, 25);
        //        oRng.Font.Name = @"Arial Black";
        //        oRng.HorizontalAlignment = XlHAlign.xlHAlignCenter;

        //        // date of modification
        //        oRng = oSheet.Range["I5"];
        //        StaticFunctions.SaveRichTextToCell(oRng, DateTime.Today.ToString("d"),PublicEnums.FontWeight.Bold,20,Color.Blue);
        //        oRng.Font.Underline = true;
        //        oRng.HorizontalAlignment = XlHAlign.xlHAlignRight;

        //        Int32 current_row = 5;

        //        foreach (PressWeekControl weekControl in WeekControls)
        //        {
        //            //       row+2
        //            current_row += 2;

        //            //       [D7 rA]Plate mix for:  	[E7 lA]Trim Stock			// row+2
        //            oRng = oSheet.Range["D" + current_row];
        //            oRng.RowHeight = 36.75;
        //            StaticFunctions.SaveRichTextToCell(oRng, "Plate mix for:", PublicEnums.FontWeight.Bold, 24);
        //            oRng.Font.Name = "Arial Black";
        //            oRng.HorizontalAlignment = XlHAlign.xlHAlignRight;
        //            oRng.Font.Underline = true;

        //            String targetProducts = String.Empty;
        //            oRng = oSheet.Range["E" + current_row];
        //            targetProducts = weekControl.ControlsList.Aggregate(targetProducts,
        //                (current, itemControl) => current + itemControl.ItemName);
        //            StaticFunctions.SaveRichTextToCell(oRng, targetProducts, PublicEnums.FontWeight.Bold, 24, Color.Red);
        //            oRng.Font.Name = "Arial Black";
        //            oRng.Font.Underline = true;

        //            oSheet.Range["A" + (current_row + 1)].RowHeight = 9;

        //            current_row += 2;
        //            //  [B9 lA]Monday 9/29/14						// row+2
        //            oRng = oSheet.Range["B" + current_row];
        //            oRng.RowHeight = 33.75;
        //            oSheet.Range["A" + (current_row + 1)].RowHeight = 12.75;

        //            StaticFunctions.SaveRichTextToCell(oRng,weekControl.WeekTitle,PublicEnums.FontWeight.Bold,26,Color.Blue);
        //            oRng.Font.Underline = true;

        //            current_row += 2;
        //            //  [D11 rA]15 -	[E11 lA]Old Mill			// row+2
        //            //  [D13 rA]15 -	[E13 lA]Smooth			// row+2
        //            foreach (var mixControl in weekControl.MixControls)
        //            {
        //                oRng = oSheet.Range["D" + current_row];
        //                oRng.RowHeight = 26.25;
        //                StaticFunctions.SaveRichTextToCell(oRng,mixControl.NumChanges + " - ", PublicEnums.FontWeight.Bold, 20,Color.Blue);
        //                oRng.HorizontalAlignment = XlHAlign.xlHAlignRight;

        //                oRng = oSheet.Range["E" + current_row];
        //                StaticFunctions.SaveRichTextToCell(oRng, mixControl.Tex.Name, PublicEnums.FontWeight.Bold, 20);

        //                current_row ++;
        //            }

        //            oSheet.Range["A" + current_row].RowHeight = 26.25;
        //            oSheet.Range["A" + (current_row + 1)].RowHeight = 8.25;

        //            current_row += 2;
						
        //            //  [B15 lA]Press Schedule:						// row+1
        //            oRng = oSheet.Range["B" + current_row];
        //            oRng.RowHeight = 33.75;
        //            StaticFunctions.SaveRichTextToCell(oRng, "Press Schedule:", PublicEnums.FontWeight.Bold, 22);
        //            oRng.Font.Underline = true;
        //            current_row ++;
                    
        //            //  [C16 lA]Please start the Production-Line on:	// row+1				
        //            oRng = oSheet.Range["C" + current_row];
        //            oRng.RowHeight = 26.25;
        //            StaticFunctions.SaveRichTextToCell(oRng,"Please start the Production line on: ", PublicEnums.FontWeight.None,20);
        //            current_row++;

        //            //  [C17 lA]7/16"	[D17 lA]Thickness				// done
        //            oRng = oSheet.Range["C" + current_row];
        //            oRng.RowHeight = 32.25;
        //            String startThick = weekControl.ControlsList.Count > 0 ? weekControl.ControlsList[0].Thickness : "";
        //            StaticFunctions.SaveRichTextToCell(oRng,startThick,PublicEnums.FontWeight.Bold,20,Color.Blue);
        //            oRng.HorizontalAlignment = XlHAlign.xlHAlignRight;
        //            oRng.Font.Name = "Arial Black";

        //            oRng = oSheet.Range["D" + current_row];
        //            StaticFunctions.SaveRichTextToCell(oRng,"Thickness",PublicEnums.FontWeight.Bold,20);
        //            oRng.Font.Name = "Arial Black";

        //            //

        //        }


        //        SetColumnWidths(oSheet);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //    }

        //}

        private void SetColumnWidths(_Worksheet oSheet)
        {
            double[] widths = {3.43, 12.86, 14.29, 13, 8.43, 8.43, 8.43, 11.86, 31.14};

            for (Int32 index = 0; index < widths.Length; index++)
            {
                Range rng = oSheet.Range[StaticFunctions.GetRangeIndex(index + 1, 1)];
                rng.ColumnWidth = widths[index];
            }
        }
    }
}
