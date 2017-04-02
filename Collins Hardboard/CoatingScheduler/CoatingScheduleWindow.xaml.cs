using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Configuration_windows;
using ImportLib;
using ModelLib;
using StaticHelpers;

namespace CoatingScheduler
{
    /// <summary>
    /// Interaction logic for CoatingSchedule.xaml
    /// </summary>
    public partial class CoatingScheduleWindow : Window, ICoatingScheduleControl
    {
        public static CoatingScheduleWindow CurrentCoatingScheduleWindow;

        public static RoutedCommand MyCommand = new RoutedCommand();
        private CoatingSchedule _schedule;


        public static List<double> TrackingItemRunningTotals { get; set; }

        private ObservableCollection<Label> _trackingLabels = new ObservableCollection<Label>();
        
        private ObservableCollection<Label> TrackingLabels { get { return _trackingLabels; } } 
        public CoatingSchedule Schedule { get { return _schedule; } set { _schedule = value; } }

        private ObservableCollection<DayControl> _dayControls = new ObservableCollection<DayControl>();
        
        public ObservableCollection<DayControl> DayControls { get { return _dayControls; } set { _dayControls = value; } }


        public CoatingScheduleWindow(CoatingSchedule schedule = null)
        {
            InitializeComponent();


            CurrentCoatingScheduleWindow = this;
            if (schedule != null)
            {
                schedule.Control = this;
                Connect(schedule);
                schedule.ReconnectToControls();
                TrackingStackPanel.DataContext = TrackingLabels;
               // PopulateControlList();
                SchedulerListView.DataContext = typeof(DayControl);
                SchedulerListView.ItemsSource = DayControls;

                LoadInstructions(schedule.InstructionSets);

                if (!ShiftHandler.CoatingInstance.LoadShifts())
                    MessageBox.Show("Could not load shift information");
            }
        }

        public void LoadInstructions(ObservableCollection<CoatingLineInstructionSet> instructionSets)
        {
            InstructionsPanel.Children.Clear();// remove and old controls

            foreach (CoatingLineInstructionSet lineInstructionSet in instructionSets)
            {
                InstructionsPanel.Children.Add(new InstructionSetControl(lineInstructionSet));
            }
        }

        private void PopulateControlList()
        {
            for (Int32 index = 0; index < Schedule.ChildrenLogic.Count; index++)
            {
                DayControls.Add(new DayControl());
            }
        }


        private void AddDayButton_Click(object sender, RoutedEventArgs e)
        {
            Add_Button(sender, e);
        }

        public void RemoveDay(DayControl dayControl)
        {
            DayControls.Remove(dayControl);
            Schedule.RemoveDay(dayControl.Day);
        }

        public void Add_Button(object sender, RoutedEventArgs e)
        {
            Schedule.AddLogic();
        }

        public void AddControlToBottom(ICoatingScheduleLogic logic)
        {
            DayControl newDay = new DayControl();
            DayControls.Add(newDay);
            logic.Connect(newDay);
            newDay.Connect(this);
        }

        public void AddControlToTop(ICoatingScheduleLogic logic)
        {
            DayControl newDay = new DayControl();
            DayControls.Insert(0,newDay);
            logic.Connect(newDay);
            newDay.Connect(this);
        }

        public void RemoveControl(ICoatingScheduleControl child)
        {
            DayControls.Remove((DayControl) child);
            child.DestroySelf();
        }

        public void Connect(ICoatingScheduleLogic logic)
        {
            Schedule = (CoatingSchedule) logic;
            DataContext = logic;
            UpdateControlData();
        }

        public void Connect(ICoatingScheduleControl parent)
        {
        }

        public ICoatingScheduleLogic GetLogic()
        {
            return Schedule;
        }

        public void DestroySelf()
        {
            InstructionsPanel.Children.Clear();
            foreach (var dayControl in DayControls)
            {
                dayControl.DestroySelf();
            }
            DayControls.Clear();
        }

        public void Disconnect()
        {

        }
        public ICoatingScheduleControl SwapControlWithBottom(ICoatingScheduleControl newControl)
        {
            ICoatingScheduleControl returnControl = null;

            if (DayControls.Count == 0) return null;

            if (newControl.GetType() == DayControls.Last().GetType())
            {
                returnControl = DayControls.Last();
                DayControls[DayControls.Count - 1] = (DayControl)newControl;
            }
            else
            {
                returnControl = DayControls.Last().SwapControlWithBottom(newControl);
            }
            return returnControl;
        }

        public ICoatingScheduleControl SwapControlWithTop(ICoatingScheduleControl newControl)
        {
            ICoatingScheduleControl returnControl = null;

            if (DayControls.Count == 0) return null;

            if (newControl.GetType() == DayControls.Last().GetType())
            {
                returnControl = DayControls.First();
                DayControls[0] = (DayControl)newControl;
            }
            else
            {
                returnControl = DayControls.First().SwapControlWithTop(newControl);
            }
            return returnControl;
        }

        public void UpdateControlData()
        {
            if (InstructionsPanel != null)
            {
                InstructionsPanel.Children.Clear();
                foreach (CoatingLineInstructionSet lineInstructionSet in Schedule.InstructionSets)
                {
                    InstructionsPanel.Children.Add(new InstructionSetControl(lineInstructionSet));
                }
            }
            foreach (var dayControl in DayControls)
            {
                dayControl.UpdateControlData();
            }
        }

        public bool SwapChildControl(ICoatingScheduleControl oldControl, ICoatingScheduleControl newControl)
        {
            bool replaced = false;

            try
            {
                Int32 index = DayControls.IndexOf((DayControl)oldControl);
                if (index >= 0)
                {
                    DayControls[index] = (DayControl)newControl;
                    replaced = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return replaced;
        
        }

        public void UpdateDateRange(String newText)
        {
            DateTextBlock.Text = newText;
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            // force focus change to force validation on child controls
            Keyboard.Focus(this);
            Schedule.Save();
        }

        private void LoadButton_OnClick(object sender, RoutedEventArgs e)
        {
            // force focus change to force validation on child controls
            Keyboard.Focus(this);
            var schedule = CoatingSchedule.LoadSchedule();
            if (schedule != null)
            {
                Schedule.Clear();
                Schedule = schedule;
                Schedule.Control = this;
                Schedule.ReconnectToControls();
            }
        }

        private void ExcelExportButton_OnClick(object sender, RoutedEventArgs e)
        {
            // force focus change to force validation on child controls
            Keyboard.Focus(this);

            // start export
            if(!Schedule.Exporting)
            {
                Thread exportThread = new Thread(new ThreadStart(
            Schedule.ExportToExcel));
                exportThread.Start();
            }
            else
            {
                MessageBox.Show("Export already taking place.");
            }
            
        }

        private void NewButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Save current schedule?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                SaveButton_OnClick(sender,e);
            }

            Schedule.Clear();
        }
    }
}
