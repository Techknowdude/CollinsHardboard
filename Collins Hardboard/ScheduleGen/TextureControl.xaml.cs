using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ImportLib;
using ModelLib;
using StaticHelpers;

namespace ScheduleGen
{
    /// <summary>
    /// Interaction logic for TextureControl.xaml
    /// </summary>
    public partial class TextureControl : GenControl
    {
        private Texture _selectedTexture;
        private bool toggle = true;
        public static String Type
        {
            get
            {
                return "TextureControl";
            }
        }

        public Texture SelectedTexture
        {
            get { return _selectedTexture; }
            set
            {

                Texture texture = value;

                if (value != null)
                {
                    texture = StaticFactoryValuesManager.TexturesList.FirstOrDefault(x => x.Name == value.Name);

                    int index = StaticFactoryValuesManager.TexturesList.IndexOf(texture);

                    toggle = false;
                    TexComboBox.SelectedIndex = index;
                    toggle = true;
                }
                _selectedTexture = texture;
            }
        }

        public bool OnlyOn { get; set; }
        public DayOfWeek Day { get; set; }

        public TextureControl(ScheduleGenWindow parent, DayOfWeek day = default(DayOfWeek), bool onlyOn = false, Texture tex = null, int priority = 1) : base(parent)
        {
            InitializeComponent();
            DataContext = this;

            TexComboBox.ItemsSource = StaticFactoryValuesManager.TexturesList;

            Day = day;
            OnlyOn = onlyOn;
            SelectedTexture = tex;
            Priority = priority;

            DayComboBox.SelectedIndex = (int) Day;
        }

        
        private void TexComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(toggle)
            {
                if (TexComboBox.SelectedIndex != -1)
                {
                    SelectedTexture = TexComboBox.SelectedItem as Texture;
                }
                else
                {
                    SelectedTexture = null;
                }
            }
        }

        private void WhenComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WhenComboBox.SelectedIndex != -1)
            {
                OnlyOn = WhenComboBox.SelectedIndex == 0;
            }
        }

        private void DayComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DayComboBox.SelectedIndex != -1)
            {
                Day = (DayOfWeek) DayComboBox.SelectedIndex;
            //    switch (DayComboBox.SelectedIndex)
            //    {
            //        case 0: // Monday
            //            Day = DayOfWeek.Monday;
            //            break;
            //        case 1:
            //            Day = DayOfWeek.Tuesday;
            //            break;
            //        case 2:
            //            Day = DayOfWeek.Wednesday;
            //            break;
            //        case 3:
            //            Day = DayOfWeek.Thursday;
            //            break;
            //        case 4:
            //            Day = DayOfWeek.Friday;
            //            break;
            //        case 5:
            //            Day = DayOfWeek.Saturday;
            //            break;
            //        case 6:
            //            Day = DayOfWeek.Sunday;
            //            break;
            //    }
            }
        }

        public override bool Save(BinaryWriter writer)
        {
            try
            {
                writer.Write(Type);
                writer.Write((int)Day);
                writer.Write(OnlyOn);
                SelectedTexture.Save(writer);
                writer.Write(Priority);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override string ChildType
        {
            get { return Type; }
        }

        public override int GetCost(ProductMasterItem item)
        {
            if (OnlyOn)
            {
                if (item.Texture.Equals(SelectedTexture) && ScheduleGenerator.Instance.CurrentDay.DayOfWeek == Day)
                    return Priority;
            }
            else
            {
                if (item.Texture.Equals(SelectedTexture) && ScheduleGenerator.Instance.CurrentDay.DayOfWeek != Day)
                    return Priority;
            }


            return 0;
        }

        public static GenControl Load(BinaryReader reader, ScheduleGenWindow window)
        {
            DayOfWeek day = (DayOfWeek)reader.ReadInt32();
            bool onlyOn = reader.ReadBoolean();
            Texture tex = Texture.Load(reader);
            int priority = reader.ReadInt32();
            return new TextureControl(window,day,onlyOn,tex,priority);
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            ParentWindow.Remove(this);
        }
    }
}
