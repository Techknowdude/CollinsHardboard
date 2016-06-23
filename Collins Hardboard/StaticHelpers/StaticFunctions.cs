using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Office.Interop.Excel;
using Font = Microsoft.Office.Interop.Excel.Font;

namespace StaticHelpers
{
    public static class StaticFunctions
    {

        public static DateTime GetDayAndTime(DateTime day, DateTime time)
        {
            return new DateTime(day.Year,day.Month,day.Day,time.Hour,time.Minute,time.Second);
        }
        
        /// <summary>
        /// Returns the string equivalent for a 1 indexed location
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static string GetRangeIndex(Int32 column, Int32 row)
        {
            return (char)('A' + column - 1) + row.ToString();
        }

        /// <summary>
        /// Exports the rich text string to the excel sheet passed in the [row,col]
        /// </summary>
        /// <param name="range">The cell range for the excel document</param>
        /// <param name="richText">String containing the rich text</param>
        /// <param name="fontWeight">Weight of the font. Bold, Italic, or both</param>
        /// <param name="fontSize">Size of font for text</param>
        /// <param name="color">Color of text</param>
        public static void SaveRichTextToCell(Range range, string richText, PublicEnums.FontWeight fontWeight = PublicEnums.FontWeight.None,
            Int32 fontSize = 0, Color color = default(Color))
        {
            // the number of '{' in front of the actual text to junk
            string delims = "{{{{{{{";
            range.NumberFormat = "@";
            Font font = range.Font;
            font.Color = ColorTranslator.ToOle(color);
            if (fontSize != 0)
                font.Size = fontSize;
            switch (fontWeight)
            {
                case PublicEnums.FontWeight.Bold:
                    font.Bold = true;
                    break;
                case PublicEnums.FontWeight.Italics:
                    font.Italic = true;
                    break;
                case PublicEnums.FontWeight.BoldItalic:
                    font.Bold = true;
                    font.Italic = true;
                    break;
            }

            // string to be parsed through
            string workingString = richText;

            // get index of first section by removing junk in front
            Int32 junkIndex;
            for (Int32 index = 0; index < delims.Length; index++)
            {
                char delim = delims[index];
                junkIndex = workingString.IndexOf(delim) + 1;
                if (junkIndex != -1)
                    workingString = workingString.Substring(junkIndex);
                else
                    index = delims.Length; // break loop
            }

            junkIndex = workingString.IndexOf('{');
            if (junkIndex != -1)
            {
                workingString = workingString.Substring(junkIndex);

                List<Tuple<int, int, bool, bool>> formatList = new List<Tuple<int, int, bool, bool>>();

                Int32 start = 0;
                string outputString = String.Empty;
                while (workingString != String.Empty && workingString[0] == '{')
                {
                    // start of text in working
                    Int32 subStart = workingString.IndexOf('h') + 2;
                    // len of text
                    Int32 len = workingString.IndexOf('}') - subStart;
                    // get text
                    outputString = workingString.Substring(subStart, len);
                    // add text to cell
                    range.Value = range.Text + outputString;

                    // save format info
                    bool isBold = workingString.Substring(0, workingString.IndexOf('}')).Contains(@"\b");
                    bool isItalic = workingString.Substring(0, workingString.IndexOf('}')).Contains(@"\i");
                    formatList.Add(new Tuple<int, int, bool, bool>(start, len, isBold, isItalic));

                    // move start of section in output
                    start = start + len;

                    // move forward in parse string
                    workingString = workingString.Substring(workingString.IndexOf('{') + 1);
                    Int32 nextIndex = workingString.IndexOf('{');
                    if (nextIndex != -1)
                        workingString = workingString.Substring(nextIndex);
                    else
                    {
                        workingString = String.Empty;
                    }
                }

                // format
                foreach (Tuple<int, int, bool, bool> tuple in formatList)
                {
                    range.Characters[tuple.Item1, tuple.Item2].Font.Bold = tuple.Item3;
                    range.Characters[tuple.Item1, tuple.Item2].Font.Italic = tuple.Item4;
                }
            }
            else
            {
                range.Value = workingString;
            }
        }
        /// <summary>
        /// Strips the rich text formatting of a string and returns just plain text.
        /// </summary>
        /// <param name="richText">String containing the rich text</param>
        public static string RemoveRichTextFormatting(string richText)
        {
            // the number of '{' in front of the actual text to junk
            string delims = "{{{{{{{";
            string outputString = String.Empty;

            // string to be parsed through
            string workingString = richText;

            // get index of first section by removing junk in front
            Int32 junkIndex;
            for (Int32 index = 0; index < delims.Length; index++)
            {
                char delim = delims[index];
                junkIndex = workingString.IndexOf(delim) + 1;
                if (junkIndex != -1)
                    workingString = workingString.Substring(junkIndex);
                else
                    index = delims.Length; // break loop
            }

            junkIndex = workingString.IndexOf('{');
            if (junkIndex != -1)
            {
                workingString = workingString.Substring(junkIndex);

                Int32 start = 0;
                while (workingString != String.Empty && workingString[0] == '{')
                {
                    // start of text in working
                    Int32 subStart = workingString.IndexOf('h') + 2;
                    // len of text
                    Int32 len = workingString.IndexOf('}') - subStart;
                    // get text
                    outputString += workingString.Substring(subStart, len);

                    // move start of section in output
                    start = start + len;

                    // move forward in parse string
                    workingString = workingString.Substring(workingString.IndexOf('{') + 1);
                    Int32 nextIndex = workingString.IndexOf('{');
                    if (nextIndex != -1)
                        workingString = workingString.Substring(nextIndex);
                    else
                    {
                        workingString = String.Empty;
                    }
                }
            }
            else
            {
                outputString = workingString;
            }
            return outputString;
            
        }

        public static Int32 DivRoundUp(double x, double y)
        {
            return x % y < 1 ? Convert.ToInt32(x / y) : Convert.ToInt32(x / y) + 1;
        }
        public static Int32 DivRoundUp(Int32 x, Int32 y)
        {
            if (y == 0) return 0;
            return x % y == 0 ? Convert.ToInt32(x / y) : Convert.ToInt32(x / y) + 1;
        }
        public static Int32 DoubleRoundUp(float x)
        {
            return x - Convert.ToInt32(x) < 1 ? Convert.ToInt32(x) : Convert.ToInt32(x) + 1;
        }
        public static double StringToDouble(string data)
        {
            double retVal = 0;
            bool found = false;
            foreach (Tuple<string, double> tuple in StaticFactoryValuesManager.StringDoubleConversionList.Where(tuple => tuple.Item1 == data))
            {
                found = true;
                retVal = tuple.Item2;
            }

            if(found) return retVal;

            string dat1 = "";
            string dat2 = "";
            double d1 = 0;
            double d2 = 0;

            if (data.Contains("MM"))
                return Convert.ToDouble(data.Substring(0, data.IndexOf('M')));
            if (data.Contains("mm"))
                return Convert.ToDouble(data.Substring(0, data.IndexOf('m')));

            if (!data.Contains("/")) return Convert.ToDouble(data);

            for (Int32 i = 0; i < data.Length; i++)
            {
                switch (data[i])
                {
                    case ' ':
                        retVal += Convert.ToDouble(dat1);
                        dat1 = "";
                        break;
                    case '\\':
                    case '/':
                        d1 = Convert.ToDouble(dat1);
                        break;
                    default:
                        if (d1 != 0)
                            dat2 += data[i];
                        else
                            dat1 += data[i];
                        break;
                }
            }

            d2 = Convert.ToDouble(dat2);

            retVal += (d1 / d2);

            return retVal;
        }
        public static string ConvertDoubleToStringThickness(double thick)
        {
            string retVal = "";
            bool found = false;
            foreach (Tuple<string, double> tuple in StaticFactoryValuesManager.StringDoubleConversionList.Where(tuple => Math.Abs(tuple.Item2 - thick) < 0.0001))
            {
                found = true;
                retVal = tuple.Item1;
            }
            if(found) return retVal;


            if (StaticFactoryValuesManager.ProductionMaxThickness < thick)
            {
                return String.Format("{0}MM", thick);
            }
            Int32 incPerInch = Convert.ToInt32(Math.Round(1 / StaticConstValues.thicknessIncrement, 0, MidpointRounding.AwayFromZero));

            if (thick % StaticConstValues.thicknessIncrement == 0)
            {
                Int32 i = 0;

                while ((i * StaticConstValues.thicknessIncrement) < thick)
                {
                    i++;
                }

                if (i >= incPerInch)
                {
                    retVal += "1 ";
                    i -= incPerInch;
                }

                if ((i % 16) == 0 && i > 0)
                {
                    retVal += (i / 16).ToString() + "/2";
                }
                else if ((i % 8) == 0 && i > 0)
                {
                    retVal += (i / 8).ToString() + "/4";
                }
                else if ((i % 4) == 0 && i > 0)
                {
                    retVal += (i / 4).ToString() + "/8";
                }
                else if ((i % 2) == 0 && i > 0)
                {
                    retVal += (i / 2).ToString() + "/16";
                }
                else if (i > 0)
                {
                    retVal += i.ToString() + "/32";
                }
            }
            else if (Math.Round(thick % StaticConstValues.inchesPerMilimeter, 5) == 0)
            {
                Int32 i = 0;

                while ((i * StaticConstValues.inchesPerMilimeter) < thick)
                {
                    i++;
                }
                retVal = String.Format("{0}MM", i);
            }
            else
                retVal = Convert.ToString(thick);

            return retVal;
        }
    }
}
