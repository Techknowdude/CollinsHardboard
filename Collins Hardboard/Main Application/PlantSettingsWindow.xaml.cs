using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;
using ImportLib;
using StaticHelpers;

namespace Main_Application
{
    /// <summary>
    /// Interaction logic for PlantSettingsWindow.xaml
    /// </summary>
    public partial class PlantSettingsWindow : Window
    {

        #region Fields
        private ObservableCollection<CoatingLineControl> _coatingLineControls = new ObservableCollection<CoatingLineControl>();
        private ObservableCollection<TextureControl> _textureControls = new ObservableCollection<TextureControl>(); 
        #endregion

        #region Properties

        public ObservableCollection<CoatingLineControl> CoatingLineControls
        {
            get { return _coatingLineControls; }
            set { _coatingLineControls = value; }
        }

        public char WiPChar
        {
            get { return StaticInventoryTracker.WiPMarker; }
            set { StaticInventoryTracker.WiPMarker = value; }
        }

        public string CurrentWaste
        {
            get { return StaticFactoryValuesManager.CurrentWaste.ToString("N"); }
            set
            {
                double data;
                if (Double.TryParse(value, out data))
                {
                    StaticFactoryValuesManager.CurrentWaste = data;
                }
            }
        }

        public String WasteMin
        {
            get { return StaticFactoryValuesManager.WasteMin.ToString("N"); }
            set
            {
                double data;
                if (Double.TryParse(value, out data))
                {
                    StaticFactoryValuesManager.WasteMin = data;
                }
            }
        }
        public String WasteMax
        {
            get { return StaticFactoryValuesManager.WasteMax.ToString("N"); }
            set
            {
                double data;
                if (Double.TryParse(value, out data))
                {
                    StaticFactoryValuesManager.WasteMax = data;
                }
            }
        }

        public ObservableCollection<GradeControl> GradeControls { get; set; }
        public List<Texture> Textures { get { return StaticFactoryValuesManager.TexturesList; } }

        public ObservableCollection<String> CoatingLines { get { return StaticFactoryValuesManager.CoatingLines; } }

        public ObservableCollection<TextureControl> TextureControls
        {
            get { return _textureControls; }
            set { _textureControls = value; }
        }

        #endregion

        public PlantSettingsWindow()
        {
            GradeControls = new ObservableCollection<GradeControl>();
            InitializeComponent();
            DataContext = this;

            for (int index = 0; index < StaticFactoryValuesManager.CoatingLines.Count; index++)
            {
                var coatingLine = StaticFactoryValuesManager.CoatingLines[index];
                CoatingLineControls.Add(new CoatingLineControl(index,this));
            }
            Closing += PlantSettingsWindow_Closing;

            UpdateControlInfo();
        }

        void PlantSettingsWindow_Closing(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show("Save any changes?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (StaticFactoryValuesManager.SaveValues()) MessageBox.Show("Save successful");
                else
                {
                    e.Cancel = true;
                }

            }
        }

        private void UpdateControlInfo()
        {
            GradeControls.Clear();
            for (int index = 0; index < StaticFactoryValuesManager.GradesList.Count; index++)
            {
                var grade = StaticFactoryValuesManager.GradesList[index];
                var abbr = StaticFactoryValuesManager.GradeAbbrList[index];
                GradeControls.Add(new GradeControl(grade,abbr,index));
            }
            UpdateLineControls();
            UpdateTexControls();

            WasteMaxTextBox.Text = WasteMax;
            WasteMinTextBox.Text = WasteMin;
            WasteCurrentTextBox.Text = CurrentWaste;
        }


        private void AddGradeButton_OnClick(object sender, RoutedEventArgs e)
        {
            StaticFactoryValuesManager.GradesList.Add("");
            StaticFactoryValuesManager.GradeAbbrList.Add("");

            UpdateControlInfo();
        }

        private void DeleteGradeButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (GradeListView.SelectedIndex != -1)
            {
                StaticFactoryValuesManager.GradesList.RemoveAt(GradeListView.SelectedIndex);
                StaticFactoryValuesManager.GradeAbbrList.RemoveAt(GradeListView.SelectedIndex);

                UpdateControlInfo();
            }
        }

        private void AddTexButton_OnClick(object sender, RoutedEventArgs e)
        {
            StaticFactoryValuesManager.TexturesList.Add(new Texture(""));
            UpdateTexControls();
        }

        private void AddCoatingButton_OnClick(object sender, RoutedEventArgs e)
        {
            AddCoatingBox();
        }


        private void SaveItem_OnClick(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(AddCoatingButton); //force change focus to save current entry
            StaticFactoryValuesManager.SaveValues();
        }

        private void LoadItem_OnClick(object sender, RoutedEventArgs e)
        {
            StaticFactoryValuesManager.LoadValues();
            UpdateControlInfo();
        }

        private void AddCoatingBox()
        {
            CoatingLines.Add("");
            CoatingLineControls.Add(new CoatingLineControl(CoatingLines.Count-1, this));
        }

        public void Remove(CoatingLineControl coatingLineControl)
        {
            StaticFactoryValuesManager.CoatingLines.RemoveAt(coatingLineControl.Index);
            CoatingLineControls.RemoveAt(coatingLineControl.Index);
            UpdateLineControls();
        }

        private void UpdateLineControls()
        {
            for (int index = 0; index < StaticFactoryValuesManager.CoatingLines.Count; index++)
            {
                if(CoatingLineControls.Count == index) // for when there are not enough
                    CoatingLineControls.Add(new CoatingLineControl(index,this));
                else // update existing
                {
                    var coatingLineControl = CoatingLineControls[index];
                    coatingLineControl.Index = index;
                }
                CoatingLineControls[index].UpdateControlInfo();
            }
        }

        public void Remove(TextureControl texControl)
        {
            StaticFactoryValuesManager.TexturesList.RemoveAt(texControl.Index);
            TextureControls.RemoveAt(texControl.Index);
            UpdateTexControls();
        }

        private void UpdateTexControls()
        {
            for (int index = 0; index < StaticFactoryValuesManager.TexturesList.Count; index++)
            {
                if (TextureControls.Count == index) // for when there are not enough
                    TextureControls.Add(new TextureControl(index, this));
                else // update existing
                {
                    var texControl = TextureControls[index];
                    texControl.Index = index;
                }
                TextureControls[index].UpdateControlInfo();
            }
        }
    }
}
