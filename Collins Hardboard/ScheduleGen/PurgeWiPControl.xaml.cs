using System;
using System.IO;
using System.Linq;
using System.Windows;
using ImportLib;
using ModelLib;

namespace ScheduleGen
{
    /// <summary>
    /// Interaction logic for PurgeWiPControl.xaml
    /// </summary>
    public partial class PurgeWiPControl : GenControl
    {
        public static String Type {get { return "WiPPurge"; }}

        public PurgeWiPControl(ScheduleGenWindow window, int priority = 1) :base(window)
        {
            InitializeComponent();

            Priority = priority;
        }

        public override string ChildType
        {
            get { return Type; }
        }

        public override int GetCost(ProductMasterItem item)
        {
            return StaticInventoryTracker.WiPItems.Any(x => x.IsPurged && x.MasterID == item.MasterID) ? Priority : 0;
        }

        public override bool Save(BinaryWriter writer)
        {
            try
            {
                writer.Write(Type);
                writer.Write(Priority);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            ParentWindow.Remove(this);
        }

        public static GenControl Load(BinaryReader reader, ScheduleGenWindow window)
        {
            int priority = reader.ReadInt32();

            return new PurgeWiPControl(window,priority);
        }
    }
}
