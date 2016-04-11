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
using ImportLib;
using StaticHelpers;

namespace Main_Application
{
    /// <summary>
    /// Interaction logic for TextureControl.xaml
    /// </summary>
    public partial class TextureControl : UserControl
    {
        private int _index;

        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                NameText = StaticFactoryValuesManager.TexturesList[value].Name;
            } 
        }

        public String NameText
        {
            get { return StaticFactoryValuesManager.TexturesList[Index].Name; }
            set
            {
                StaticFactoryValuesManager.TexturesList[Index].Name = value;
            }
        }

        public PlantSettingsWindow ParentWindow { get; set; }
        
        public TextureControl(int index, PlantSettingsWindow parentWindow)
        {
            InitializeComponent();

            Index = index;
            ParentWindow = parentWindow;
        }

        public void UpdateControlInfo()
        {
            LineNameTextBox.Text = NameText;
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            ParentWindow.Remove(this);
        }
    }
}
