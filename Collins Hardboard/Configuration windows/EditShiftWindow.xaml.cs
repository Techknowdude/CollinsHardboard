using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace Configuration_windows
{
    /// <summary>
    /// Interaction logic for NewShiftWindow.xaml
    /// </summary>
    public partial class EditShiftWindow : Window
    {
        private Shift _shift;
        private bool _isCoating = true;
        public bool Accepted { get; set; }

        public EditShiftWindow(bool isCoating = true, Shift shift = null)
        {
            InitializeComponent();
            Accepted = false;
            _shift = shift;
            if (shift != null)
                LoadShiftInfo();
            _isCoating = isCoating;
        }

        private void LoadShiftInfo()
        {
            NameTextBox.Text = Shift.Name;
            StartDatePicker.SelectedDate = Shift.StartDate;
            EndDatePicker.SelectedDate = Shift.EndDate;
            StartTimeBox.Text = Shift.StartTime.ToString("hh:mm:ss tt");
            EndTimeBox.Text = Shift.Duration.ToString("hh':'mm':'ss");

            MondayCheckBox.IsChecked = Shift.DaysList.Any(day => day == DayOfWeek.Monday);
            TuesdayCheckBox.IsChecked = Shift.DaysList.Any(day => day == DayOfWeek.Tuesday);
            WednesdayCheckBox.IsChecked = Shift.DaysList.Any(day => day == DayOfWeek.Wednesday);
            ThursdayCheckBox.IsChecked = Shift.DaysList.Any(day => day == DayOfWeek.Thursday);
            FridayCheckBox.IsChecked = Shift.DaysList.Any(day => day == DayOfWeek.Friday);
            SaturdayCheckBox.IsChecked = Shift.DaysList.Any(day => day == DayOfWeek.Saturday);
            SundayCheckBox.IsChecked = Shift.DaysList.Any(day => day == DayOfWeek.Sunday);

            ForegroundColorPicker.SelectedColor = Shift.ForegroundColor;
            BackgroundColorPicker.SelectedColor = Shift.BackgroundColor;
        }

        public Shift Shift
        {
            get { return _shift; }
            set { _shift = value; }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            Accepted = true;
            try
            {
                DateTime start;
                DateTime end;
                try
                {
                    start = (DateTime)StartDatePicker.SelectedDate;
                }
                catch (Exception)
                {
                    MessageBox.Show("Please enter a valid start date.");
                    Accepted = false;
                    return;
                } try
                {
                    end = (DateTime)EndDatePicker.SelectedDate;
                }
                catch (Exception)
                {
                    MessageBox.Show("Please enter a valid end date.");
                    Accepted = false;
                    return;
                }
                DateTime shiftStart;
                DateTime time;
                try
                {
                    time = DateTime.ParseExact(StartTimeBox.Text, "hh:mm:ss tt", CultureInfo.CurrentCulture);
                    shiftStart = start + time.TimeOfDay;
                    
                }
                catch
                {
                    MessageBox.Show("Shift start time is invalid.");
                    Accepted = false;
                    return;
                }
                TimeSpan duration;
                if (!TimeSpan.TryParse(EndTimeBox.Text, out duration))
                {
                    MessageBox.Show("Shift duration is invalid.");
                    Accepted = false;
                    return;
                }
                List<DayOfWeek> daysOfWeek = new List<DayOfWeek>();
                if(MondayCheckBox.IsChecked == true)
                    daysOfWeek.Add(DayOfWeek.Monday);
                if (TuesdayCheckBox.IsChecked == true)
                    daysOfWeek.Add(DayOfWeek.Tuesday);
                if (WednesdayCheckBox.IsChecked == true)
                    daysOfWeek.Add(DayOfWeek.Wednesday);
                if (ThursdayCheckBox.IsChecked == true)
                    daysOfWeek.Add(DayOfWeek.Thursday);
                if (FridayCheckBox.IsChecked == true)
                    daysOfWeek.Add(DayOfWeek.Friday);
                if (SaturdayCheckBox.IsChecked == true)
                    daysOfWeek.Add(DayOfWeek.Saturday);
                if (SundayCheckBox.IsChecked == true)
                    daysOfWeek.Add(DayOfWeek.Sunday);

                if(_shift == null)
                    Shift = Shift.ShiftFactory(NameTextBox.Text, shiftStart, duration, start, end, null, daysOfWeek, _isCoating);
                else
                {
                    _shift.Name = NameTextBox.Text;
                    _shift.StartDate = start;
                    _shift.Duration = duration;
                    _shift.StartTime = shiftStart;
                    _shift.EndDate = end;
                    _shift.DaysList = daysOfWeek;
                }
                Shift.ForegroundColor = ForegroundColorPicker.SelectedColor;
                Shift.BackgroundColor = BackgroundColorPicker.SelectedColor;
            }
            catch(Exception exception)
            {
                MessageBox.Show(String.Format( "There was an error while trying to save the shift information.\nDeveloper information: {0}",exception.Message));
                Accepted = false;
            }
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Accepted = false;
            Close();
        }
    }
}
