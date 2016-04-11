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
        private static ObservableCollection<ProductMasterItem> _pressRoughList = new ObservableCollection<ProductMasterItem>();
        static ObservableCollection<DayOfWeek> _changeDays = new ObservableCollection<DayOfWeek>();
        private static Int32 _numChanges;

        public static ObservableCollection<ProductMasterItem> PressRoughList
        {
            get { return _pressRoughList; }
            set { _pressRoughList = value; }
        }

        public ObservableCollection<ProductMasterItem> RoughList { get { return StaticInventoryTracker.ProductMasterList; } }

        public static ObservableCollection<DayOfWeek> ChangeDays
        {
            get { return _changeDays; }
            set { _changeDays = value; }
        }

        public static Int32 NumChanges
        {
            get { return _numChanges; }
            set { _numChanges = value; }
        }

        public PressSettingsWindow()
        {
            InitializeComponent();
            DataContext = this;
            Closing += PressSettingsWindow_Closing;
            Load();
            SetControlData();
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
            if ( result ==
                MessageBoxResult.Yes)
            {
                if (Save())
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

        public static bool Save()
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(new FileStream("pressSettings.dat",FileMode.OpenOrCreate)))
                {
                    writer.Write(NumChanges);
                    writer.Write(ChangeDays.Count);
                    foreach (var changeDay in ChangeDays)
                    {
                        writer.Write((Int32)changeDay);
                    }
                    writer.Write(PressRoughList.Count);
                    foreach (var productMasterItem in PressRoughList)
                    {
                        productMasterItem.Save(writer);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool Load()
        {
            try
            {
                using (BinaryReader reader = new BinaryReader(new FileStream("pressSettings.dat", FileMode.Open)))
                {
                    NumChanges = reader.ReadInt32();
                    ChangeDays.Clear();
                    for (Int32 i = reader.ReadInt32(); i > 0; i--)
                    {
                        ChangeDays.Add((DayOfWeek)reader.ReadInt32());
                    }

                    PressRoughList.Clear();
                    for (Int32 i = reader.ReadInt32(); i > 0; i--)
                    {
                        PressRoughList.Add(ProductMasterItem.Load(reader));
                    }
                }

            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void AddRoughItemButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (RoughItemsComboBox.SelectedIndex != -1)
            {
                if (!PressRoughList.Contains(RoughItemsComboBox.SelectedItem))
                {
                    PressRoughList.Add((ProductMasterItem) RoughItemsComboBox.SelectedItem);
                }
            }
        }

        private void RemoveRoughItemButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (RoughItemListView.SelectedIndex != -1)
            {
                PressRoughList.RemoveAt(RoughItemListView.SelectedIndex);
            }
        }

        private void MondayCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            if(MondayCheckBox.IsChecked == true)
                ChangeDays.Add(DayOfWeek.Monday);
            else
            {
                ChangeDays.Remove(DayOfWeek.Monday);
            }
        }

        private void TuesdayCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {

            if (TuesdayCheckBox.IsChecked == true)
                ChangeDays.Add(DayOfWeek.Tuesday);
            else
            {
                ChangeDays.Remove(DayOfWeek.Tuesday);
            }
        }

        private void WednesdayCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {

            if (WednesdayCheckBox.IsChecked == true)
                ChangeDays.Add(DayOfWeek.Wednesday);
            else
            {
                ChangeDays.Remove(DayOfWeek.Wednesday);
            }
        }

        private void ThursdayCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {

            if (ThursdayCheckBox.IsChecked == true)
                ChangeDays.Add(DayOfWeek.Thursday);
            else
            {
                ChangeDays.Remove(DayOfWeek.Thursday);
            }
        }

        private void FridayCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {

            if (FridayCheckBox.IsChecked == true)
                ChangeDays.Add(DayOfWeek.Friday);
            else
            {
                ChangeDays.Remove(DayOfWeek.Friday);
            }
        }

        private void SaturdayCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {

            if (SaturdayCheckBox.IsChecked == true)
                ChangeDays.Add(DayOfWeek.Saturday);
            else
            {
                ChangeDays.Remove(DayOfWeek.Saturday);
            }
        }

        private void SundayCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {

            if (SundayCheckBox.IsChecked == true)
                ChangeDays.Add(DayOfWeek.Sunday);
            else
            {
                ChangeDays.Remove(DayOfWeek.Sunday);
            }
        }
    }
}
