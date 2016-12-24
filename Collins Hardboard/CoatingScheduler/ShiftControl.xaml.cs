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
    /// Interaction logic for ShiftControl.xaml
    /// </summary>
    public partial class ShiftControl : UserControl, ICoatingScheduleControl
    {
        // TODO: Abstract out the product and note controls
        public ICoatingScheduleControl ParentControl { get; set; }
        
        private CoatingScheduleShift _shift;
        public CoatingScheduleShift Shift { get { return _shift; } set { _shift = value; } }

        public ObservableCollection<ProductControlBase> ProductControls { get; set; } 

        public ShiftControl(ICoatingScheduleLogic logic)
        {
            ProductControls = new ObservableCollection<ProductControlBase>();
            InitializeComponent();
            ShiftListView.ItemsSource = ProductControls;
            ShiftListView.DataContext = typeof (ProductControlBase);
            this.DataContext = logic;
        }

        private void BtnAddProduct_OnClick(object sender, RoutedEventArgs e)
        {
            Add_Button(sender,e);
        }

        public void Add_Button(object sender, RoutedEventArgs e)
        {
            AddProductWindow addProductWindow = new AddProductWindow();
            addProductWindow.ShowDialog();

            if (!addProductWindow.Accepted) return;
            CoatingScheduleProduct newProduct = new CoatingScheduleProduct(addProductWindow.MasterItem);
            Shift.AddLogic(newProduct);
            var productControl = newProduct.Control as ProductControl;
            if (productControl != null)
            {
                productControl.Machine = addProductWindow.ItemMachine;
                productControl.Config = addProductWindow.Config;
            }

            ((LineControl)ParentControl).ReloadTrackingInfo();
        }

        public void AddControlToBottom(ICoatingScheduleLogic logic)
        {
                ProductControlBase newControl = ProductControlBase.CreateControl(logic);

                ProductControls.Add(newControl);
                logic.Connect(newControl);
                newControl.Connect(this);
        }

        public void AddControlToTop(ICoatingScheduleLogic logic)
        {
            ProductControlBase newControl = ProductControlBase.CreateControl(logic);

            if (newControl != null)
            {
                ProductControls.Insert(0, newControl);
                logic.Connect(newControl);
                newControl.Connect(this);
            }
        }

        public void RemoveControl(ICoatingScheduleControl child)
        {
            ProductControlBase removeControl = (ProductControlBase) child;
            ProductControls.Remove(removeControl);
            
        }

        public void Connect(ICoatingScheduleLogic logic)
        {
            Shift = (CoatingScheduleShift)logic;
            DataContext = Shift;
            UpdateControlData();
        }

        public void Connect(ICoatingScheduleControl parent)
        {
            ParentControl = parent;
        }

        public ICoatingScheduleLogic GetLogic()
        {
            return Shift;
        }

        public void DestroySelf()
        {
            foreach (ProductControlBase control in ProductControls)
            {
                control.DestroySelf();
            }
           ProductControls.Clear();
            Disconnect();

        }

        public void Disconnect()
        {
            Shift = null;
            ParentControl = null;
        }
        
        public ICoatingScheduleControl SwapControlWithBottom(ICoatingScheduleControl newControl)
        {
            // TODO: Instead of swapping, create a new one with the old logic
            ICoatingScheduleControl returnControl = null;

                returnControl = ProductControls.Last();
                ProductControls[ProductControls.Count - 1] = (ProductControlBase)newControl;
            
            return returnControl;
        }

        public ICoatingScheduleControl SwapControlWithTop(ICoatingScheduleControl newControl)
        {
            // TODO: Instead of swapping, create a new one with the old logic
            ICoatingScheduleControl returnControl = null;

                returnControl = ProductControls.First();
                ProductControls[0] = (ProductControlBase)newControl;
           
            return returnControl;
        }

        public void UpdateControlData()
        {
            foreach (var productControlBase in ProductControls)
            {
                productControlBase.UpdateControlData();
            }
        }

        /// <summary>
        /// This function swaps two controls. If the two controls are both children of the same parent, it returns true.
        /// </summary>
        /// <param name="oldControl"></param>
        /// <param name="newControl"></param>
        /// <returns></returns>
        public bool SwapChildControl(ICoatingScheduleControl oldControl, ICoatingScheduleControl newControl)
        {
            bool sameParent = false;

            try
            {
                Int32 oldIndex = ProductControls.IndexOf((ProductControlBase) oldControl);
                Int32 newIndex = ProductControls.IndexOf((ProductControlBase) newControl);

                if (oldIndex >= 0 && newIndex >= 0)
                    sameParent = true;

                if (oldIndex >= 0)
                    ProductControls[oldIndex] = (ProductControlBase) newControl;
                if (newIndex >= 0)
                    ProductControls[newIndex] = (ProductControlBase) oldControl;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return sameParent;
        }

        private void BtnAddNote_OnClick(object sender, RoutedEventArgs e)
        {
            CoatingScheduleNote newNote = new CoatingScheduleNote();
            Shift.AddLogic(newNote);
        }

        public void LoadTrackingInfo()
        {
            for (Int32 index = 0; index < ProductControls.Count; index++)
            {
                var productControlBase = ProductControls[index];
                ProductControl pControl = (ProductControl) ProductControls[index];
                ProductMasterItem product =
                    TrackingSelectionWindow.TrackingItems.FirstOrDefault(
                        x => x.MasterID == pControl.Product.MasterID);
                Int32 column = -1;

                if (product != null)
                    column = TrackingSelectionWindow.TrackingItems.IndexOf(product);

                //((LineControl) ParentControl).AddRunningTotal(productControlBase.LoadTrackingItem(),index +1,column +1);
            }
        }

        public void ReloadTrackingItems()
        {
            ((LineControl)ParentControl).ReloadTrackingInfo();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productMasterItem"></param>
        /// <param name="row">0 indexed</param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public double GetRunningTotal(ProductMasterItem productMasterItem, int row)
        {
            if (ProductControls.Count <= row) return 0;

            var product = ProductControls[row] as ProductControl;

            if (product == null) return 0;
            double temp = 0;

            if (product.Product.DurationType == DurationType.Hours)
            {
                try
                {
                    double unitChange = 0;

                    if (product.Config.ItemInID == productMasterItem.MasterID)
                    {
                        Double.TryParse(product.Product.Units, out temp);
                        //TODO: update all of this....
                        unitChange -= product.Config.GetUnitsConsumed(productMasterItem, temp, null);
                    }
                    if (product.Config.ItemOutID == productMasterItem.MasterID)
                    {
                        Double.TryParse(product.Product.Units, out temp);
                        unitChange += product.Config.ItemsOut * temp;
                    }
                        
                    return unitChange;

                }
                catch (Exception)
                {

                }
            }

            double units = 0;

            if (product.Config.ItemInID == productMasterItem.MasterID)
            {
                Double.TryParse(product.Product.Units,out temp);
                units -= (product.Config.ItemsIn/(double) product.Config.ItemsOut)*temp;
            }
            if (product.Config.ItemOutID == productMasterItem.MasterID)
            {
                Double.TryParse(product.Product.Units, out temp);
                units += temp;
            }

            return units;
        }

        public void GetInvChange()
        {
            foreach (var productControlBase in ProductControls)
            {
                var control = productControlBase as ProductControl;
                if(control != null)
                {
                    control.ComputeInvChange();
                }
            }
        }

    }
}
