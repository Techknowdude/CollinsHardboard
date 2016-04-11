using System.Windows;
using System.Windows.Controls;

namespace CoatingScheduler
{
    /// <summary>
    /// Interaction logic for InstructionSetControl.xaml
    /// </summary>
    public partial class InstructionSetControl : UserControl
    {
        private CoatingLineInstructionSet logicControl;
        public InstructionSetControl(CoatingLineInstructionSet logic)
        {
            logicControl = logic;
            logic.Control = this;
            InitializeComponent();
            InstructionListView.ItemsSource = logic.InstructionsCollection;
            InstructionListView.DataContext = logic.InstructionsCollection;
            CoatingLineText.Text = logic.CoatingLine;
        }

        private void AddInstructionButton_OnClick(object sender, RoutedEventArgs e)
        {
            logicControl.AddInstruction();
        }

        private void DeleteInstructionButton_OnClick(object sender, RoutedEventArgs e)
        {
            logicControl.DeleteInstruction();
        }
    }
}
