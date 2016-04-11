using System;
using System.IO;

namespace CoatingScheduleMomentos
{
    public class ProductCaretaker : ProductCaretakerBase
    {
        private ProductMomento _momento;
        public ProductCaretaker()
        {
            
        }

        public ProductCaretaker(ICoatingMomento newMomento)
        {
            ProductMomento momento = newMomento as ProductMomento;
            _momento = momento;
        }

        public override ProductMomentoBase Momento
        {
            get { return _momento; }
            set { _momento = value as ProductMomento; }
        }

        public override void SaveToBin(BinaryWriter fout)
        {
            // save properties
            fout.Write(((ProductMomento) Momento).Thickness);
            fout.Write(((ProductMomento) Momento).ProductCode);
            fout.Write(((ProductMomento) Momento).Grades);
            fout.Write(((ProductMomento) Momento).HasBarcode);
            fout.Write(((ProductMomento) Momento).Units);
            fout.Write(((ProductMomento) Momento).Notes);
            fout.Write(((ProductMomento) Momento).Placement);
            fout.Write(((ProductMomento) Momento).Description);
        }

        public override void LoadFromBin(BinaryReader fin)
        {
            if(Momento == null)
                Momento = new ProductMomento();

            ((ProductMomento)Momento).Thickness = fin.ReadString();
            ((ProductMomento)Momento).ProductCode = fin.ReadString();
            ((ProductMomento)Momento).Grades = fin.ReadString();
            ((ProductMomento)Momento).HasBarcode = fin.ReadBoolean();
            ((ProductMomento)Momento).Units = fin.ReadString();
            ((ProductMomento)Momento).Notes = fin.ReadString();
            ((ProductMomento)Momento).Placement = fin.ReadString();
            ((ProductMomento)Momento).Description = fin.ReadString();
        }

        public override void ExportToExcel(string file = "")
        {
            // TODO: Implement this...
            throw new NotImplementedException();
        }
    }
}
