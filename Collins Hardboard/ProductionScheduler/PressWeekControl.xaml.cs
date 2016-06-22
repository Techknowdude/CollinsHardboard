using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Configuration_windows.Annotations;
using ImportLib;
using ModelLib;
using WarehouseManager;

namespace ProductionScheduler
{
    /// <summary>
    /// Interaction logic for PressDayControl.xaml
    /// </summary>
    public partial class PressWeekControl : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<PressItemControl> _controlsList = new ObservableCollection<PressItemControl>();
        private DateTime _week;
        private ObservableCollection<PlateMixControl> _mixControls = new ObservableCollection<PlateMixControl>();
        private string _availablePlatesText;
        private int _availablePlates;

        public ObservableCollection<PlateMixControl> MixControls
        {
            get { return _mixControls; }
            set { _mixControls = value; }
        }

        public int AvailablePlates
        {
            get { return _availablePlates; }
            set
            {
                _availablePlates = value;
                OnPropertyChanged();
                AvailablePlatesText = $"Available plates: {value}";
            }
        }

        public String WeekTitle { get { return _week.ToString("dddd M/dd/yy"); } }

        public String AvailablePlatesText
        {
            get { return _availablePlatesText; }
            set
            {
                _availablePlatesText = value;
                OnPropertyChanged(); // will this send the property name?
            }
        }

        public DateTime Week
        {
            get { return _week; }
            set
            {
                _week = value;
            }
        }

        public ObservableCollection<PressItemControl> ControlsList
        {
            get { return _controlsList; }
            set
            {
                _controlsList = value;
                PressItemView.ItemsSource = value;
            }
        }

        public PressWeekControl(DateTime week, PressScheduleWindow window)
        {
            InitializeComponent();
            DataContext = this;

            Schedule = window;
            Week = week;
            AvailablePlates = PressManager.Instance.NumPlates;
        }

        public PressWeekControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void AddItemToTop(PressItem item)
        {
            _controlsList.Insert(0,new PressItemControl(item,this));
        }

        public void AddItemToBottom(PressItem item)
        {
            _controlsList.Add(new PressItemControl(item,this));
        }

        
        public bool RemoveItem(PressItem item)
        {
            PressItemControl found =
                ControlsList.FirstOrDefault(control => control != null && ((PressItemControl) control).Item == item) as PressItemControl;
            if (found == null) return false;

            ControlsList.Remove(found);
            return true;
        }

        public bool RemoveItem(Int32 index)
        {
            if (ControlsList.Count == 0 || index < 0 || ControlsList.Count <= index) return false;

            ControlsList.RemoveAt(index);
            return true;
        }

        public void MoveUp(PressItemControl pressItemControl)
        {
            return;
            if (Equals(pressItemControl, _controlsList[0]))
            {
                //Schedule.MoveUp(pressItemControl, this);
            }
            else
            {
                Int32 index = ControlsList.IndexOf(pressItemControl);

                ControlsList.RemoveAt(index);

                ControlsList.Insert(index - 1,pressItemControl);
            }
        }

        public PressScheduleWindow Schedule { get; set; }

        public void AddItemToBottom(PressItemControl pressItemControl)
        {
            _controlsList.Add(pressItemControl);
            pressItemControl.WeekControl = this;
        }

        public void AddItemToTop(PressItemControl pressItemControl)
        {
            _controlsList.Insert(0,pressItemControl);
            pressItemControl.WeekControl = this;
        }

        public void MoveDown(PressItemControl pressItemControl)
        {
            return;
            if (Equals(pressItemControl, _controlsList.Last()))
            {
                //Schedule.MoveDown(pressItemControl, this);
            }
            else
            {
                Int32 index = ControlsList.IndexOf(pressItemControl);

                ControlsList.RemoveAt(index);

                ControlsList.Insert(index + 1, pressItemControl);
            }
        }

        public void RemoveItem(PressItemControl pressItemControl)
        {
            ControlsList.Remove(pressItemControl);
        //    if (ControlsList.Count == 0)
        //        Schedule.WeekControls.Remove(this);
        }

        private void AddMixButton_OnClick(object sender, RoutedEventArgs e)
        {
            _mixControls.Add(new PlateMixControl(this));
        }
        public void RemovePlateMix(PlateMixControl plateMixControl)
        {
            _mixControls.Remove(plateMixControl);
        }


        public void Save(BinaryWriter writer)
        {
            writer.Write(Week.ToLongTimeString());
            writer.Write(ControlsList.Count);
            foreach (var pressItemControl in ControlsList)
            {
                pressItemControl.Save(writer);
            }

            writer.Write(MixControls.Count);
            foreach (var plateMixControl in MixControls)
            {
                plateMixControl.Save(writer);
            }
        }

        public PressWeekControl(BinaryReader reader)
        {

            InitializeComponent();
            DataContext = this;
            AvailablePlates = PressManager.Instance.NumPlates;

            Week = DateTime.Parse(reader.ReadString());
            Int32 numControls = reader.ReadInt32();
            for (; numControls > 0; --numControls)
            {
                PressItemControl control = PressItemControl.Load(reader);
                control.WeekControl = this;
                ControlsList.Add(control);
            }

            MixControls.Clear();
            Int32 plates = reader.ReadInt32();
            for (; plates > 0; --plates)
            {
                PlateMixControl control = PlateMixControl.Load(reader);
                control.Window = this;
                MixControls.Add(control);
                AvailablePlates -= control.NumChanges;
            }

        }

        private void DeleteWeekButton_OnClick(object sender, RoutedEventArgs e)
        {
            //if(MessageBox.Show("Are you sure?","",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
              //  PressScheduleWindow.WeekControls.Remove(this);
        }

        private void AddItemButton_OnClick(object sender, RoutedEventArgs e)
        {
            NewPressItemWindow window = new NewPressItemWindow();
            if (window.ShowDialog() == true)
            {
                AddItemToBottom(new PressItemControl(window.CreatedItem, this));
            }

        }

        private void RunWeekButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Run week and add to inventory?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {

                foreach (var pressItemControl in ControlsList)
                {

                    if (pressItemControl.Item.Product1 != null)
                    {
                        var inv =
                            StaticInventoryTracker.WiPItems.FirstOrDefault(x => x.MasterID == pressItemControl.Item.Product1.MasterID);
                        if (inv != null)
                        {
                            StaticInventoryTracker.WiPItems.FirstOrDefault(x => x.MasterID == pressItemControl.Item.Product1.MasterID).Units = inv.Units + pressItemControl.NumShifts * 8 * pressItemControl.Item.Product1.UnitsPerHour;
                        }
                        else
                        {
                            StaticInventoryTracker.WiPItems.Add(new InventoryItem(pressItemControl.Item.Product1.ProductionCode,
                                pressItemControl.NumShifts * 8 * pressItemControl.Item.Product1.UnitsPerHour, pressItemControl.Item.Product1.PiecesPerUnit,
                                "WiP", pressItemControl.Item.Product1.MasterID));
                        }
                    }
                }
                WiPInventoryWindow.UpdateControls();
                //PressScheduleWindow.WeekControls.Remove(this);

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
