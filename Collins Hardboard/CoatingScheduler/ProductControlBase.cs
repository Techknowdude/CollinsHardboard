using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Configuration_windows;
using ModelLib;

namespace CoatingScheduler
{
    /// <summary>
    /// Interaction logic for ProductControlBase.xaml
    /// </summary>
    public abstract class ProductControlBase : UserControl, ICoatingScheduleControl
    {
        public abstract Machine Machine { get; set; }
        public abstract Configuration Config { get; set; }
        public abstract ICoatingScheduleControl ParentControl { get; set; }

        public static ProductControlBase CreateControl(ICoatingScheduleLogic logic)
        {
            ProductControlBase newControl = null;

            if (logic.GetType() == typeof (CoatingScheduleProduct))
            {
                newControl = ProductControl.CreateControl(logic);
            }
            else if (logic.GetType() == typeof (CoatingScheduleNote))
            {
                newControl = ProductNoteControl.CreateControl(logic);
            }
            else
            {
                throw new Exception("Cannot create class from passed logic.");
            }

            return newControl;
        }

        protected ProductControlBase()
        {
            
        }

        public abstract void Add_Button(object sender, RoutedEventArgs e);
        public abstract void AddControlToBottom(ICoatingScheduleLogic logic);
        public abstract void AddControlToTop(ICoatingScheduleLogic logic);
        public abstract void RemoveControl(ICoatingScheduleControl child);
        public abstract void Connect(ICoatingScheduleLogic logic);
        public abstract void Connect(ICoatingScheduleControl parent);
        public abstract ICoatingScheduleLogic GetLogic();
        public abstract void DestroySelf();
        public abstract void Disconnect();
        public abstract ICoatingScheduleControl SwapControlWithBottom(ICoatingScheduleControl newControl);
        public abstract ICoatingScheduleControl SwapControlWithTop(ICoatingScheduleControl newControl);
        public abstract void UpdateControlData();
        public bool SwapChildControl(ICoatingScheduleControl oldControl, ICoatingScheduleControl newControl)
        {
            throw new NotImplementedException();
        }

        public abstract string LoadTrackingItem();
    }
}