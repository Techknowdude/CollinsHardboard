using System;
using System.IO;

namespace CoatingScheduleMomentos
{
    public abstract class ProductCaretakerBase : ICoatingCaretaker
    {
        public abstract void SaveToBin(BinaryWriter fout);

        public abstract void LoadFromBin(BinaryReader fin);

        public abstract void ExportToExcel(string file = "");

        public abstract ProductMomentoBase Momento { get; set; }

        public static ProductCaretakerBase CreateProductCaretaker(string type)
        {
            ProductCaretakerBase caretaker = null;
            String product = typeof (ProductCaretaker).ToString();
            String note = typeof (NoteCaretaker).ToString();
            
            if(type == product)
                caretaker = new ProductCaretaker();
            else if(type == note)
                caretaker = new NoteCaretaker();
       
            return caretaker;
        }

        public static ProductCaretakerBase CreateProductCaretaker(ICoatingMomento newMomento)
        {
            ProductCaretakerBase newBase = null;
            if (newMomento is ProductMomento)
                newBase = new ProductCaretaker(newMomento);
            else if (newMomento is NoteMomento)
                newBase = new NoteCaretaker(newMomento);
            
            return newBase;
        }
    }
}
