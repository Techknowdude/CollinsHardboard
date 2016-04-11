using System;

namespace CoatingScheduleMomentos
{
    public class NoteMomento : ProductMomentoBase
    {
        private String _text;

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public NoteMomento(String text)
        {
            Text = text;
        }
    }
}
