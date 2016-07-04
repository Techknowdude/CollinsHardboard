using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ImportLib;
using ModelLib;
using StaticHelpers;

namespace ExtendedScheduleViewer
{
    /// <summary>
    /// Interaction logic for WatchWindow.xaml
    /// </summary>
    public partial class WatchWindow : Window
    {
        public ObservableCollection<ProductMasterItem> MasterList => StaticInventoryTracker.ProductMasterList;

        public ObservableCollection<ProductMasterItem> WatchList => ExtendedSchedule.Instance.Watches; 

        public WatchWindow()
        {
            InitializeComponent();
            Closing += OnClosing;
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            // save the watches
            ExtendedSchedule.Instance.Save();
        }

        public ICommand AddWatchCommand { get { return new DelegateCommand(AddWatch);} }
        public ICommand RemoveWatchCommand { get { return new DelegateCommand(RemoveWatch);} }
        public ICommand MoveUpCommand { get { return new DelegateCommand(MoveItemUp);} }
        public ICommand MoveDownCommand { get { return new DelegateCommand(MoveItemDown);} }


        private void MoveItemDown(object obj)
        {
            ProductMasterItem item = obj as ProductMasterItem;
            if (item == null) return;

            int curIndex = WatchList.IndexOf(item);
            if (curIndex >= 0 && curIndex != WatchList.Count -1) // if not last and found
            {
                WatchList.RemoveAt(curIndex);
                WatchList.Insert(curIndex + 1, item);
            ExtendedSchedule.Instance.Update();
            }
        }

        private void MoveItemUp(object obj)
        {
            ProductMasterItem item = obj as ProductMasterItem;
            if (item == null) return;

            int curIndex = WatchList.IndexOf(item);
            if (curIndex > 0) // if not first and found
            {
                WatchList.RemoveAt(curIndex);
                WatchList.Insert(curIndex-1,item);
            ExtendedSchedule.Instance.Update();
            }
        }

        private void AddWatch(object obj)
        {
            ProductMasterItem newWatch = obj as ProductMasterItem;
            if (newWatch == null) return;

            if (!WatchList.Contains(newWatch))
            {
                WatchList.Add(newWatch);
            ExtendedSchedule.Instance.Update();
            }
        }
        private void RemoveWatch(object obj)
        {
            ProductMasterItem newWatch = obj as ProductMasterItem;
            if (newWatch == null) return;
            
            WatchList.Remove(newWatch);
            ExtendedSchedule.Instance.Update();
        }
    }
}
