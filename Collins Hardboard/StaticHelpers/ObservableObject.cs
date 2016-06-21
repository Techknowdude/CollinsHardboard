using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StaticHelpers
{
    [Serializable]
    public class ObservableObject : INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableObject(PropertyChangedEventHandler listener = null)
        {
            if (listener != null)
                PropertyChanged += listener;
        }
    }
}
