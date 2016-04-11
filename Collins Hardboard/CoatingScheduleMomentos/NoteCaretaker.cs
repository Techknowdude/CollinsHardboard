using System;
using System.IO;

namespace CoatingScheduleMomentos
{
    public class NoteCaretaker : ProductCaretakerBase
    {
        #region Fields
        private NoteMomento _momento;
        #endregion

        #region Properties
        public override ProductMomentoBase Momento
        {
            get { return _momento; }
            set { _momento = value as NoteMomento; }
        }
        #endregion

        public NoteCaretaker(ICoatingMomento newMomento)
        {
            Momento = newMomento as NoteMomento;
        }

        public NoteCaretaker()
        {
            
        }


        public override void SaveToBin(BinaryWriter fout)
        {
            fout.Write(((NoteMomento) Momento).Text);
        }

        public override void LoadFromBin(BinaryReader fin)
        {
            String text = fin.ReadString();
            if (Momento == null)
                Momento = new NoteMomento(text);
            else
                ((NoteMomento)Momento).Text = text;
        }

        public override void ExportToExcel(string file = "")
        {
            // TODO: Fill this in
            throw new NotImplementedException();
        }
    }
}
