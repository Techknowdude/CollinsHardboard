using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using StaticHelpers;

namespace ProductionScheduler
{
    [Serializable]
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
                _tex = Texture.GetTexture(value.Name); 
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

        public int TexIndex
        {
            get { return Texture.TexturesCollection.IndexOf(Texture.GetTexture(Tex.Name)); }
        }
    }
}
