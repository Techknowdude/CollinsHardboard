using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoatingScheduleMomentos
{
    public class ShiftCaretaker : ICoatingCaretaker
    {
        private ShiftMomento _momento;

        public ShiftMomento Momento
        {
            get { return _momento; }
            set { _momento = value; }
        }

        public void SaveToBin(BinaryWriter fout)
        {
            // save # of products
            fout.Write(Momento.Caretakers.Count);
            // foreach product, call save
            foreach (ProductCaretakerBase caretaker in Momento.Caretakers)
            {
                fout.Write(caretaker.GetType().ToString());
                caretaker.SaveToBin(fout);
            }
        }

        public void LoadFromBin(BinaryReader fin)
        {
            // load number of records
            int numCaretakers = fin.ReadInt32();

            if(Momento == null)
                Momento = new ShiftMomento();

            for (; numCaretakers  > -1 ; numCaretakers--)
            {
                // get caretaker type
                String type = fin.ReadString();

                // make caretaker
                ProductCaretakerBase newCaretaker = ProductCaretakerBase.CreateProductCaretaker(type);

                // load momento
                newCaretaker.LoadFromBin(fin);

                // add new caretaker to list
                Momento.Caretakers.Add(newCaretaker);
            }
        }

        public void ExportToExcel(string file = "")
        {
            // TODO: Fill this in
            throw new NotImplementedException();
        }
    }
}
