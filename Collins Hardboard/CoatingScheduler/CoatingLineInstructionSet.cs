using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Microsoft.Office.Interop.Excel;
using StaticHelpers;

namespace CoatingScheduler
{
    public class CoatingLineInstructionSet 
    {
        #region Fields
        private ObservableCollection<Instruction> _instructionsCollection = new ObservableCollection<Instruction>();
        private string _coatingLine;

        private static Int32 _width = 4;

        #endregion

        #region Properties
        public static Int32 Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public String CoatingLine
        {
            get { return _coatingLine; }
            set { _coatingLine = value; }
        }

        public ObservableCollection<Instruction> InstructionsCollection
        {
            get { return _instructionsCollection; }
            set { _instructionsCollection = value; }
        }

        public InstructionSetControl Control { get; set; }

        #endregion


        public CoatingLineInstructionSet(string line, ObservableCollection<Instruction> instructions = null)
        {
            CoatingLine = line;
            if (instructions != null)
            {
                InstructionsCollection = instructions;
            }
            else if (InstructionsCollection == null)
            {
                InstructionsCollection = new ObservableCollection<Instruction>();
            }
        }

        public CoatingLineInstructionSet()
        {

        }

        public static CoatingLineInstructionSet Load(BinaryReader reader)
        {
            string coatingLine = reader.ReadString();

            ObservableCollection<Instruction> instructions = new ObservableCollection<Instruction>();
            int numInstructions = reader.ReadInt32();
            for (; numInstructions > 0; --numInstructions)
            {
                instructions.Add(Instruction.Load(reader));
            }

            return new CoatingLineInstructionSet(coatingLine,instructions);
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(CoatingLine);

            writer.Write(InstructionsCollection.Count);

            foreach (var instruction in InstructionsCollection)
            {
                instruction.Save(writer);
            }
        }

        public Tuple<int, int> ExportToExcel(_Worksheet sheet, Int32 column, Int32 row)
        {
            Int32 nextRow = row;

            Range range = sheet.Range[StaticFunctions.GetRangeIndex(column, nextRow)];
            StaticFunctions.SaveRichTextToCell(range, CoatingLine, PublicEnums.FontWeight.Bold, 14);
            ++nextRow;

            foreach (var instruction in InstructionsCollection)
            {
                Tuple<int, int> nextPlace = instruction.ExportToExcel(sheet, column, nextRow);
                ++nextRow;
            }

            return new Tuple<int, int>(nextRow, column + Width);
        }

        public void AddInstruction()
        {
            InstructionsCollection.Add(new Instruction());
        }

        public void DeleteInstruction()
        {
            if(InstructionsCollection.Count > 0)
                InstructionsCollection.RemoveAt(InstructionsCollection.Count - 1);
        }

        public void Close()
        {
            InstructionsCollection.Clear();
        }
    }
}
