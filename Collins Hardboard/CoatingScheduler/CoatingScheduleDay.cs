using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Configuration_windows;
using Microsoft.Office.Interop.Excel;
using StaticHelpers;

namespace CoatingScheduler
{
    public class CoatingScheduleDay : ICoatingScheduleLogic
    {
       
        public DateTime Date { get; set; }


        public CoatingScheduleDay()
        {
            
        }

        private CoatingScheduleDay(DateTime date, string coatingLine, ObservableCollection<ICoatingScheduleLogic> children = null)
        {
            Date = date;
            CoatingLine = coatingLine;
            if (children != null)
            {
                ChildrenLogic = children;
            }
            else if (ChildrenLogic == null)
            {
                ChildrenLogic = new ObservableCollection<ICoatingScheduleLogic>();
            }
        }


        public override void ReconnectToControls()
        {
            if (Control != null)
            {
                foreach (var logic in ChildrenLogic)
                {
                    if (logic.Control == null)
                    {
                        Control.AddControlToBottom(logic);
                        logic.Connect(this);
                        logic.ReconnectToControls();
                    }
                }
            }

        }

        public static CoatingScheduleDay Load(BinaryReader reader)
        {
            string dateString = reader.ReadString();
            DateTime date;
            DateTime.TryParse(dateString, out date);
            string coatingLine = reader.ReadString();

            ObservableCollection<ICoatingScheduleLogic> children = new ObservableCollection<ICoatingScheduleLogic>();

            int numChildren = reader.ReadInt32();
            for (; numChildren > 0; --numChildren)
            {
                children.Add(CoatingScheduleLine.Load(reader));
            }
            
            return new CoatingScheduleDay(date,coatingLine,children);
        }


        public override void Save(BinaryWriter writer)
        {
            writer.Write(Date.ToString());
            writer.Write(CoatingLine);
            writer.Write(ChildrenLogic.Count);
            foreach (var logic in ChildrenLogic)
            {
                logic.Save(writer);
            }
        }

        public override Tuple<int, int> ExportToExcel(_Worksheet sheet, int column, int row)
        {
            Int32 nextRow = row;
            Int32 currentColumn = column;

            // output all child stuff
            currentColumn = column + 2;
            foreach (var logic in ChildrenLogic)
            {
                Tuple<int, int> nextPlace = logic.ExportToExcel(sheet, column + 2, nextRow);
                nextRow = nextPlace.Item1;
                currentColumn = nextPlace.Item2;
            }

            // output borders
            Borders borders = sheet.Range[StaticFunctions.GetRangeIndex(column, row), StaticFunctions.GetRangeIndex(currentColumn - 1, nextRow - 1)].Borders;

            borders.Item[XlBordersIndex.xlEdgeRight].Weight = XlBorderWeight.xlMedium;
            borders.Item[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
            borders.Item[XlBordersIndex.xlEdgeBottom].Weight = XlBorderWeight.xlMedium;
            borders.Item[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
            borders.Item[XlBordersIndex.xlEdgeTop].Weight = XlBorderWeight.xlMedium;

            //output date
            Range range = sheet.Range[StaticFunctions.GetRangeIndex(column, row), StaticFunctions.GetRangeIndex(column, nextRow - 1)];
            range.ColumnWidth = 3;
            range.Merge();
            range.Borders.Item[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
            range.Borders.Item[XlBordersIndex.xlEdgeLeft].Weight = XlBorderWeight.xlMedium;
            range.VerticalAlignment = XlVAlign.xlVAlignCenter;
            range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            range.Orientation = 90;
            StaticFunctions.SaveRichTextToCell(range, Date.ToString("dddd"));

            range = sheet.Range[StaticFunctions.GetRangeIndex(column + 1, row), StaticFunctions.GetRangeIndex(column + 1, nextRow - 1)];
            range.ColumnWidth = 3;
            range.Merge();
            range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            range.VerticalAlignment = XlVAlign.xlVAlignCenter;
            range.Orientation = 90;
            StaticFunctions.SaveRichTextToCell(range, Date.ToString("MM/dd"));

            return new Tuple<int, int>(nextRow, currentColumn);

        }

        public override bool IsFull()
        {
            if (ChildrenLogic.Count < ShiftHandler.CoatingInstance.Shifts.Count) return false;

            return ChildrenLogic.All(logic => logic.IsFull());
        }

        public override void AddLogic(ICoatingScheduleLogic newController = null)
        {

            CoatingScheduleLine newLogic;
            if (newController != null)
            {
                newLogic = (CoatingScheduleLine)newController;
                newLogic.Connect(this);
                if (Control != null)
                    Control.AddControlToBottom(newLogic);
            }
            else
            {
                newLogic = new CoatingScheduleLine();
                newLogic.Shift = GetNextShift();
                newLogic.Connect(this);
                if (Control != null)
                    Control.AddControlToBottom(newLogic);
                newLogic.AddLogic();
            }
            ChildrenLogic.Add(newLogic);
        }
        public override void AddControlToBottom(ICoatingScheduleLogic upChild)
        {
            if (upChild.GetType() == typeof(CoatingScheduleLine)) // if child type, add to list
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
        }

        public override void SwapDown()
        {
            ParentLogic.SwapChildDown(this,null);
        }

        public override void SwapChildDown(ICoatingScheduleLogic downChild, ICoatingScheduleLogic lastParent)
        {
            if (lastParent.GetType() == typeof(CoatingScheduleLine))
            {
                Int32 indexOfParent = ChildrenLogic.IndexOf((CoatingScheduleLine)lastParent);

                if (indexOfParent < ChildrenLogic.Count - 1) // if not at the bottom
                {
                    lastParent.RemoveLogic(downChild);
                    ChildrenLogic[indexOfParent + 1].AddControlToTop(downChild);
                }
                else if (indexOfParent == ChildrenLogic.Count - 1)
                {
                    ParentLogic.SwapChildDown(downChild, this);
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

        public override void AddControlToTop(ICoatingScheduleLogic newController)
        {
            CoatingScheduleLine newLogic;
            if (newController != null)
            {
                if (newController.GetType() == typeof (CoatingScheduleLine))
                {
                    newLogic = (CoatingScheduleLine) newController;

                    newLogic.Shift = GetNextShift();
                    ChildrenLogic.Add(newLogic);
                    newLogic.Connect(this);
                    Control.AddControlToTop(newLogic);
                    newLogic.AddLogic();
                }
                else if(ChildrenLogic.Count > 0)
                {
                    ChildrenLogic[0].AddControlToTop(newController);
                }
                else
                {
                    newLogic = new CoatingScheduleLine();

                    newLogic.Date = Date;
                    newLogic.Shift = GetNextShift();
                    ChildrenLogic.Add(newLogic);
                    newLogic.Connect(this);
                    Control.AddControlToTop(newLogic);
                    newLogic.AddLogic();
                    
                    newLogic.AddControlToTop(newController);
                }
            }
            else
            {
                newLogic = new CoatingScheduleLine();
                
                newLogic.Date = Date;
                newLogic.Shift = GetNextShift();
                ChildrenLogic.Add(newLogic);
                newLogic.Connect(this);
                Control.AddControlToTop(newLogic);
                newLogic.AddLogic();
            }
        }

        public override void PushDown()
        {
            ParentLogic.PushChildDown(this,null);
        }

        public override void PushChildDown(ICoatingScheduleLogic upChild, ICoatingScheduleLogic lastParent)
        {
            ParentLogic.PushChildDown(upChild,this);
        }

        public override void PushDownChildren(ICoatingScheduleLogic upChild)
        {
            Int32 lineCount = ChildrenLogic.Count;
            Int32 foundIndex = IndexOfChild(upChild);
            Int32 startIndex = foundIndex >= 0 ? foundIndex : 0;
            for (Int32 index = startIndex; index < lineCount; index++)
            {
                var line = ChildrenLogic[index];
                line.PushDownChildren(upChild);
            }
        }

        public override void PushUp()
        {
            ParentLogic.PushChildUp(this,null);
        }

        public override void PushChildUp(ICoatingScheduleLogic upChild, ICoatingScheduleLogic lastParent)
        {
            ParentLogic.PushChildUp(upChild,this);
        }

        public override void PushUpChildren(ICoatingScheduleLogic upChild)
        {
            Int32 lineCount = ChildrenLogic.Count;
            for (Int32 index = 0; index < lineCount; index++)
            {
                var line = ChildrenLogic[index];
                line.PushUpChildren(upChild);
            }
        }


        private Shift GetNextShift()
        {
            Shift nextShift = null;

            if (ChildrenLogic.Count == 0)
            {
                if (ShiftHandler.CoatingInstance.Shifts.Count == 0)
                {
                    var result = MessageBox.Show("No shift schedule loaded. Try loading now?", "",
                        MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        bool loaded = ShiftHandler.CoatingInstance.LoadShifts();
                        if (loaded)
                        {
                            MessageBox.Show("Shifts loaded successfully.");
                            nextShift = ShiftHandler.CoatingInstance.Shifts.FirstOrDefault(x => x.StartDate <= Date && x.EndDate >= Date && x.DaysList.Contains(Date.DayOfWeek));
                        }
                        else
                        {
                            MessageBox.Show("Shifts could not be loaded.");
                        }
                    }
                }
                else
                    nextShift = ShiftHandler.CoatingInstance.Shifts[0];
            }
            else
            {
                    foreach (var shift in ShiftHandler.CoatingInstance.Shifts)
                    {
                        bool hasShift = false;
                        for (Int32 index = 0; !hasShift && index < ChildrenLogic.Count; index++)
                        {
                            CoatingScheduleLine line = ChildrenLogic[index] as CoatingScheduleLine;
                            if (line != null && line.ShiftName == shift.Name)
                                hasShift = true;
                        }
                        if (!hasShift && (shift.StartDate <= Date && shift.EndDate >= Date &&
                                          shift.DaysList.Contains(Date.DayOfWeek)))
                        {
                            nextShift = shift;
                            break;
                        }
                    }
                
            
            }

            //if (ChildrenLogic.Count == 0)
            //    nextTitle = StaticFactoryValuesManager.Shifts.FirstOrDefault();
            //else
            //{
            //    try
            //    {
            //        foreach (string shift in StaticFactoryValuesManager.Shifts.Reverse())
            //        {
            //            bool hasShift = false;
            //            for (Int32 index = 0; !hasShift && index < ChildrenLogic.Count; index++)
            //            {
            //                CoatingScheduleLine line = ChildrenLogic[index] as CoatingScheduleLine;
            //                if (line.ShiftName == shift)
            //                    hasShift = true;
            //            }
            //            if (hasShift) continue;
            //            nextTitle = shift;
            //            break;
            //        }
            //    }
            //    catch (Exception)
            //    {

            //    }
            //}
            return nextShift;
        }

        public override void Connect(ICoatingScheduleLogic logic)
        {
            ParentLogic = logic;
        }

        public override void RemoveLogic(ICoatingScheduleLogic child)
        {
            if (child.GetType() == typeof(CoatingScheduleLine))
            {
                ChildrenLogic.Remove((CoatingScheduleLine)child);
                Control.RemoveControl(child.Control);
            }
            else
            {
                foreach (var logic in ChildrenLogic)
                {
                    logic.RemoveLogic(child);
                }
            }
        }

        public override void DestroySelf()
        {
            for (Int32 index = 0; index < ChildrenLogic.Count; index++)
            {
                ChildrenLogic[index].Disconnect();
            }
            ParentLogic.RemoveLogic(this);
            ChildrenLogic.Clear();
            Disconnect();
        }

        public override bool ChildIsTop(ICoatingScheduleLogic child)
        {
            bool isTop = false;
            if (child.GetType() == typeof(CoatingScheduleLine) && ChildrenLogic.Count > 0)
            {
                isTop = ChildrenLogic.First() == (CoatingScheduleLine)child;
                if (isTop)
                    isTop = ParentLogic.ChildIsTop(this);
            }
            else
            {
                throw new Exception("Child is not contained in this class.");
            }
            return isTop;
        }

        public override void SwapUp()
        {
            throw new NotImplementedException();
        }

        public override void SwapChildUp(ICoatingScheduleLogic upChild, ICoatingScheduleLogic lastParent)
        {
            if (lastParent.GetType() == typeof (CoatingScheduleLine))
            {
                Int32 indexOfParent = ChildrenLogic.IndexOf((CoatingScheduleLine) lastParent);

                if (indexOfParent > 0) // if not at the top
                {
                    lastParent.RemoveLogic(upChild);
                    ChildrenLogic[indexOfParent -1].AddControlToBottom(upChild);
                }
                else if (indexOfParent == 0)
                {
                    ParentLogic.SwapChildUp(upChild,this);
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

        public bool CanAddShift()
        {
            int shiftsRanToday = ShiftHandler.CoatingInstance.ShiftsRan(Date);

            return shiftsRanToday > ChildrenLogic.Count;
        }
    }
}
