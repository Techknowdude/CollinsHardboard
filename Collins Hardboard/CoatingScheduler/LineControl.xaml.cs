using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Configuration_windows;
using ImportLib;
using ModelLib;
using StaticHelpers;

namespace CoatingScheduler
{
    /// <summary>
    /// Interaction logic for LineControl.xaml
    /// </summary>
    public partial class LineControl : UserControl, ICoatingScheduleControl
    {
        private CoatingScheduleLine _line;
        public DayControl Day;
        public ICoatingScheduleControl ParentControl { get; set; }

        public CoatingScheduleLine Line { get { return _line; } set { _line = value; } }

        public ObservableCollection<ShiftControl> ShiftControls
        {
            get;
            set;
        }
        public LineControl(ICoatingScheduleLogic logic)
        {
            ShiftControls = new ObservableCollection<ShiftControl>();

            InitializeComponent();
            ShiftListView.ItemsSource = ShiftControls;
            ShiftBox.ItemsSource = ShiftHandler.CoatingInstance.Shifts;
            DataContext = logic;

            UpdateControlData();
        }

        private void BtnRemove_OnClick(object sender, RoutedEventArgs e)
        {
            Line.DestroySelf();
        }

        public void Add_Button(object sender, RoutedEventArgs e)
        {
            Line.AddLogic();
        }

        public void AddControlToBottom(ICoatingScheduleLogic logic)
        {
            ShiftControl newControl = new ShiftControl(logic) { VerticalAlignment = VerticalAlignment.Top };
            ShiftControls.Add(newControl);
            logic.Connect(newControl);
            newControl.Connect(this);
        }

        public void AddControlToTop(ICoatingScheduleLogic logic)
        {
            ShiftControl newControl = new ShiftControl(logic) { VerticalAlignment = VerticalAlignment.Top };
            ShiftControls.Insert(0, newControl);
            logic.Connect(newControl);
            newControl.Connect(this);
        }

        public void RemoveControl(ICoatingScheduleControl child)
        {
            ShiftControls.Remove((ShiftControl)child);
            child.DestroySelf();
        }

        public void Connect(ICoatingScheduleLogic logic)
        {
            Line = (CoatingScheduleLine)logic;
            DataContext = Line;
            UpdateControlData();
        }

        public void Connect(ICoatingScheduleControl parent)
        {
            ParentControl = parent;
        }

        public ICoatingScheduleLogic GetLogic()
        {
            return Line;
        }

        public void DestroySelf()
        {
            for (Int32 index = 0; index < ShiftControls.Count; index++)
            {
                var control = ShiftControls[index];
                control.DestroySelf();
            }
            ShiftControls.Clear();
            Disconnect();
        }

        public void Disconnect()
        {
            ParentControl = null;
            Line = null;
        }

        public ICoatingScheduleControl SwapControlWithBottom(ICoatingScheduleControl newControl)
        {
            ICoatingScheduleControl returnControl = null;

            if (ShiftControls.Count == 0) return null;

            if (newControl.GetType() == ShiftControls.Last().GetType())
            {
                returnControl = ShiftControls.Last();
                ShiftControls[ShiftControls.Count - 1] = (ShiftControl)newControl;
            }
            else
            {
                returnControl = ShiftControls.Last().SwapControlWithBottom(newControl);
            }
            return returnControl;
        }

        public ICoatingScheduleControl SwapControlWithTop(ICoatingScheduleControl newControl)
        {
            ICoatingScheduleControl returnControl = null;

            if (ShiftControls.Count == 0) return null;

            if (newControl.GetType() == ShiftControls.Last().GetType())
            {
                returnControl = ShiftControls.First();
                ShiftControls[0] = (ShiftControl)newControl;
            }
            else
            {
                returnControl = ShiftControls.First().SwapControlWithBottom(newControl);
            }
            return returnControl;
        }

        public void UpdateControlData()
        {
            if (Line != null)
            {

                Int32 foundIndex = ShiftHandler.CoatingInstance.Shifts.ToList().FindIndex(x => x == Line.Shift);
                if (foundIndex >= 0)
                    ShiftBox.SelectedIndex = foundIndex;
                foreach (var shiftControl in ShiftControls)
                {
                    shiftControl.UpdateControlData();
                }
            }
        }

        public bool SwapChildControl(ICoatingScheduleControl oldControl, ICoatingScheduleControl newControl)
        {
            throw new Exception("Cannot swap children.");
        }

        private void ShiftBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Line != null)
            {
                Line.Shift = ShiftBox.SelectedItem as Shift;
            }
        }

        public void ClearRunningTotals()
        {
            RunningTotalStackPanel.Children.Clear();
        }

        public void AddRunningTotal(String content, Int32 row)
        {
            while (row > RunningTotalStackPanel.Children.Count - 1)// remove -1
                RunningTotalStackPanel.Children.Add(new StackPanel() { Orientation = Orientation.Horizontal });

            StackPanel stack = (StackPanel)RunningTotalStackPanel.Children[row];
            stack.Children.Add(new Label()
            {
                Content = content,
                Width = 70,
                Height = 87,
                Margin = new Thickness(0, 0, 0, 4),
                BorderBrush = System.Windows.Media.Brushes.RoyalBlue,
                BorderThickness = new Thickness(2),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            });
        }

        public void LoadTrackingInfo()
        {
            ClearRunningTotals();

            // foreach tracking
            // foreach row
            // foreach shift

            // update each tracking item
            for (int index = 0; index < TrackingSelectionWindow.TrackingItems.Count; index++)
            {
                bool done = false;

                var productMasterItem = TrackingSelectionWindow.TrackingItems[index];
                double runningTotal = CoatingScheduleWindow.TrackingItemRunningTotals[index];

                // get numbers from each shift row
                for (int row = 0; !done; row++)
                {
                    done = true;
                    for (int i = 0; i < ShiftControls.Count; i++)
                    {
                        var shiftControl = ShiftControls[i];

                        if (shiftControl.ProductControls.Count > row)
                        {
                            done = false;

                            try
                            {
                                runningTotal += shiftControl.GetRunningTotal(productMasterItem, row);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);

                            }

                            CoatingScheduleWindow.TrackingItemRunningTotals[index] = runningTotal;
                        }
                    }
                    if(!done)
                        AddRunningTotal(runningTotal.ToString(), row);

                }
            }

            #region old code
            //foreach (var shiftControl in ShiftControls)
            //{
            //    foreach (var productMasterItem in TrackingSelectionWindow.TrackingItems)
            //    {
            //        foreach (var productControl in shiftControl.ProductControls)
            //        {
            //            if (productControl is ProductNoteControl)
            //            {
            //                AddRunningTotal(CoatingScheduleWindow.GetCurrentTotal(productMasterItem, 0), 0, 0);

            //            }
            //            else
            //            {
            //                AddRunningTotal(CoatingScheduleWindow.GetCurrentTotal(productMasterItem, 0), 0, 0);

            //            }
            //        }
            //    }
            //}

            #endregion
        }

        public void ReloadTrackingInfo()
        {
            ((DayControl)ParentControl).ReloadTrackingInfo();
        }

        public void GetInvChange()
        {
            foreach (var shiftControl in ShiftControls)
            {
                shiftControl.GetInvChange();
            }
        }
    }
}
