﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Configuration_windows;
using ImportLib;
using Microsoft.Office.Interop.Excel;
using ModelLib;
using StaticHelpers;

namespace CoatingScheduler
{
    [Serializable]
    public class CoatingScheduleShift : ICoatingScheduleLogic
    {
        private static int _excelWidth = CoatingScheduleProductBase.ExcelWidth;

        public CoatingScheduleShift()
        {

        }

        private CoatingScheduleShift(string coatingLine = "", ObservableCollection<ICoatingScheduleLogic> children = null)
        {
            CoatingLine = coatingLine;
            if (children == null && ChildrenLogic == null)
            {
                ChildrenLogic = new ObservableCollection<ICoatingScheduleLogic>();
            }
            else
            {
                ChildrenLogic = children;
            }
        }

        public static int ExcelWidth
        {
            get { return _excelWidth; }
            set { _excelWidth = value; }
        }

        public override void Save(Stream stream, IFormatter formatter)
        {
            formatter.Serialize(stream,this);
        }

        public override Tuple<int, int> ExportToExcel(_Worksheet sheet, int column, int row)
        {
            //Set column widths
            Range range = sheet.Range[StaticFunctions.GetRangeIndex(column, row)];
            range.ColumnWidth = 5;
            range = sheet.Range[StaticFunctions.GetRangeIndex(column + 1, row)];
            range.ColumnWidth = 20;
            range = sheet.Range[StaticFunctions.GetRangeIndex(column + 2, row)];
            range.ColumnWidth = 5;
            range = sheet.Range[StaticFunctions.GetRangeIndex(column + 3, row)];
            range.ColumnWidth = 5;
            Int32 nextRow = row;
            Int32 width = column;
            foreach (var logic in ChildrenLogic)
            {
                Tuple<int, int> nextPlace = logic.ExportToExcel(sheet, column, nextRow);
                nextRow = nextPlace.Item1;
                width = nextPlace.Item2 > width ? nextPlace.Item2 : width;
            }
            if (ChildrenLogic.Count == 0)
                width += CoatingScheduleProductBase.ExcelWidth;

            return new Tuple<int, int>(nextRow, width);
        }

        public override void ReconnectToControls()
        {
            if (Control != null)
            {
                foreach (var logic in ChildrenLogic) // foreach product or note, create a control
                {
                    if (logic.Control == null)
                    {
                        Control.AddControlToBottom(logic);
                        logic.Connect(this);
                    }
                }
            }
        }


        public static CoatingScheduleShift Load(Stream stream, IFormatter formatter)
        {
            return (CoatingScheduleShift) formatter.Deserialize(stream);
        }


        public override bool IsFull()
        {
            Shift shift = ((CoatingScheduleLine)ParentLogic).Shift;
            if (shift != null)
            {
                double hours = shift.Duration.TotalHours;

                foreach (var logic in ChildrenLogic)
                {
                    var product = logic as CoatingScheduleProduct;
                    if (product != null && product.Units != String.Empty)
                    {
                        var units = Double.Parse(product.Units);
                            var master = StaticInventoryTracker.ProductMasterList.FirstOrDefault(m => m.MasterID == product.MasterID);
                        if (product.Config == null)
                        {
                            if (master != null)
                            {
                                product.Config =
                                MachineHandler.Instance.AllConfigurations.FirstOrDefault(
                                    c => c.CanMake(master));
                            }
                        }
                        if (product.Config != null && master != null)
                        {
                            // use configs to find the rate
                            var output = product.Config.OutputItems.FirstOrDefault(o => o.MasterID == product.MasterID);
                            if(output != null)
                            {
                                var pcsPerMin = output.Pieces * product.Config.PiecesOutPerMinute;
                                hours -= units / (pcsPerMin * 60 / master.PiecesPerUnit);
                            }
                            else
                            {
                                // do our best to estimate what the master file says
                                hours -= units / product.UnitsPerHour;
                            }
                        }
                        else
                        {
                            // do our best to estimate what the master file says
                            hours -= units / product.UnitsPerHour;
                        }
                    }
                }

                // mark as full if the shift has been used 75% or more
                return hours/ shift.Duration.TotalHours <= .15;
            }
            return true;
        }

        public override void AddLogic(ICoatingScheduleLogic newController = null) //update with a picker window. include a default.
        {
            try
            {
                if (newController != null)
                {
                    newController.CoatingLine = CoatingLine;
                }
                else
                {
                    newController = new CoatingScheduleProduct(0, "", "", "", "") { CoatingLine = CoatingLine };
                }
                ChildrenLogic.Add(newController);
                newController.Connect(this);
                if (Control != null)
                    Control.AddControlToBottom(newController);

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public override void AddControlToBottom(ICoatingScheduleLogic upChild)
        {

            ChildrenLogic.Add(upChild);
            upChild.Connect(this);
            Control.AddControlToBottom(upChild);
        }

        public override void SwapDown()
        {
            ParentLogic.SwapChildDown(this, this);
        }

        public override void SwapChildDown(ICoatingScheduleLogic downChild, ICoatingScheduleLogic lastParent)
        {
            ICoatingScheduleLogic childProduct = downChild;
            Int32 currentIndex = ChildrenLogic.IndexOf(childProduct);

            if (currentIndex == ChildrenLogic.Count - 1) // is at bottom
            {
                ChildrenLogic.Remove(childProduct);
                Control.RemoveControl(childProduct.Control);
                ParentLogic.SwapChildDown(downChild, this);
            }
            else
            {
                ICoatingScheduleLogic swapProduct = ChildrenLogic[currentIndex + 1];
                ChildrenLogic[currentIndex + 1] = childProduct;
                ChildrenLogic[currentIndex] = swapProduct;

                childProduct.SwapControls(swapProduct);
            }
        }

        public override void AddControlToTop(ICoatingScheduleLogic newLogic)
        {
            ChildrenLogic.Insert(0, newLogic);
            newLogic.Connect(this);
            Control.AddControlToTop(newLogic);
            SpreadUnits();
        }

        public override void PushDown()
        {
            ParentLogic.PushDownChildren(this);
        }

        public override void PushChildDown(ICoatingScheduleLogic upChild, ICoatingScheduleLogic lastParent)
        {
            ParentLogic.PushChildDown(upChild, this);
        }

        public override void PushDownChildren(ICoatingScheduleLogic upChild)
        {
            if (ChildrenLogic.Count > 0)
            {
                ChildrenLogic.Last().SwapDown();
            }
        }

        public override void PushUp()
        {
            ParentLogic.PushChildUp(this, null);
        }

        public override void PushChildUp(ICoatingScheduleLogic upChild, ICoatingScheduleLogic lastParent)
        {
            ParentLogic.PushChildUp(upChild, this);
        }

        public override void PushUpChildren(ICoatingScheduleLogic upChild)
        {
            if (ChildrenLogic.Count > 0 && !ParentLogic.ChildIsTop(this))
            {
                ChildrenLogic.First().SwapUp();
            }
        }

        public override void Connect(ICoatingScheduleLogic logic)
        {
            ParentLogic = logic;
        }

        public override void RemoveLogic(ICoatingScheduleLogic child)
        {

            ChildrenLogic.Remove(child);
            Control.RemoveControl(child.Control);

        }

        public override void DestroySelf()
        {
            for (Int32 index = 0; index < ChildrenLogic.Count; index++)
            {
                var product = ChildrenLogic[index];
                product.Disconnect();
            }
            ParentLogic.RemoveLogic(this);
            ChildrenLogic.Clear();
            Disconnect();
        }

        public override bool ChildIsTop(ICoatingScheduleLogic child)
        {
            bool isTop = false;
            if (ChildrenLogic.Count > 0)
            {
                isTop = ChildrenLogic.First() == child;
                if (isTop)
                    isTop = ParentLogic.ChildIsTop(this);
            }
            return isTop;
        }

        public override void SwapUp()
        {
            ParentLogic.SwapChildUp(this, this);
        }

        public override void SwapChildUp(ICoatingScheduleLogic upChild, ICoatingScheduleLogic lastLogic)
        {
            ICoatingScheduleLogic childProduct = upChild;
            Int32 currentIndex = ChildrenLogic.IndexOf(childProduct);

            if (currentIndex == 0)
            {
                ChildrenLogic.Remove(childProduct);
                Control.RemoveControl(childProduct.Control);
                ParentLogic.SwapChildUp(upChild, this);
            }
            else
            {
                ICoatingScheduleLogic swapProduct = ChildrenLogic[currentIndex - 1];
                ChildrenLogic[currentIndex - 1] = childProduct;
                ChildrenLogic[currentIndex] = swapProduct;

                childProduct.SwapControls(swapProduct);
            }

        }

        public void SpreadUnits()
        {
            // check if shift is full
            if (!IsFull()) return;

            // if full, give next shift remaining units

            Shift shift = ((CoatingScheduleLine)ParentLogic).Shift;
            double hours = shift.Duration.TotalHours;

            foreach (var logic in ChildrenLogic)
            {
                var product = logic as CoatingScheduleProduct;
                if (product != null)
                {
                    var units = Double.Parse(product.Units);
                    if (hours - units / product.UnitsPerHour >= 0) // not full. continue.
                        hours -= units / product.UnitsPerHour;
                    else // shift is full, split into two products and add to next shift
                    {
                        // Save as much production as possible and pass problem onto next shift
                        double max = hours * product.UnitsPerHour;

                        if (max <= 0)
                        {
                            product.SwapDown(); // move to next, cascade spread
                        }
                        else
                        {

                            CoatingScheduleProduct next = new CoatingScheduleProduct(product)
                            {
                                Config = product.Config,
                                Machine = product.Machine,
                                CoatingLine = CoatingLine
                            };
                            product.SetUnits(max.ToString("N"));
                            product.Control.UpdateControlData();

                            next.SetUnits((Double.Parse(next.Units) - max).ToString("N"));

                            // add to bottom
                            AddControlToBottom(next);

                            // move to next control
                            next.SwapDown();

                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Fills the shift with the item by using the config passed.
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="config"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public double ScheduleItem(Machine machine, Configuration config, ProductMasterItem item)
        {
            double made = 0;

            CoatingScheduleProduct product = new CoatingScheduleProduct(item);

            AddLogic(product);

            Shift shift = ((CoatingScheduleLine)ParentLogic).Shift;
            double hours = shift.Duration.TotalHours;

            var productController = ChildrenLogic.Last() as CoatingScheduleProduct;

            // find the maximum units that can be scheduled.
            made = config.UnitsToMakeInHours(item,hours);
            made = Math.Ceiling(made);

            productController.Machine = machine;
            productController.Config = config;
            productController.SetUnits(made.ToString());

            return made;
        }

        public double UnitsProduced(ProductMasterItem item)
        {
            double produced = 0;
            foreach (var coatingScheduleLogic in ChildrenLogic.Where(l => l is CoatingScheduleProduct))
            {
                var product = ((CoatingScheduleProduct)coatingScheduleLogic);

                if (product.MasterID == item.MasterID)
                {
                    double temp = 0;
                    Double.TryParse(product.Units, out temp);
                    produced += temp;
                }
            }

            return produced;
        }

        public double UnitsConsumed(ProductMasterItem item)
        {
            double consumed = 0;
            foreach (var coatingScheduleLogic in ChildrenLogic.Where(l => l is CoatingScheduleProduct))
            {
                var product = ((CoatingScheduleProduct) coatingScheduleLogic);
                var config = product.Config;

                consumed += config.GetUnitsConsumed(item, product.Units, product.MasterID);
            }

            return consumed;
        }

        public List<Machine> GetMachinesUsed()
        {
            List<Machine> machines = new List<Machine>();

            foreach (var coatingScheduleLogic in ChildrenLogic)
            {
                var prodItem = coatingScheduleLogic as CoatingScheduleProduct;
                if (prodItem?.Machine != null)
                {
                    machines.Add(prodItem.Machine);
                }
            }

            return machines;
        }
    }

}
