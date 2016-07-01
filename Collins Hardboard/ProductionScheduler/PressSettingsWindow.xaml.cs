using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ImportLib;
using Microsoft.Win32;
using ModelLib;

namespace ProductionScheduler
{
    /// <summary>
    /// Interaction logic for PressSettingsWindow.xaml
    /// </summary>
    public partial class PressSettingsWindow : Window
    {
        public int NumPlates
        {
            get { return PressManager.NumPlates; }
            set { PressManager.NumPlates = value; }
        }

        public double PressLoadsPerHour { get { return PressManager.PressLoadsPerHour; } set
        {
            PressManager.PressLoadsPerHour = value;
        } }

        public bool MondayChecked
        {
            get { return _mondayChecked; }
            set
            {
                _mondayChecked = value;
                if (value)
                {
                    if (!ChangeDays.Contains(DayOfWeek.Monday))
                    {
                        ChangeDays.Add(DayOfWeek.Monday);
                    }
                }
                else
                {
                    ChangeDays.Remove(DayOfWeek.Monday);
                }
            }
        }

        public bool TuesdayChecked
        {
            get { return _tuesdayChecked; }
            set
            {
                _tuesdayChecked = value;
                if (value)
                {
                    if (!ChangeDays.Contains(DayOfWeek.Tuesday))
                    {
                        ChangeDays.Add(DayOfWeek.Tuesday);
                    }
                }
                else
                {
                    ChangeDays.Remove(DayOfWeek.Tuesday);
                }
            }
        }

        public bool WednesdayChecked
        {
            get { return _wednesdayChecked; }
            set
            {
                _wednesdayChecked = value;
                if (value)
                {
                    if (!ChangeDays.Contains(DayOfWeek.Wednesday))
                    {
                        ChangeDays.Add(DayOfWeek.Wednesday);
                    }
                }
                else
                {
                    ChangeDays.Remove(DayOfWeek.Wednesday);
                }
            }
        }

        public bool ThursdayChecked
        {
            get { return _thursdayChecked; }
            set
            {
                _thursdayChecked = value;
                if (value)
                {
                    if (!ChangeDays.Contains(DayOfWeek.Thursday))
                    {
                        ChangeDays.Add(DayOfWeek.Thursday);
                    }
                }
                else
                {
                    ChangeDays.Remove(DayOfWeek.Thursday);
                }
            }
        }

        public bool FridayChecked
        {
            get { return _fridayChecked; }
            set
            {
                _fridayChecked = value;
                if (value)
                {
                    if (!ChangeDays.Contains(DayOfWeek.Friday))
                    {
                        ChangeDays.Add(DayOfWeek.Friday);
                    }
                }
                else
                {
                    ChangeDays.Remove(DayOfWeek.Friday);
                }
            }
        }

        public bool SaturdayChecked
        {
            get { return _saturdayChecked; }
            set
            {
                _saturdayChecked = value;
                if (value)
                {
                    if (!ChangeDays.Contains(DayOfWeek.Saturday))
                    {
                        ChangeDays.Add(DayOfWeek.Saturday);
                    }
                }
                else
                {
                    ChangeDays.Remove(DayOfWeek.Saturday);
                }
            }
        }

        public bool SundayChecked
        {
            get { return _sundayChecked; }
            set
            {
                _sundayChecked = value;
                if (value)
                {
                    if (!ChangeDays.Contains(DayOfWeek.Sunday))
                    {
                        ChangeDays.Add(DayOfWeek.Sunday);
                    }
                }
                else
                {
                    ChangeDays.Remove(DayOfWeek.Sunday);
                }
            }
        }

        private bool _mondayChecked;
        private bool _tuesdayChecked;
        private bool _wednesdayChecked;
        private bool _thursdayChecked;
        private bool _sundayChecked;
        private bool _saturdayChecked;
        private bool _fridayChecked;

        public static ObservableCollection<DayOfWeek> ChangeDays
        {
            get { return PressManager.PlateChangeDays; }
        }

        public PressSettingsWindow()
        {
            PressManager.Instance.GetHashCode(); // init instance
            InitializeComponent();
            DataContext = this;
            SetControlData();
            Closing += PressSettingsWindow_Closing;
        }

        private void SetControlData()
        {
            if (ChangeDays.Any(x => x == DayOfWeek.Monday))
            {
                MondayCheckBox.IsChecked = true;
            }
            if (ChangeDays.Any(x => x == DayOfWeek.Tuesday))
            {
                TuesdayCheckBox.IsChecked = true;
            }
            if (ChangeDays.Any(x => x == DayOfWeek.Wednesday))
            {
                WednesdayCheckBox.IsChecked = true;
            }
            if (ChangeDays.Any(x => x == DayOfWeek.Thursday))
            {
                ThursdayCheckBox.IsChecked = true;
            }
            if (ChangeDays.Any(x => x == DayOfWeek.Friday))
            {
                FridayCheckBox.IsChecked = true;
            }
            if (ChangeDays.Any(x => x == DayOfWeek.Saturday))
            {
                SaturdayCheckBox.IsChecked = true;
            }
            if (ChangeDays.Any(x => x == DayOfWeek.Sunday))
            {
                SundayCheckBox.IsChecked = true;
            }
        }

        void PressSettingsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Would you like to save any changes?", "", MessageBoxButton.YesNoCancel);
            if (result ==
                MessageBoxResult.Yes)
            {
                if (PressManager.Save())
                    MessageBox.Show("Save successful");
                else
                {
                    MessageBox.Show("There was an issue saving.");
                }
            }
            else if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }
    }
}
