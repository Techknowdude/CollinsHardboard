using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ModelLib;

namespace CoatingScheduler
{
    /// <summary>
    /// Interaction logic for DayControl.xaml
    /// </summary>
    public partial class DayControl : ICoatingScheduleControl
    {
        private CoatingScheduleDay _day;

        public CoatingScheduleWindow ParentControl { get; set; }
        public CoatingScheduleDay Day { get { return _day; } set { _day = value; } }

        public ObservableCollection<LineControl> LineControls { get; set; }

        public DayControl()
        {
            LineControls = new ObservableCollection<LineControl>();

            InitializeComponent();
            LineListView.DataContext = typeof(LineControl);
           
            LineListView.ItemsSource = LineControls;
            UpdateControlData();
        }

        public void ReloadTrackingInfo()
        {
        }

        private void RemoveDayButton_Click(object sender, RoutedEventArgs e)
        {
            Day.DestroySelf();
        }

        public void Add_Button(object sender, RoutedEventArgs e)
        {
            Day.AddLogic();
        }

        public void AddControlToBottom(ICoatingScheduleLogic logic)
        {
            LineControl newControl = new LineControl(logic) { VerticalAlignment = VerticalAlignment.Top };
            LineControls.Add(newControl);
            logic.Connect(newControl);
            newControl.Connect(this);
        }

        public void AddControlToTop(ICoatingScheduleLogic logic)
        {
            LineControl newControl = new LineControl(logic){VerticalAlignment = VerticalAlignment.Top};
            LineControls.Insert(0,newControl);
            logic.Connect(newControl);
            newControl.Connect(this);
        }

        public void RemoveControl(ICoatingScheduleControl child)
        {
            LineControls.Remove((LineControl) child);
            child.DestroySelf();
        }

        public void Connect(ICoatingScheduleLogic logic)
        {
            Day = (CoatingScheduleDay)logic;
            DataContext = Day;
            UpdateControlData();
        }

        public void Connect(ICoatingScheduleControl parent)
        {
            ParentControl = parent as CoatingScheduleWindow;
        }


        public ICoatingScheduleLogic GetLogic()
        {
            return Day;
        }


        public void DestroySelf()
        {
            foreach (var lineControl in LineControls)
            {
                lineControl.DestroySelf();
            }
            LineControls.Clear();
            Disconnect();
        }

        public void Disconnect()
        {
            Day = null;
        }
        public ICoatingScheduleControl SwapControlWithBottom(ICoatingScheduleControl newControl)
        {
            ICoatingScheduleControl returnControl = null;

            if (LineControls.Count == 0) return null;

            if (newControl.GetType() == LineControls.Last().GetType())
            {
                returnControl = LineControls.Last();
                LineControls[LineControls.Count - 1] = (LineControl)newControl;
            }
            else
            {
                returnControl = LineControls.Last().SwapControlWithBottom(newControl);
            }
            return returnControl;
        }

        public ICoatingScheduleControl SwapControlWithTop(ICoatingScheduleControl newControl)
        {
            ICoatingScheduleControl returnControl = null;

            if (LineControls.Count == 0) return null;

            if (newControl.GetType() == LineControls.Last().GetType())
            {
                returnControl = LineControls.First();
                LineControls[0] = (LineControl)newControl;
            }
            else
            {
                returnControl = LineControls.First().SwapControlWithTop(newControl);
            }
            return returnControl;
        }

        public void UpdateControlData()
        {
            if(Day == null) return;

            DayDatePicker.SelectedDate = Day.Date;
            foreach (var lineControl in LineControls)
            {
                lineControl.UpdateControlData();
            }
        }
   
        public bool SwapChildControl(ICoatingScheduleControl oldControl, ICoatingScheduleControl newControl)
        {
            bool replaced = false;

            try
            {
                Int32 index = LineControls.IndexOf((LineControl) oldControl);
                if (index >= 0)
                {
                    LineControls[index] = (LineControl)newControl;
                    replaced = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return replaced;
        }
        

        private void DayDatePicker_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Day == null) return;
            try
            {
                Day.Date = (DateTime)DayDatePicker.SelectedDate;
                ((CoatingSchedule) Day.ParentLogic).UpdateDateText();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);        
            }

        }

        private void RunDayButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Change inventory according to scheduled items?", "", MessageBoxButton.YesNo) ==
                MessageBoxResult.Yes)
            {
                foreach (var lineControl in LineControls)
                {
                    lineControl.GetInvChange();
                }
                // modify inventory accordingly

                Day.DestroySelf();
            }
        }
    }
}
