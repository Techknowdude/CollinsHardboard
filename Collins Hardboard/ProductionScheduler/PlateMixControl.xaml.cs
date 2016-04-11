using System;
using System.Collections.Generic;
using System.IO;
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

namespace ProductionScheduler
{
    /// <summary>
    /// Interaction logic for PlateMixControl.xaml
    /// </summary>
    public partial class PlateMixControl : UserControl
    {
        public List<Texture> Textures { get { return StaticFactoryValuesManager.TexturesList; } } 
        private Int32 _numChanges;
        private Texture _texture;
        public PlateMixControl(PressWeekControl window)
        {
            InitializeComponent();
            Window = window;
        }

        public PressWeekControl Window { get; set; }

        public Int32 NumChanges
        {
            get { return _numChanges; }
            set { _numChanges = value; }
        }

        public Texture Tex
        {
            get { return _texture; }
            set
            {
                if (value != null)
                {
                    if (TextureComboBox.ItemsSource == null)
                    {
                        TextureComboBox.ItemsSource = Textures;
                    }
                    Int32 index = Textures.FindIndex(x => x.Name == value.Name);
                        TextureComboBox.SelectedIndex = index;
                }
                _texture = value;
            }
        }

        private void TextureComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TextureComboBox.SelectedIndex != -1)
            {
                _texture = TextureComboBox.SelectedItem as Texture;
            }
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            Window.RemovePlateMix(this);
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(NumChanges);
            writer.Write(Tex != null);
            if (Tex != null) Tex.Save(writer);
        }

        public static PlateMixControl Load(BinaryReader reader)
        {
            PlateMixControl newControl = new PlateMixControl(null);

            
            newControl.NumChanges = reader.ReadInt32();
            if(reader.ReadBoolean())
                newControl.Tex = Texture.Load(reader);

            return newControl;
        }
    }
}
