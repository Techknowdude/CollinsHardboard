using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StaticHelpers;

namespace InventoryManager
{
    public partial class ForcastWindow : Form
    {
        public ForcastWindow()
        {
            InitializeComponent();
            bindingSource1.DataSource = StaticInventoryTracker.ForcastItems;
        }
    }
}
