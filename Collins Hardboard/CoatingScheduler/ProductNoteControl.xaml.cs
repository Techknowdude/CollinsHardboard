using System;
using System.Windows;
using Configuration_windows;

namespace CoatingScheduler
{
    /// <summary>
    /// Interaction logic for ProductNotes.xaml
    /// </summary>
    public partial class ProductNoteControl : ProductControlBase
    {
        public CoatingScheduleNote Note { get; set; }
        public override ICoatingScheduleControl ParentControl { get; set; }

        public new static ProductNoteControl CreateControl(ICoatingScheduleLogic logic)
        {
            return new ProductNoteControl(logic);
        }

        private ProductNoteControl(ICoatingScheduleLogic logic)
        {
            InitializeComponent();
            DataContext = logic;
        }

        public override Machine Machine { get { return null; } set{} }
        public override Configuration Config { get { return null; } set { } }

        public override void Add_Button(object sender, RoutedEventArgs e)
        {
            if(Note != null)
                Note.DestroySelf();
        }

        public override void AddControlToBottom(ICoatingScheduleLogic logic)
        {
            throw new NotImplementedException();
        }

        public override void AddControlToTop(ICoatingScheduleLogic logic)
        {
            throw new NotImplementedException();
        }

        public override void RemoveControl(ICoatingScheduleControl child)
        {
            throw new NotImplementedException();
        }

        public override void Connect(ICoatingScheduleLogic logic)
        {
            Note = (CoatingScheduleNote) logic;
            DataContext = Note;
            UpdateControlData();
        }

        public override void Connect(ICoatingScheduleControl parent)
        {
            ParentControl = parent;
        }

        public override ICoatingScheduleLogic GetLogic()
        {
            return Note;
        }

        public override void DestroySelf()
        {
            Disconnect();
        }

        public override void Disconnect()
        {
            Note = null;
            ParentControl = null;
        }

        public override ICoatingScheduleControl SwapControlWithBottom(ICoatingScheduleControl newControl)
        {
            throw new NotImplementedException();
        }

        public override ICoatingScheduleControl SwapControlWithTop(ICoatingScheduleControl newControl)
        {
            throw new NotImplementedException();
        }

        public override void UpdateControlData()
        {
            NotesBox.Text = Note.Text;
        }
        
        private void BtnSwapUp_OnClick(object sender, RoutedEventArgs e)
        {
            if(Note != null)
                Note.SwapUp();
        }

        private void BtnSwapDown_OnClick(object sender, RoutedEventArgs e)
        {
            if(Note != null)
                Note.SwapDown();
        }

        private void BtnRemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            if(Note != null)
                Note.DestroySelf();
        }

    }
}
