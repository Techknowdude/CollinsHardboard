using System;
using System.IO;
using System.Windows;
using ModelLib;

namespace ScheduleGen
{
    /// <summary>
    /// Interaction logic for WidthControl.xaml
    /// </summary>
    public partial class WidthControl : GenControl
    {
        public static string Type { get { return "WidthControl"; } }
        public WidthControl(ScheduleGenWindow window, int priority = 1)
            : base(window)
        {
            InitializeComponent();
            Priority = priority;
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            ParentWindow.Remove(this);
        }

        public override string ChildType
        {
            get { return Type; }
        }

        /// <summary>
        /// Gets the priority of the passed item. Same thickness is best, thicker is worse, thinner is better. Small changes prefered.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override int GetCost(ProductMasterItem item)
        {
            if (ScheduleGenerator.Instance.LastWidth == item.Width)
            {
                return Priority;
            }
            else if (ScheduleGenerator.Instance.LastWidth > item.Width)
            {
                return (int) ((ScheduleGenerator.Instance.LastWidth/item.Width)*Priority);
            }
            else
            {
                return -(int) ((ScheduleGenerator.Instance.LastWidth/item.Width)*Priority);
            }
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

        public static WidthControl Load(BinaryReader reader, ScheduleGenWindow window)
        {
            int priority = reader.ReadInt32();

            return new WidthControl(window,priority);
        }
    }
}
