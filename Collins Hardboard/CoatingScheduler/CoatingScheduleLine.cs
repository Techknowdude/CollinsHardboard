using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using CoatingScheduler.Annotations;
using Configuration_windows;
using Microsoft.Office.Interop.Excel;
using ModelLib;
using StaticHelpers;

namespace CoatingScheduler
{
    public class CoatingScheduleLine : ICoatingScheduleLogic, INotifyPropertyChanged
    {
        #region Fields
        private String _shiftName;
        private Shift _shift;
        private DateTime _date;

        #endregion

        #region Properties
        public String ShiftName
        {
            get { return _shiftName; }
            set
            {
                // if first time being created through binary load
                if (string.IsNullOrEmpty(_shiftName))
                {
                    _shift = ShiftHandler.CoatingInstance.Shifts.FirstOrDefault(x => x.Name == value &&
                        x.StartDate <= Date && x.EndDate >= Date && x.DaysList.Contains(Date.DayOfWeek));
                }
                _shiftName = value;
                //NotifyPropertyChanged();
            }
        }

        public Shift Shift
        {
            get
            {
                if (_shift == null && Date != null)
                {
                    _shift = ShiftHandler.CoatingInstance.Shifts.FirstOrDefault(x => x.Name == ShiftName &&
                        x.StartDate <= Date && x.EndDate >= Date && x.DaysList.Contains(Date.DayOfWeek));
                }
                return _shift;
            }
            set
            {
                if(value != null)
                {
                    ShiftName = value.Name;
                }
                _shift = value;
            }
        }

        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }

        #endregion


        public CoatingScheduleLine()
        {

        }
        public CoatingScheduleLine(List<CoatingScheduleShift> shifts)
        {
            foreach (CoatingScheduleShift shift in shifts)
            {
                ChildrenLogic.Add(shift);
                shift.Connect(this);
                if (Control != null)
                    Control.AddControlToBottom(shift);
            }
        }

        private CoatingScheduleLine(ObservableCollection<ICoatingScheduleLogic> shifts, string name, string coatingLine, DateTime date, Shift shift)
        {
            if(shifts != null)
                ChildrenLogic = shifts;
            else if(ChildrenLogic == null)
                ChildrenLogic = new ObservableCollection<ICoatingScheduleLogic>();

            ShiftName = name;
            CoatingLine = coatingLine;
            Date = date;
            Shift = shift;
        }

        public void AddShifts(List<CoatingScheduleShift> shifts)
        {
            foreach (CoatingScheduleShift shift in shifts)
            {
                ChildrenLogic.Add(shift);
                shift.Connect(this);
                if (Control != null)
                    Control.AddControlToBottom(shift);
            }
        }

        public void Remove()
        {
            
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

        public override void Save(BinaryWriter writer)
        {
            writer.Write(ShiftName);
            writer.Write(CoatingLine);
            writer.Write(Date.ToString());
            Shift.Save(writer);
            writer.Write(ChildrenLogic.Count);
            foreach (var logic in ChildrenLogic)
            {
                logic.Save(writer);
            }
        }

        public static CoatingScheduleLine Load(BinaryReader reader)
        {
            string name = reader.ReadString();
            string coatingLine = reader.ReadString();
            string datestring = reader.ReadString();
            DateTime date;
            DateTime.TryParse(datestring, out date);
            Shift shift = Shift.Load(reader,true);
            ObservableCollection<ICoatingScheduleLogic> shifts = new ObservableCollection<ICoatingScheduleLogic>();

            int numShifts = reader.ReadInt32();
            for (; numShifts > 0; --numShifts)
            {
                shifts.Add(CoatingScheduleShift.Load(reader));
            }

            return new CoatingScheduleLine(shifts,name,coatingLine,date,shift);
        }


        public override Tuple<int, int> ExportToExcel(_Worksheet sheet, int column, int row)
        {
            Int32 nextRow = row;
            Int32 prevRow = row;
            Int32 currentCollumn = column + 1;

            // set width of columns
            Range range = sheet.Range[StaticFunctions.GetRangeIndex(column, row)];
            range.ColumnWidth = 6;

            //output shift name
            range = sheet.Range[StaticFunctions.GetRangeIndex(column, row)];
            StaticFunctions.SaveRichTextToCell(range, ShiftName, PublicEnums.FontWeight.Bold);
            foreach (var logic in ChildrenLogic)
            {
                prevRow = nextRow;
                Tuple<int, int> nextPlace = logic.ExportToExcel(sheet, currentCollumn, row);

                // get larger row increment
                nextRow = nextPlace.Item1 > prevRow ? nextPlace.Item1 : prevRow;
                currentCollumn = nextPlace.Item2;
            }
            if (ChildrenLogic.Count == 0)
                currentCollumn += CoatingScheduleShift.ExcelWidth;

            //create right borders
            for (Int32 i = 0; i < ChildrenLogic.Count; ++i)
            {
                Borders inBorders = sheet.Range[StaticFunctions.GetRangeIndex(column, row), StaticFunctions.GetRangeIndex(column + ((i + 1) * CoatingScheduleShift.ExcelWidth), nextRow - 1)].Borders;
                inBorders.Item[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                inBorders.Item[XlBordersIndex.xlEdgeRight].Weight = XlBorderWeight.xlMedium;

            }
            //create left border
            Borders borders = sheet.Range[StaticFunctions.GetRangeIndex(column, row), StaticFunctions.GetRangeIndex(column, nextRow - 1)].Borders;
            borders.Item[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
            borders.Item[XlBordersIndex.xlEdgeLeft].Weight = XlBorderWeight.xlMedium;

            return new Tuple<int, int>(nextRow, currentCollumn);
        }

        public override bool IsFull()
        {
            var full = ChildrenLogic.All(logic => logic.IsFull());

            return full;
        }

        public override void AddLogic(ICoatingScheduleLogic newController = null)
        {
            if (newController == null)
            {
                foreach (string coatingLine in StaticFactoryValuesManager.CoatingLines)
                {
                    CoatingScheduleShift newLogic = new CoatingScheduleShift();
                    newLogic.CoatingLine = coatingLine;
                    ChildrenLogic.Add(newLogic);
                    newLogic.Connect(this);
                    if (Control != null)
                        Control.AddControlToBottom(newLogic);
                }
            }
            else
            {
                ChildrenLogic.Add(newController);
                newController.Connect(this);
                if (Control != null)
                    Control.AddControlToBottom(newController);
            }

        }


        public override void Connect(ICoatingScheduleLogic logic)
        {
            ParentLogic = logic;
            Date = ((CoatingScheduleDay) logic).Date;
            CoatingLine = ((CoatingScheduleDay) logic).CoatingLine;
        }

        public override void RemoveLogic(ICoatingScheduleLogic child)
        {
            if (child.GetType() == typeof (CoatingScheduleShift))
            {
                ChildrenLogic.Remove((CoatingScheduleShift)child);
                Control.RemoveControl(child.Control);
            }
            else
            {
                foreach (var coatingScheduleShift in ChildrenLogic)
                {
                    coatingScheduleShift.RemoveLogic(child);
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
            return ParentLogic.ChildIsTop(this);
        }

        public override void SwapUp()
        {

        }

        public override void SwapChildUp(ICoatingScheduleLogic upChild, ICoatingScheduleLogic lastLogic)
        {
            ParentLogic.SwapChildUp(upChild,this);
        }

        public event PropertyChangedEventHandler PropertyChanged;


        [NotifyPropertyChangedInvocator]
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public override void AddControlToBottom(ICoatingScheduleLogic upChild)
        {
            if (upChild.GetType() == typeof (CoatingScheduleShift))
            {
                CoatingScheduleShift newController = (CoatingScheduleShift) upChild;

                bool foundLineMatch = false;
                for (Int32 index = 0; !foundLineMatch && index < ChildrenLogic.Count; index++)
                {
                    var shift = ChildrenLogic[index];
                    if (newController.CoatingLine == shift.CoatingLine)
                    {
                        foundLineMatch = true;
                        shift.AddControlToBottom(newController);
                    }
                }
            }
            else if(ChildrenLogic.Count > 0)
            {
                var foundShift =
                    ChildrenLogic.Last(
                        shift => shift.CoatingLine == upChild.CoatingLine);
                if(foundShift!=null)
                    foundShift.AddControlToBottom(upChild);
            }
        }

        public override void SwapDown()
        {

        }

        public override void SwapChildDown(ICoatingScheduleLogic downChild, ICoatingScheduleLogic lastParent)
        {
            ParentLogic.SwapChildDown(downChild, this);
        }

        public override void AddControlToTop(ICoatingScheduleLogic newLogic)
        {
            if (newLogic.GetType() == typeof(CoatingScheduleShift))
            {
                CoatingScheduleShift newController = (CoatingScheduleShift)newLogic;

                bool foundLineMatch = false;
                for (Int32 index = 0; !foundLineMatch && index < ChildrenLogic.Count; index++)
                {
                    var shift = ChildrenLogic[index];
                    if (newController.CoatingLine == shift.CoatingLine)
                    {
                        foundLineMatch = true;
                        shift.AddControlToTop(newController);
                    }
                }
            }
            else if (ChildrenLogic.Count > 0)
            {
                var foundShift =
                    ChildrenLogic.FirstOrDefault(
                        shift => shift.CoatingLine == newLogic.CoatingLine);
                if (foundShift != null)
                    foundShift.AddControlToTop(newLogic);
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
            if (upChild.GetType() == typeof(CoatingScheduleProduct))
            {
                var shifts = ChildrenLogic.Where(x => x.CoatingLine == upChild.CoatingLine);
                foreach (var shift in shifts)
                {
                    shift.PushDownChildren(upChild);
                }
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
            if (upChild.GetType() == typeof(CoatingScheduleProduct))
            {
                var shifts = ChildrenLogic.Where(x => x.CoatingLine == ((CoatingScheduleProduct) upChild).CoatingLine);
                foreach (var shift in shifts)
                {
                    shift.PushUpChildren(upChild);
                }
            }
        }

        public void AddProduct(CoatingScheduleProduct next, CoatingScheduleShift coatingScheduleShift)
        {
            
        }

        public double UnitsProduced(ProductMasterItem item)
        {
            double produced = 0;

            foreach (var coatingScheduleLogic in ChildrenLogic)
            {
                CoatingScheduleShift shift = coatingScheduleLogic as CoatingScheduleShift;
                produced += shift.UnitsProduced(item);
            }

            return produced;
        }

        public double UnitsConsumed(ProductMasterItem item)
        {
            double consumed = 0;
            foreach (var coatingScheduleLogic in ChildrenLogic)
            {
                CoatingScheduleShift shift = coatingScheduleLogic as CoatingScheduleShift;
                consumed += shift.UnitsConsumed(item);
            }

            return consumed;

        }
    }

}
