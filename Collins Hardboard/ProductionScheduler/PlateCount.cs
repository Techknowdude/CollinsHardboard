using System.Collections.ObjectModel;
using System.ComponentModel;
using StaticHelpers;

namespace ProductionScheduler
{
    public class PlateCount : ObservableObject
    {
        private Texture _tex;
        private int _count;

        public PlateCount(Texture texture, int count, PropertyChangedEventHandler handler) : base(handler)
        {
            Tex = texture;
            Count = count;
        }

        public Texture Tex
        {
            get { return _tex; }
            set
            {
                _tex = value; 
                RaisePropertyChangedEvent();
            }
        }

        public int Count
        {
            get { return _count; }
            set
            {
                _count = value; 
                RaisePropertyChangedEvent();
            }
        }

        public ObservableCollection<Texture> Textures
        {
            get { return Texture.TexturesCollection; }
        }
    }
}
