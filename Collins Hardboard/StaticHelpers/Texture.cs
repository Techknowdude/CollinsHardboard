using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace StaticHelpers
{
    public class Texture : ObservableObject
    {
        static ObservableCollection<Texture> _textures = new ObservableCollection<Texture>();
        public static ObservableCollection<Texture> TexturesCollection { get { return _textures; } }

        private String _name;

        public static Texture GetTexture(String name)
        {
            var tex = _textures.FirstOrDefault(t => t.Name == name);
            if (tex == null)
            {
                tex = new Texture(name);
                _textures.Add(tex);
            }

            return tex;
        }

        private Texture(string name)
        {
            Name = name;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChangedEvent();
            }
        }


        public override string ToString()
        {
            return Name;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Name);
        }

        public static Texture Load(BinaryReader reader)
        {
            return new Texture(reader.ReadString());
        }

        public static Texture GetDefault()
        {
            if (_textures.Count > 0)
            {
                return _textures.First();
            }
            else
            {
                return GetTexture("OM");
            }
        }
    }
}
