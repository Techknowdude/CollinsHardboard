using System.IO;

namespace CoatingScheduleMomentos
{
    /// <summary>
    /// This should be inherited by each thing to be saved.
    /// </summary>
    public interface ICoatingOriginator
    {
        void LoadMomento(ICoatingMomento loadedMomento);
        ICoatingMomento CreateMomento();
    }

    /// <summary>
    /// This class stores the information
    /// </summary>
    public interface ICoatingMomento
    {
         
    }

    /// <summary>
    /// This class will determine the use of the momento. 
    /// Intended to take care of the excel export and schedule save.
    /// </summary>
    public interface ICoatingCaretaker
    {
        void SaveToBin(BinaryWriter fout);
        void LoadFromBin(BinaryReader fin);
        void ExportToExcel(string file = "");
    }
}
