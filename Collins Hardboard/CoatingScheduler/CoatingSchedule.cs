using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Configuration_windows;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using StaticHelpers;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace CoatingScheduler
{
    public class CoatingSchedule : ICoatingScheduleLogic
    {
        private const string datFile = "DefaultSchedule.sch";

        public static String DefaultExtension
        {
            get { return ".sch"; }
        }

        public static CoatingSchedule DefaultSchedule;
    

            private String _dateRange = String.Empty;

        private ObservableCollection<CoatingLineInstructionSet> _instructionSets =
            new ObservableCollection<CoatingLineInstructionSet>();

        public CoatingSchedule()
        {
            foreach (string line in StaticFactoryValuesManager.CoatingLines)
            {
                InstructionSets.Add(new CoatingLineInstructionSet(line));
            }
        }

        private CoatingSchedule(string dateRange, ObservableCollection<CoatingLineInstructionSet> instructions, ObservableCollection<ICoatingScheduleLogic> days)
        {
            DateRange = dateRange;
            InstructionSets = instructions;
            ChildrenLogic = days;

            if (InstructionSets.Count == 0)
            {
                foreach (string line in StaticFactoryValuesManager.CoatingLines)
                {
                    InstructionSets.Add(new CoatingLineInstructionSet(line));
                }
            }
        }

        public ObservableCollection<CoatingLineInstructionSet> InstructionSets
        {
            get { return _instructionSets; }
            set { _instructionSets = value; }
        }

        public string DateRange
        {
            get { return _dateRange; }
            set { _dateRange = value; }
        }

        public void RemoveDay(CoatingScheduleDay day)
        {
            ChildrenLogic.Remove(day);
            UpdateDateText();
        }

        public void UpdateDateText()
        {
            if (ChildrenLogic.Count == 0)
            {
                DateRange = "-- through --";
                ((CoatingScheduleWindow) Control).UpdateDateRange(DateRange);
            }
            else if (ChildrenLogic.Count == 1)
            {
                DateTime dateTime = ((CoatingScheduleDay) ChildrenLogic[0]).Date;
                DateRange = String.Format("{0} through {0}",
                    dateTime.ToString("ddd, MMM d"));
                ((CoatingScheduleWindow) Control).UpdateDateRange(DateRange);
            }
            else
            {
                DateTime dateTimeMax = DateTime.MinValue;
                DateTime dateTimeMin = DateTime.MaxValue;
                foreach (CoatingScheduleDay logic in ChildrenLogic)
                {
                    if (logic.Date > dateTimeMax)
                        dateTimeMax = logic.Date;
                    if (logic.Date < dateTimeMin)
                        dateTimeMin = logic.Date;
                }
                DateRange = String.Format("{0} through {1}",
                    dateTimeMin.ToString("ddd, MMM d"), dateTimeMax.ToString("ddd, MMM d"));

                ((CoatingScheduleWindow) Control).UpdateDateRange(DateRange);
            }
        }

        public override void AddControlToBottom(ICoatingScheduleLogic upChild)
        {
            if (upChild.GetType() == typeof (CoatingScheduleDay)) // if child type, add to list
            {
                AddLogic(upChild);
            }
            else if (ChildrenLogic.Count > 0) // if not child type, but this logic has children, try to have them add
            {
                ChildrenLogic.Last().AddControlToBottom(upChild);
            }
            else // if no children, create one and add new control to bottom
            {
                AddLogic();
                ChildrenLogic[0].AddControlToBottom(upChild);
            }
            UpdateDateText();
        }

        public override void SwapDown()
        {
            throw new NotImplementedException();
        }

        public override void SwapChildDown(ICoatingScheduleLogic downChild, ICoatingScheduleLogic lastParent)
        {
            if (lastParent.GetType() == typeof (CoatingScheduleDay))
            {
                Int32 indexOfParent = ChildrenLogic.IndexOf((CoatingScheduleDay) lastParent);

                if (indexOfParent < ChildrenLogic.Count - 1) // if not at the bottom
                {
                    lastParent.RemoveLogic(downChild);
                    ChildrenLogic[indexOfParent + 1].AddControlToTop(downChild);
                }
                else if (indexOfParent == ChildrenLogic.Count - 1)
                {
                    //must add back the child
                    var day = ChildrenLogic.Last();
                    var line = day.ChildrenLogic.Last();
                    var shift = line.ChildrenLogic.First(x => x.CoatingLine == downChild.CoatingLine);
                    shift.AddControlToBottom(downChild);
                }
                else
                {
                    throw new Exception("Invoking object not a member of current object");
                }
            }
            else
            {
                throw new Exception("Call received by a non-child control.");
            }
        }

        public override void AddControlToTop(ICoatingScheduleLogic newLogic)
        {
            if (newLogic.GetType() == typeof (CoatingScheduleDay)) // if child type, add to list
            {
                AddControlToTop(newLogic);
            }
            else if (ChildrenLogic.Count > 0) // if not child type, but this logic has children, try to have them add
            {
                ChildrenLogic.Last().AddControlToTop(newLogic);
            }
            else // if no children, create one and add new control to bottom
            {
                AddLogic();
                ChildrenLogic[0].AddControlToTop(newLogic);
            }
            UpdateDateText();
        }

        public override void PushDown()
        {
            ParentLogic.PushChildDown(this, null);
        }

        public override void PushChildDown(ICoatingScheduleLogic upChild, ICoatingScheduleLogic lastParent)
        {
            if (lastParent.GetType() == typeof (CoatingScheduleDay))
            {
                Int32 indexOfParent = ChildrenLogic.IndexOf((CoatingScheduleDay) lastParent);
                Int32 dayCount = ChildrenLogic.Count;

                for (Int32 i = dayCount - 1; i >= indexOfParent; --i)
                {
                    ChildrenLogic[i].PushDownChildren(upChild);
                }
            }
        }

        public override void PushDownChildren(ICoatingScheduleLogic upChild)
        {
            throw new NotImplementedException();
        }

        public override void PushUp()
        {
            ParentLogic.PushChildUp(this, null);
        }

        public override void PushChildUp(ICoatingScheduleLogic upChild, ICoatingScheduleLogic lastParent)
        {
            if (lastParent.GetType() == typeof (CoatingScheduleDay))
            {
                Int32 indexOfParent = ChildrenLogic.IndexOf((CoatingScheduleDay) lastParent);

                for (Int32 i = indexOfParent; i >= 0; --i)
                {
                    ChildrenLogic[i].PushUpChildren(upChild);
                }
            }
        }

        public override void PushUpChildren(ICoatingScheduleLogic upChild)
        {
        }

        public override void ReconnectToControls()
        {
            if (Control != null)
            {
                ((CoatingScheduleWindow)Control).LoadInstructions(InstructionSets);
                foreach (var logic in ChildrenLogic)
                {
                    Control.AddControlToBottom(logic);
                    logic.Connect(this);
                    logic.ReconnectToControls();
                }
            }
        }

        public static CoatingSchedule Load(BinaryReader reader)
        {
            string dateRange = reader.ReadString();

            ObservableCollection<CoatingLineInstructionSet> instructions = new ObservableCollection<CoatingLineInstructionSet>();
            int numInstructions = reader.ReadInt32();
            for (; numInstructions > 0; --numInstructions)
            {
                instructions.Add(CoatingLineInstructionSet.Load(reader));
            }

            ObservableCollection<ICoatingScheduleLogic> days = new ObservableCollection<ICoatingScheduleLogic>();
            int numDays = reader.ReadInt32();
            for (; numDays > 0; --numDays)
            {
                days.Add(CoatingScheduleDay.Load(reader));
            }

            var schedule = new CoatingSchedule(dateRange, instructions, days);

            return schedule;
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(DateRange);

            writer.Write(InstructionSets.Count);
            foreach (var instructionSet in InstructionSets)
            {
                instructionSet.Save(writer);
            }

            writer.Write(ChildrenLogic.Count);
            foreach (var logic in ChildrenLogic)
            {
                logic.Save(writer);
            }
        }

        public override Tuple<int, int> ExportToExcel(_Worksheet sheet, int column, int row)
        {
            Int32 nextRow = row;

            // Write out header
            Range range = sheet.Range[StaticFunctions.GetRangeIndex(column, nextRow)];
            StaticFunctions.SaveRichTextToCell(range, "Coating Schedule", PublicEnums.FontWeight.Bold, 20);

            range = sheet.Range[StaticFunctions.GetRangeIndex(column + 5, nextRow)];
            StaticFunctions.SaveRichTextToCell(range, DateRange, PublicEnums.FontWeight.Bold, 20, Color.Blue);

            range = sheet.Range[StaticFunctions.GetRangeIndex(column, nextRow), StaticFunctions.GetRangeIndex(column + 10, nextRow)];
            Borders borders = range.Borders;
            borders.Item[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlDouble;
            ++nextRow;

            // Display headers for instructions
            Int32 instructionColumn = column + 2;
            Int32 instructionRow = nextRow;

            // Display instructions
            foreach (var instruction in InstructionSets)
            {
                Tuple<int, int> nextPlace = instruction.ExportToExcel(sheet, instructionColumn, instructionRow);
                nextRow = nextPlace.Item1 > nextRow ? nextPlace.Item1 : nextRow;
                instructionColumn = nextPlace.Item2 + 1;
            }

            //Write out the column headers.
            Int32 lineColumn = column + 4;
            range = sheet.Range[StaticFunctions.GetRangeIndex(column, nextRow)];
            StaticFunctions.SaveRichTextToCell(range, "Date", PublicEnums.FontWeight.Bold, 10);
            for (Int32 index = 0; index < StaticFactoryValuesManager.CoatingLines.Count; index++)
            {
                string coatingLine = StaticFactoryValuesManager.CoatingLines[index];
                range = sheet.Range[StaticFunctions.GetRangeIndex(lineColumn, nextRow)];
                StaticFunctions.SaveRichTextToCell(range, coatingLine, PublicEnums.FontWeight.Bold, 14);

                lineColumn += 1;
                range = sheet.Range[StaticFunctions.GetRangeIndex(lineColumn, nextRow)];
                StaticFunctions.SaveRichTextToCell(range, "Grade", PublicEnums.FontWeight.None, 8);
                lineColumn += 1;
                range = sheet.Range[StaticFunctions.GetRangeIndex(lineColumn, nextRow)];
                StaticFunctions.SaveRichTextToCell(range, "BarCode", PublicEnums.FontWeight.None, 6);
                lineColumn += 2;
            }
            ++nextRow;

            foreach (var day in ChildrenLogic)
            {
                Tuple<int, int> nextPlace = day.ExportToExcel(sheet, column, nextRow);
                nextRow = nextPlace.Item1;
            }

            // fit page for printing
            PageSetup page = sheet.PageSetup;
            page.Zoom = false;
            page.FitToPagesWide = 1;
            page.FitToPagesTall = 1;
            page.Orientation = XlPageOrientation.xlPortrait;
            page.PaperSize = XlPaperSize.xlPaperA4;

            return new Tuple<int, int>(0, 0);

        }

        public override bool IsFull()
        {
            return ChildrenLogic.All(logic => logic.IsFull());
        }

        public override void AddLogic(ICoatingScheduleLogic newController = null)
        {
            CoatingScheduleDay newLogic;
            if (newController != null)
            {
                newLogic = (CoatingScheduleDay) newController;
            }
            else
            {
                newLogic = new CoatingScheduleDay();
                newLogic.Date = GetNextDay();
            }
            ChildrenLogic.Add(newLogic);
            newLogic.Connect(this);
            if (Control != null)
                Control.AddControlToBottom(newLogic);
            UpdateDateText();
        }

        public void ExportToExcel()
        {
            try
            {
                Exporting = true;
                //Start Excel and get Application object.
                var oXL = new Application();
                oXL.Visible = true;
                oXL.UserControl = false;

                _Worksheet oSheet;
                Range oRng;

                //Get a new workbook.
                _Workbook oWB = oXL.Workbooks.Add(Missing.Value);
                oSheet = (_Worksheet) oWB.ActiveSheet;

                // create caretaker to generate excel
                ExportToExcel(oSheet, 1, 1);

                Exporting = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Exporting = false;
            }
        }

        public bool Exporting { get; set; }

        public DateTime GetNextDay()
        {
            if (ChildrenLogic.Count == 0) // if nothing to base off of, just start today
                return DateTime.Today;

            DateTime nextDay = ChildrenLogic.Max(day => ((CoatingScheduleDay) day).Date);

            return ShiftHandler.CoatingInstance.GetNextWorkingDay(nextDay);
        }

        public override void Connect(ICoatingScheduleLogic logic)
        {
            ParentLogic = logic;
        }

        public override void RemoveLogic(ICoatingScheduleLogic child)
        {
            ChildrenLogic.Remove((CoatingScheduleDay) child);
            Control.RemoveControl(child.Control);
            UpdateDateText();
        }

        public override void DestroySelf()
        {
            for (Int32 index = 0; index < ChildrenLogic.Count; index++)
            {
                ChildrenLogic[index].Disconnect();
            }
            ChildrenLogic.Clear();
            Disconnect();
        }

        public override bool ChildIsTop(ICoatingScheduleLogic child)
        {
            bool isTop = false;
            if (child.GetType() == typeof (CoatingScheduleDay) && ChildrenLogic.Count > 0)
            {
                isTop = ChildrenLogic.First() == (CoatingScheduleDay) child;
            }
            else
            {
                throw new Exception("Child not contained within class.");
            }
            return isTop;
        }

        public override void SwapUp()
        {
            throw new NotImplementedException();
        }

        public override void SwapChildUp(ICoatingScheduleLogic upChild, ICoatingScheduleLogic lastParent)
        {
            if (lastParent.GetType() == typeof (CoatingScheduleDay))
            {
                Int32 indexOfParent = ChildrenLogic.IndexOf((CoatingScheduleDay) lastParent);

                if (indexOfParent > 0) // if not at the top
                {
                    lastParent.RemoveLogic(upChild);
                    ChildrenLogic[indexOfParent - 1].AddControlToBottom(upChild);
                }
                else if (indexOfParent == 0)
                {
                    //must add back the child
                    ICoatingScheduleLogic child = ChildrenLogic[0];
                    ICoatingScheduleLogic child2 = child.ChildrenLogic[0];
                    ICoatingScheduleLogic child3 = child2.ChildrenLogic.First(x => x.CoatingLine == upChild.CoatingLine);
                    child3.AddLogic(upChild);
                }
                else
                {
                    throw new Exception("Invoking object not a member of current object");
                }
            }
            else
            {
                throw new Exception("Call received by a non-child control.");
            }
        }

        public void Save()
        {
            if (MessageBox.Show("Save as default?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Save(datFile);
            }
            else
            {

            SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Save Coating schedule";
                saveFileDialog.DefaultExt = DefaultExtension;

                if (saveFileDialog.ShowDialog() == true)
                {
                    Save(saveFileDialog.FileName);
                }
            }
        }

        public void Save(string file)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(new FileStream(file,FileMode.OpenOrCreate)))
                {
                    Save(writer);
                }
            }
            catch (Exception)
            {
                
            }
        }

        public static CoatingSchedule LoadSchedule()
        {
            CoatingSchedule schedule = null;
            if (MessageBox.Show("Load default schedule?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                schedule = Load(datFile);

                if (schedule == null)
                {
                    MessageBox.Show("Failed to load default. Please select a backup.");

                }
            }
            if (schedule == null)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.DefaultExt = DefaultExtension;
                openFileDialog.Title = "Open schedule file";
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == true)
                {
                    schedule = Load(openFileDialog.FileName);
                }
            }

            return schedule;
        }

        public static CoatingSchedule Load(string file = "")
        {
            if (file == "")
                file = datFile;

            CoatingSchedule schedule = null;
            try
            {
                using (BinaryReader reader = new BinaryReader(new FileStream(file,FileMode.Open)))
                {
                    schedule = Load(reader);
                }
            }
            catch (Exception exception)
            {
                
            }

            return schedule;
        }

        public void Clear()
        {
            Control.DestroySelf();
            DestroySelf();
        }
    }
}