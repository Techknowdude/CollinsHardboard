using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Win32;

namespace Configuration_windows
{
    /// <summary>
    /// Interaction logic for ShiftWindow.xaml
    /// </summary>
    public partial class ShiftWindow : Window
    {
        public bool IsCoating;
        public ObservableCollection<Shift> Shifts { get { return IsCoating ? ShiftHandler.CoatingInstance.Shifts : ShiftHandler.ProductionInstance.Shifts; } }


        public ShiftWindow(bool isCoating)
        {
            InitializeComponent();
            IsCoating = isCoating;
            ShiftCalendar.IsCoating = isCoating;
            ShiftListView.ItemsSource = Shifts;
            Closing += ShiftWindow_Closing;
            ShiftCalendar.Refresh();
            Title = IsCoating ? "Coating Shifts" : "Production Shifts";
        }

        void ShiftWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Would you like to save before exiting?", "", MessageBoxButton.YesNoCancel);
            var success = true;
            if (result == MessageBoxResult.Yes)
            {
                if (IsCoating)
                {
                    success = ShiftHandler.CoatingInstance.SaveShifts();

                }
                else
                {
                    success = ShiftHandler.ProductionInstance.SaveShifts();
                }
                if(success)
                    MessageBox.Show("Changes have been saved.");
                else
                {
                    MessageBox.Show("There was an error while trying to save.");
                    e.Cancel = true;
                }
            }
            else if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        private void AddShiftButton_Click(object sender, RoutedEventArgs e)
        {
            EditShiftWindow editShiftWindow = new EditShiftWindow(IsCoating);
            editShiftWindow.ShowDialog();
            if (editShiftWindow.Accepted)
            {
                if (ShiftCalendar != null) ShiftCalendar.Refresh();
            }
        }

        private void DeleteShiftButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShiftListView.SelectedIndex > -1)
            {
                if (MessageBox.Show("Are you sure you want to delete this shift?", "", MessageBoxButton.YesNo) ==
                    MessageBoxResult.Yes)
                {
                    ShiftHandler.CoatingInstance.Shifts.RemoveAt(ShiftListView.SelectedIndex);
                    if (ShiftCalendar != null) ShiftCalendar.Refresh();
                }
            }
        }

        private void EditShiftButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShiftListView.SelectedIndex >= 0)
            {
                var shift = ShiftListView.SelectedItem as Shift;
                EditShiftWindow editShiftWindow = new EditShiftWindow(IsCoating,shift);
                editShiftWindow.ShowDialog();
                if(editShiftWindow.Accepted == true)
                    if (ShiftCalendar != null) ShiftCalendar.Refresh();
            }
        }

        private void LoadShiftButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (IsCoating)
            {

                bool loaded = ShiftHandler.CoatingInstance.LoadShifts();
                if (loaded)
                {
                    MessageBox.Show("Shifts loaded");
                }
                else
                {
                    bool tryAgain = MessageBox.Show("Load failed. Open save file?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
                    if (tryAgain)
                    {
                        OpenFileDialog dialog = new OpenFileDialog
                        {
                            Title = "Open shift save file.",
                            Multiselect = false
                        };

                        if (dialog.ShowDialog() == true)
                        {
                            if (ShiftHandler.CoatingInstance.LoadShifts(dialog.FileName))
                            {
                                MessageBox.Show("Shifts loaded.");
                            }
                            else
                            {
                                MessageBox.Show("Load failed.");
                            }
                        }
                    }
                }
            }
            else
            {
                bool loaded = ShiftHandler.ProductionInstance.LoadShifts();
                if (loaded)
                {
                    MessageBox.Show("Shifts loaded");
                }
                else
                {
                    bool tryAgain = MessageBox.Show("Load failed. Open save file?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
                    if (tryAgain)
                    {
                        OpenFileDialog dialog = new OpenFileDialog();
                        dialog.Title = "Open shift save file.";
                        dialog.Multiselect = false;

                        if (dialog.ShowDialog() == true)
                        {
                            if (ShiftHandler.ProductionInstance.LoadShifts(dialog.FileName))
                            {
                                MessageBox.Show("Shifts loaded.");
                            }
                            else
                            {
                                MessageBox.Show("Load failed.");
                            }
                        }

                       
                    }
                }
            }
            ShiftCalendar.Refresh();
        }

        private void SaveShiftButton_OnClick(object sender, RoutedEventArgs e)
        {
            if(IsCoating)
            {
                MessageBox.Show(ShiftHandler.CoatingInstance.SaveShifts()
                    ? "Shifts saved"
                    : "There was an error when trying to save shifts");
            }
            else
            {
                MessageBox.Show(ShiftHandler.ProductionInstance.SaveShifts()
                    ? "Shifts saved"
                    : "There was an error in saving your shifts");
            }
        }
    }
}
