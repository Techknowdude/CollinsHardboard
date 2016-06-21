using System;
using System.Collections.Generic;
using System.IO;

namespace StaticHelpers
{
    public class Texture : ObservableObject
    {
        static Dictionary<String,Texture> _textures = new Dictionary<String,Texture>();

        private String _name;

        public static Texture GetTexture(String name)
        {
            if (!_textures.ContainsKey(name))
            {
                _textures[name] = new Texture(name);
            }

            return _textures[name];
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
    }
}
