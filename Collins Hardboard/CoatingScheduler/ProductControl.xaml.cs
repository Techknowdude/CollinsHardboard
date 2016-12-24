#define USE_PROPERTY

using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Configuration_windows;
using ImportLib;
using ModelLib;
using Brushes = System.Windows.Media.Brushes;

namespace CoatingScheduler
{

    /// <summary>
    /// Interaction logic for ProductControl.xaml
    /// </summary>
    [Serializable]
    public partial class ProductControl : ProductControlBase
    {
        private string _configName;
        private string _machineName;

        #region Properties

        public override Machine Machine
        {
            get
            {
                return Product.Machine;
            }
            set
            {
                BorderBrush = value == null ? Brushes.Red : Brushes.Blue;
                Product.Machine = value;
                MachineName = value?.Name;
            }
        }

        public override Configuration Config
        {
            get
            {
                    return Product.Config;
            }
            set
            {
                BorderBrush = value == null ? Brushes.Red : Brushes.Blue;
                Product.Config = value;
                ConfigName = value?.Name;
            }
        }

        public string ConfigName
        {
            get { return _configName; }
            set { _configName = value; }
        }

        public CoatingScheduleProduct Product { get; set; }
        public override ICoatingScheduleControl ParentControl { get; set; }
        

        #endregion

        public new static ProductControl CreateControl(ICoatingScheduleLogic logic)
        {
            return new ProductControl(logic);
        }

        public void TextUpdated(object sender, TextChangedEventArgs e)
        {
            try
            {
                Product.Units = MtbUnits.Text;
                if(ParentControl != null)
                    ((ShiftControl)ParentControl).ReloadTrackingItems();


            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private ProductControl(ICoatingScheduleLogic logic)
        {
            Product = logic as CoatingScheduleProduct;
            InitializeComponent();
            CbbBarcode.SelectionChanged += CbbBarcode_SelectionChanged;
            DataContext = logic;

            SetMachineRef();
        }

        private void SetMachineRef()
        {
            try
            {
                if (Product.MasterID != 0)
                {
                    Machine =
                        MachineHandler.Instance.MachineList.FirstOrDefault(
                            machine =>
                                machine.Name.Equals(MachineName));
                    if (Machine != null)
                    {
                        bool found = false;
                        for (int groupIndex = 0;!found && groupIndex < Machine.ConfigurationList.Count; groupIndex++)
                        {
                            var configurationGroup = Machine.ConfigurationList[groupIndex];
                            for (int configIndex = 0; !found && configIndex < configurationGroup.Configurations.Count; configIndex++)
                            {
                                var configuration = configurationGroup.Configurations[configIndex];
                                if (configuration.Name.Equals(ConfigName))
                                {
                                    Config = configuration;
                                    found = true;
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
            }
        }

        public string MachineName
        {
            get { return _machineName; }
            set { _machineName = value; }
        }

        private void BtnRemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            if (Product != null)
            {
                Product.DestroySelf();
                ((ShiftControl)ParentControl).ReloadTrackingItems();
            }
        }

        private void BtnPushUp_Click(object sender, RoutedEventArgs e)
        {
            if (Product != null)
            {
                Product.PushUp();
                ((ShiftControl)ParentControl).ReloadTrackingItems();
            }
        }

        private void BtnPushDown_OnClick(object sender, RoutedEventArgs e)
        {
            if (Product != null)
            {
                Product.PushDown();
                ((ShiftControl)ParentControl).ReloadTrackingItems();
            }
        }

        private void BtnSwapUp_OnClick(object sender, RoutedEventArgs e)
        {
            if (Product != null)
            {
                Product.SwapUp();
                ((ShiftControl)ParentControl).ReloadTrackingItems();
            }
        }

        private void BtnSwapDown_OnClick(object sender, RoutedEventArgs e)
        {
            if (Product != null)
            {
                Product.SwapDown();
                ((ShiftControl)ParentControl).ReloadTrackingItems();
            }
        }

        private void CbbBarcode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Product != null)
                Product.HasBarcode = CbbBarcode.SelectedIndex == 0;
        }

 

        public override void Add_Button(object sender, RoutedEventArgs e)
        {
            if (Product != null)
            {
                Product.AddLogic();
                ((ShiftControl)ParentControl).ReloadTrackingItems();
            }
        }

        public override void AddControlToBottom(ICoatingScheduleLogic logic)
        {
            throw new NotImplementedException();
        }

        public override void AddControlToTop(ICoatingScheduleLogic logic)
        {
            throw new NotImplementedException();
        }

        public override void RemoveControl(ICoatingScheduleControl child)
        {
            throw new NotImplementedException();
        }

        public override void Connect(ICoatingScheduleLogic logic)
        {
            Product = (CoatingScheduleProduct)logic;
            DataContext = Product;
            UpdateControlData();
        }

        public override void Connect(ICoatingScheduleControl parent)
        {
            ParentControl = parent;
        }

        public override ICoatingScheduleLogic GetLogic()
        {
            return Product;
        }

        public override void DestroySelf()
        {
            Disconnect();
        }

        public override void Disconnect()
        {
            Product = null;
            ParentControl = null;
        }

        public override ICoatingScheduleControl SwapControlWithBottom(ICoatingScheduleControl newControl)
        {
            throw new NotImplementedException();
        }

        public override ICoatingScheduleControl SwapControlWithTop(ICoatingScheduleControl newControl)
        {
            throw new NotImplementedException();
        }

        public override void UpdateControlData()
        {
            if (Product == null) return;
            TxbProductDescription.Text = Product.ProductCode;
            TxbGrades.Text = Product.Grades;
            MtbUnits.Text = Product.Units;
            TxbNotes.Text = Product.Notes;
            //CbxUnits.IsChecked = Product.HasUnits;
            //CbxPlacement.IsChecked = Product.HasPlacement;
            TxbPlacement.Text =  Product.Placement;

            CbbBarcode.SelectedIndex = Product.HasBarcode ? 0 : 1;
            CbbMarked.SelectedIndex = Product.HasBackbrand ? 0 : 1;
            PaintTrialComboBox.SelectedIndex = Product.IsTrial ? 1 : 0;
            DurationComboBox.SelectedIndex = Product.DurationType == DurationType.Units ? 0 : 1;
        }

        private void DurationComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Product.DurationType = DurationComboBox.SelectedIndex == 0 ? DurationType.Units : DurationType.Hours;

            if (ParentControl != null)
                ((ShiftControl)ParentControl).ReloadTrackingItems();

        }

        private void CbbMarked_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (Product != null)
                Product.HasBackbrand = CbbMarked.SelectedIndex == 0;
        }

        public void ComputeInvChange()
        {
            // ask user how much was made
            UnitChangeWindow window = new UnitChangeWindow(Product.ProductCode,Product.Units);
            window.ShowDialog();

            // get expected units
            double unitsExpected = Double.Parse(Product.Units);
            // add the change to tracker
            StaticInventoryTracker.InventoryChanges.Add(new InventoryChange(unitsExpected,window.Units,Product.MasterID,((LineControl)((ShiftControl)ParentControl).ParentControl).Line.Date));

            if (Config != null)
            {
                ProductMasterItem consumedItem =
                    StaticInventoryTracker.ProductMasterList.FirstOrDefault(
                        master => master.MasterID == Product.MasterID);
                ProductMasterItem createdItem =
                    StaticInventoryTracker.ProductMasterList.FirstOrDefault(
                        master => master.MasterID == Product.MasterID);

                if (consumedItem != null && createdItem != null)
                {
                    double unitsConsumed = Config.GetUnitsConsumed(consumedItem, window.Units,createdItem);
                    // remove units from inventory
                    var invItem =
                        StaticInventoryTracker.InventoryItems.FirstOrDefault(
                            item => item.MasterID == consumedItem.MasterID);
                    if (invItem != null)
                    {
                        invItem.Units -= unitsConsumed;
                    }
                    else // start tracking it
                    {
                        StaticInventoryTracker.AddInventoryItem(new InventoryItem(consumedItem.ProductionCode,-unitsConsumed,consumedItem.PiecesPerUnit,"WiP",consumedItem.MasterID));
                    }
                }
            }
        }
    }

    public enum DurationType
    {
        Units,
        Hours
    }
}
