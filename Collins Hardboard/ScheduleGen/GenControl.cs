using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ModelLib;

namespace ScheduleGen
{
    public abstract class GenControl : UserControl
    {
        private int _priority;

        public int Priority
        {
            get { return _priority; }
            set
            {
                _priority = value;
                ParentWindow.UpdateControlOrder();
            }
        }


        public String PriorityText
        {
            get { return Priority.ToString("D"); }
            set
            {
                int data;
                if (Int32.TryParse(value, out data))
                    Priority = data;
            }
        }
        protected ScheduleGenWindow ParentWindow { get; set; }
        public abstract string ChildType { get; }
        public static GenControl Instance { get; set; }

        public abstract int GetCost(ProductMasterItem item);

        protected GenControl(ScheduleGenWindow parent)
        {
            ParentWindow = parent;
        }

        public abstract bool Save(BinaryWriter writer);

        public static GenControl LoadControl(BinaryReader reader, ScheduleGenWindow window)
        {
            GenControl control = null;
            var type = reader.ReadString();
            if (type == LineControl.Type)
            {
                control = LineControl.Load(reader, window);
            }
            else if (type == RunBeforeControl.Type)
            {
                control = RunBeforeControl.Load(reader, window);
            }
            else if (type == TextureControl.Type)
            {
                control = TextureControl.Load(reader, window);
            }
            else if (type == WasteControl.Type)
            {
                control = WasteControl.Load(reader, window);
            }
            else if (type == SalesNumbersControl.Type)
            {
                control = SalesNumbersControl.Load(reader,window);
            }
            else if (type == SalesPrediction.Type)
            {
                control = SalesPrediction.Load(reader,window);
            }
            else if (type == PurgeWiPControl.Type)
            {
                control = PurgeWiPControl.Load(reader, window);
            }
            else if (type == WidthControl.Type)
            {
                control = WidthControl.Load(reader, window);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Type of control not recognized");
            }

            return control;
        }
    }

}
