using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using StaticHelpers;

namespace Main_Application
{
    /// <summary>
    /// Interaction logic for CoatingLineControl.xaml
    /// </summary>
    public partial class CoatingLineControl : UserControl
    {
        private int _index;

        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                NameText = StaticFactoryValuesManager.CoatingLines[value];
            } 
        }

        public String NameText
        {
            get { return StaticFactoryValuesManager.CoatingLines[Index]; }
            set
            {
                StaticFactoryValuesManager.CoatingLines[Index] = value;
            }
        }

        public PlantSettingsWindow ParentWindow { get; set; }

        public CoatingLineControl(int index, PlantSettingsWindow parentWindow)
        {
            InitializeComponent();

            Index = index;
            ParentWindow = parentWindow;
        }


        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            ParentWindow.Remove(this);
        }

        public void UpdateControlInfo()
        {
            LineNameTextBox.Text = NameText;
        }
    }
}
