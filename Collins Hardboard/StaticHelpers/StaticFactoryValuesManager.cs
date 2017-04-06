using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace StaticHelpers
{
    public static class StaticFactoryValuesManager
    {
        #region Fields

        private static bool _loaded = false;

        private static List<Texture> _texturesList = new List<Texture>(); 

        private const string datFile = "FactoryValues.dat";

        private static string _conversionFilename =
            "C:\\Users\\Brandon\\Documents\\collins-hardboard-scheduler\\Collins Hardboard\\UnitTests\\bin\\Debug\\ConversionFile.csv";

        private static List<Tuple<string, double>> _stringDoubleConversionList = new List<Tuple<string, double>>();
        private static double _productionMaxThickness = (double) 5/4;

        private static Char _wipMarker = 'W';
        public static Char WiPMarker { get { return _wipMarker; } set { _wipMarker = value; } }

        private static ObservableCollection<string> _coatingLines = new ObservableCollection<string>
        {
            "Lap Line",
            "Panel Line"
        };

        private static ObservableCollection<string> _gradesList = new ObservableCollection<string>
        {
            "DEALER",
            "RE",
            "WASTE",
            "ECON",
            "NONE",
            "WIP",
            "INVALID"
        };

        private static List<string> _gradeAbbrList = new List<string>
        {
            "D",
            "R",
            "W",
            "E",
            "-",
            "WiP",
            "-"
        };

        #endregion

        #region Properties

        public static List<Texture> TexturesList
        {
            get { return _texturesList; }
            set { _texturesList = value; }
        }

        public static double ProductionMaxThickness
        {
            get { return _productionMaxThickness; }
            set { _productionMaxThickness = value; }
        }

        public static List<Tuple<string, double>> StringDoubleConversionList
        {
            get { return _stringDoubleConversionList; }
            set { _stringDoubleConversionList = value; }
        }

        public static ObservableCollection<string> CoatingLines
        {
            get { return _coatingLines; }
            set { _coatingLines = value; }
        }

        public static List<string> GradeAbbrList
        {
            get { return _gradeAbbrList; }
            set { _gradeAbbrList = value; }
        }

        public static ObservableCollection<string> GradesList
        {
            get { return _gradesList; }
            set { _gradesList = value; }
        }

        public static double WasteMin { get; set; }
        public static double WasteMax { get; set; }
        public static double CurrentWaste { get; set; }
        public static bool Loaded { get { return _loaded;} }
        #endregion


        static StaticFactoryValuesManager()
        {
            LoadValues(true);
        }

        public static bool SaveValues()
        {
            bool success = SaveData(datFile);
            if (!success)
            {
                MessageBox.Show("Settings save failed. Please select save location");

                SaveFileDialog saveDlg = new SaveFileDialog();
                saveDlg.FileName = datFile;

                if (saveDlg.ShowDialog() == true)
                {
                    success = SaveData(saveDlg.FileName);
                    if(success)
                    {
                        MessageBox.Show("Settings saved!");
                    }
                    else
                    {
                        MessageBox.Show("Settings could not be saved. Please Contact developer.");
                    }
                }
            }

            return success;
        }

        private static bool SaveData(String fileName)
        {
            bool succeeded = true;
            try
            {
                using (BinaryWriter writer = new BinaryWriter(new FileStream(fileName, FileMode.OpenOrCreate)))
                {
                    // conversion, lines, grades, abbr, waste min, waste max
                    writer.Write(StringDoubleConversionList.Count);
                    foreach (var tuple in StringDoubleConversionList)
                    {
                        writer.Write(tuple.Item1);
                        writer.Write(tuple.Item2);
                    }

                    //grades and abbr
                    writer.Write(GradesList.Count);
                    for (int index = 0; index < GradesList.Count; index++)
                    {
                        var grade = GradesList[index];
                        var abbr = GradeAbbrList[index];
                        writer.Write(grade);
                        writer.Write(abbr);
                    }

                    // lines
                    writer.Write(CoatingLines.Count);
                    foreach (var coatingLine in CoatingLines)
                    {
                        writer.Write(coatingLine);
                    }

                    //waste
                    writer.Write(WasteMin);
                    writer.Write(WasteMax);
                    writer.Write(CurrentWaste);

                    writer.Write(TexturesList.Count);
                    foreach (var texture in TexturesList)
                    {
                        texture.Save(writer);
                    }

                    // WiP Marker
                    writer.Write(WiPMarker);
                }
            }
            catch (Exception exception)
            {
                succeeded = false;
            }


            return succeeded;
        }

        public static void LoadValues(bool quietMode = false)
        {
            if (!LoadData(datFile) && !quietMode)
            {
                MessageBox.Show("Failed to load factory settings. Please open factory settings file.");
                OpenFileDialog dlg = new OpenFileDialog
                {
                    FileName = datFile,
                    DefaultExt = ".dat",
                    Multiselect = false
                };

                // Show open file dialog box 
                if (dlg.ShowDialog() == true)
                {
                    if (LoadData(dlg.FileName))
                        MessageBox.Show("Factory settings successfully loaded.");
                }
            }
        }

        private static bool LoadData(String fileName)
        {
            bool succeeded = true;

            try
            {

                using (BinaryReader reader = new BinaryReader(new FileStream(fileName, FileMode.Open)))
                {
                    // conversion, lines, grades, abbr, waste min, waste max
                    int listLen = reader.ReadInt32();

                    StringDoubleConversionList.Clear();
                    for (; listLen > 0; --listLen)
                    {
                        String item1 = reader.ReadString();
                        double item2 = reader.ReadDouble();
                        StringDoubleConversionList.Add(new Tuple<string, double>(item1, item2));
                    }

                    //grades and abbr
                    listLen = reader.ReadInt32();
                    GradesList.Clear();
                    GradeAbbrList.Clear();

                    for (; listLen > 0; --listLen)
                    {
                        String grade = reader.ReadString();
                        String abbr = reader.ReadString();
                        GradesList.Add(grade);
                        GradeAbbrList.Add(abbr);
                    }

                    // lines
                    listLen = reader.ReadInt32();
                    CoatingLines.Clear();
                    for (; listLen > 0; --listLen)
                    {
                        String line = reader.ReadString();
                        CoatingLines.Add(line);
                    }

                    //waste
                    WasteMin = reader.ReadDouble();
                    WasteMax = reader.ReadDouble();
                    CurrentWaste = reader.ReadDouble();

                    listLen = reader.ReadInt32();
                    TexturesList.Clear();
                    for (; listLen > 0; --listLen)
                    {
                        TexturesList.Add(Texture.Load(reader));
                    }


                    // WiP Marker
                    WiPMarker = reader.ReadChar();
                }
            }
            catch (Exception)
            {
                succeeded = false;
            }
            if (succeeded)
                _loaded = true;

            return succeeded;
        }
    }
}
