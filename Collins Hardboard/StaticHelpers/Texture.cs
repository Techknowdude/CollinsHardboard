using System;
using System.IO;

namespace StaticHelpers
{
    public class Texture
    {
        private String _name;

        public Texture(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
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
