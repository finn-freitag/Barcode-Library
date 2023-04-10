using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Barcode
{
    public class Code128
    {
        public static Bitmap Create(string str, int borderwidth = 25, int height = 75, int multiplier = 1)
        {
            string bars = GetShortestCombiRecursiv(str);

            Bitmap bmp = new Bitmap(borderwidth * 2 + bars.Length * multiplier, borderwidth * 2 + height);

            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);

            for (int i = 0; i < bars.Length; i++)
            {
                if (bars[i] == '1') g.FillRectangle(Brushes.Black, borderwidth + i * multiplier, borderwidth, multiplier, height);
            }

            g.Flush();
            g.Dispose();

            return bmp;
        }

        private static string GetShortestCombiRecursiv(string str)
        {
            List<List<int>> combis = new List<List<int>>();
            combis.AddRange(GetCombi(new int[] { 103 }.ToList(), 'A', str));
            combis.AddRange(GetCombi(new int[] { 104 }.ToList(), 'B', str));
            combis.AddRange(GetCombi(new int[] { 105 }.ToList(), 'C', str));

            if (combis.Count == 0) throw new InvalidCastException("Unkown character!");

            List<int> shortest = combis[0];
            for(int i = 1; i < combis.Count; i++)
            {
                if (combis[i].Count < shortest.Count) shortest = combis[i];
            }

            shortest.Add(CalculateChecksum(shortest));
            shortest.Add(106); // stopcode
            return IndexesToPattern(shortest);
        }

        private static List<List<int>> GetCombi(List<int> indexes, char charset, string remaining)
        {
            List<List<int>> combis = new List<List<int>>();

            bool found = false;

            if (char.IsDigit(remaining[0]) && remaining.Length > 1 && char.IsDigit(remaining[1]))
            {
                if (charset == 'C')
                {
                    indexes.Add(GetStringPosCharsetC("" + remaining[0] + remaining[1]));
                    found = true;
                }
            }
            else
            {
                if (charset == 'A')
                {
                    int posA = GetCharPosCharsetA("" + remaining[0]);
                    if (posA >= 0)
                    {
                        indexes.Add(posA);
                        found = true;
                    }
                }
                if (charset == 'B')
                {
                    int posB = GetCharPosCharsetB("" + remaining[0]);
                    if (posB >= 0)
                    {
                        indexes.Add(posB);
                        found = true;
                    }
                }
            }

            if (!found) return combis;

            if (found) remaining = remaining.Substring(1);
            if (charset == 'C') remaining = remaining.Substring(1);

            if (remaining.Length == 0)
            {
                combis.Add(indexes);
                return combis;
            }

            List<int> indexesA = Clone(indexes);
            if (charset != 'A') indexesA.Add(101);
            combis.AddRange(GetCombi(indexesA, 'A', remaining));

            List<int> indexesB = Clone(indexes);
            if (charset != 'B') indexesB.Add(100);
            combis.AddRange(GetCombi(indexesB, 'B', remaining));

            List<int> indexesC = Clone(indexes);
            if (charset != 'C') indexesC.Add(99);
            combis.AddRange(GetCombi(indexesC, 'C', remaining));

            return combis;
        }

        private static string IndexesToPattern(List<int> indexes)
        {
            string str = "";
            for(int i = 0; i < indexes.Count; i++)
            {
                str += GetPattern(indexes[i]);
            }
            return str;
        }

        private static int CalculateChecksum(List<int> indexes)
        {
            int sum = 0;
            for (int i = 0; i < indexes.Count; i++)
            {
                sum += indexes[i] * (i == 0 ? 1 : i);
            }
            return sum % 103;
        }

        /*private static string GetShortestCombi(string str)  // broken because jumping i value by charset C -> conflicts with combis in other charsets
        {
            List<(string, char, List<int>)> combis = new List<(string, char, List<int>)>();
            //combis.Add((StartA, 'A'));
            combis.Add((StartB, 'B', new int[] { 104 }.ToList()));
            //combis.Add((StartC, 'C'));
            List<int> ignoredIndexes = new List<int>();

            for(int i = 0; i < str.Length; i++)
            {
                for(int j = 0; j < combis.Count; j++)
                {
                    if (!ignoredIndexes.Contains(j))
                    {
                        char currentCharset = combis[j].Item2;
                        int pos = -1;
                        if (currentCharset != 'C') pos = GetCharPosCharset(currentCharset, "" + str[i]);
                        else if (i < str.Length - 1) pos = GetCharPosCharset(currentCharset, "" + str[i] + str[i + 1]);
                        if (pos >= 0)
                        {
                            List<int> nums = combis[j].Item3;
                            nums.Add(pos);
                            combis[j] = (combis[j].Item1 + GetPattern(pos), currentCharset, nums);
                            if (currentCharset == 'C') i++;
                        }
                        else
                        {
                            int posA = GetCharPosCharset(currentCharset, "" + str[i]);
                            int posB = GetCharPosCharset(currentCharset, "" + str[i]);
                            int posC = -1;
                            if (i < str.Length - 1) posC = GetCharPosCharset(currentCharset, "" + str[i] + str[i]);

                            ignoredIndexes.Add(j);

                            bool found = false;

                            if (posA >= 0)
                            {
                                found = true;
                                List<int> nums = Clone(combis[j].Item3);
                                nums.Add(103);
                                nums.Add(posA);
                                combis.Add((combis[j].Item1 + StartA + GetPattern(posA), 'A', nums));
                            }
                            if (posB >= 0)
                            {
                                found = true;
                                List<int> nums = Clone(combis[j].Item3);
                                nums.Add(104);
                                nums.Add(posB);
                                combis.Add((combis[j].Item1 + StartB + GetPattern(posB), 'B', nums));
                            }
                            if (posC >= 0)
                            {
                                found = true;
                                List<int> nums = Clone(combis[j].Item3);
                                nums.Add(105);
                                nums.Add(posC);
                                combis.Add((combis[j].Item1 + StartC + GetPattern(posC), 'C', nums));
                                i++;
                            }
                            if (!found) throw new InvalidCastException("Unknown character!");
                        }
                    }
                }
            }

            string shortest = "";
            int index = 0;
            for(int i = 0; i < combis.Count; i++)
            {
                if (!ignoredIndexes.Contains(i))
                {
                    if (shortest == "") shortest = combis[i].Item1;
                    if (combis[i].Item1.Length < shortest.Length)
                    {
                        shortest = combis[i].Item1;
                        index = i;
                    }
                }
            }

            int sum = 1;
            //for(int i = combis[index].Item3.Count - 1; i >= 0; i--)
            for(int i = 0; i < combis[index].Item3.Count; i++)
            {
                sum += combis[index].Item3[i] * i;
            }

            return shortest + GetPattern(sum % 103) + Stop;
        }*/

        private static List<int> Clone(List<int> ints)
        {
            List<int> res = new List<int>();
            for(int i = 0; i < ints.Count; i++)
            {
                res.Add(ints[i]);
            }
            return res;
        }

        private static int GetCharPosCharset(char charset, string c)
        {
            if (charset == 'A') return GetCharPosCharsetA(c);
            if (charset == 'B') return GetCharPosCharsetB(c);
            if (charset == 'C') return GetStringPosCharsetC(c);
            return -1;
        }

        private static int GetCharPosCharsetA(string c)
        {
            for(int i = 0; i < CharsetABC.Length; i++)
            {
                if (CharsetABC[i][0] == c) return i;
            }
            return -1;
        }

        private static int GetCharPosCharsetB(string c)
        {
            for (int i = 0; i < CharsetABC.Length; i++)
            {
                if (CharsetABC[i][1] == c) return i;
            }
            return -1;
        }

        private static int GetStringPosCharsetC(string c)
        {
            for (int i = 0; i < CharsetABC.Length; i++)
            {
                if (CharsetABC[i][2] == c) return i;
            }
            return -1;
        }

        private static string GetPattern(int index)
        {
            return CharsetABC[index][3];
        }

        public static readonly string StartA = "11010000100";
        public static readonly string SwitchA = "11101011110";
        public static readonly string StartB = "11010010000";
        public static readonly string SwitchB = "10111101110";
        public static readonly string StartC = "11010011100";
        public static readonly string SwitchC = "10111011110";
        public static readonly string Stop = "1100011101011";

        public static readonly string[][] CharsetABC = new string[][]
        {
            //             Charset A      Charset B       CS.C  Pattern
            new string[] { " "          , " "           , "00", "11011001100" },
            new string[] { "!"          , "!"           , "01", "11001101100" },
            new string[] { "\""         , "\""          , "02", "11001100110" },
            new string[] { "#"          , "#"           , "03", "10010011000" },
            new string[] { "$"          , "$"           , "04", "10010001100" },
            new string[] { "%"          , "%"           , "05", "10001001100" },
            new string[] { "&"          , "&"           , "06", "10011001000" },
            new string[] { "'"          , "'"           , "07", "10011000100" },
            new string[] { "("          , "("           , "08", "10001100100" },
            new string[] { ")"          , ")"           , "09", "11001001000" },
            new string[] { "*"          , "*"           , "10", "11001000100" },
            new string[] { "+"          , "+"           , "11", "11000100100" },
            new string[] { ","          , ","           , "12", "10110011100" },
            new string[] { "-"          , "-"           , "13", "10011011100" },
            new string[] { "."          , "."           , "14", "10011001110" },
            new string[] { "/"          , "/"           , "15", "10111001100" },
            new string[] { "0"          , "0"           , "16", "10011101100" },
            new string[] { "1"          , "1"           , "17", "10011100110" },
            new string[] { "2"          , "2"           , "18", "11001110010" },
            new string[] { "3"          , "3"           , "19", "11001011100" },
            new string[] { "4"          , "4"           , "20", "11001001110" },
            new string[] { "5"          , "5"           , "21", "11011100100" },
            new string[] { "6"          , "6"           , "22", "11001110100" },
            new string[] { "7"          , "7"           , "23", "11101101110" },
            new string[] { "8"          , "8"           , "24", "11101001100" },
            new string[] { "9"          , "9"           , "25", "11100101100" },
            new string[] { ":"          , ":"           , "26", "11100100110" },
            new string[] { ";"          , ";"           , "27", "11101100100" },
            new string[] { "<"          , "<"           , "28", "11100110100" },
            new string[] { "="          , "="           , "29", "11100110010" },
            new string[] { ">"          , ">"           , "30", "11011011000" },
            new string[] { "?"          , "?"           , "31", "11011000110" },
            new string[] { "@"          , "@"           , "32", "11000110110" },
            new string[] { "A"          , "A"           , "33", "10100011000" },
            new string[] { "B"          , "B"           , "34", "10001011000" },
            new string[] { "C"          , "C"           , "35", "10001000110" },
            new string[] { "D"          , "D"           , "36", "10110001000" },
            new string[] { "E"          , "E"           , "37", "10001101000" },
            new string[] { "F"          , "F"           , "38", "10001100010" },
            new string[] { "G"          , "G"           , "39", "11010001000" },
            new string[] { "H"          , "H"           , "40", "11000101000" },
            new string[] { "I"          , "I"           , "41", "11000100010" },
            new string[] { "J"          , "J"           , "42", "10110111000" },
            new string[] { "K"          , "K"           , "43", "10110001110" },
            new string[] { "L"          , "L"           , "44", "10001101110" },
            new string[] { "M"          , "M"           , "45", "10111011000" },
            new string[] { "N"          , "N"           , "46", "10111000110" },
            new string[] { "O"          , "O"           , "47", "10001110110" },
            new string[] { "P"          , "P"           , "48", "11101110110" },
            new string[] { "Q"          , "Q"           , "49", "11010001110" },
            new string[] { "R"          , "R"           , "50", "11000101110" },
            new string[] { "S"          , "S"           , "51", "11011101000" },
            new string[] { "T"          , "T"           , "52", "11011100010" },
            new string[] { "U"          , "U"           , "53", "11011101110" },
            new string[] { "V"          , "V"           , "54", "11101011000" },
            new string[] { "W"          , "W"           , "55", "11101000110" },
            new string[] { "X"          , "X"           , "56", "11100010110" },
            new string[] { "Y"          , "Y"           , "57", "11101101000" },
            new string[] { "Z"          , "Z"           , "58", "11101100010" },
            new string[] { "["          , "["           , "59", "11100011010" },
            new string[] { "\\"         , "\\"          , "60", "11101111010" },
            new string[] { "]"          , "]"           , "61", "11001000010" },
            new string[] { "^"          , "^"           , "62", "11110001010" },
            new string[] { "_"          , "_"           , "63", "10100110000" },
            new string[] { "" + (char)0 , "`"           , "64", "10100001100" },
            new string[] { "" + (char)1 , "a"           , "65", "10010110000" },
            new string[] { "" + (char)2 , "b"           , "66", "10010000110" },
            new string[] { "" + (char)3 , "c"           , "67", "10000101100" },
            new string[] { "" + (char)4 , "d"           , "68", "10000100110" },
            new string[] { "" + (char)5 , "e"           , "69", "10110010000" },
            new string[] { "" + (char)6 , "f"           , "70", "10110000100" },
            new string[] { "" + (char)7 , "g"           , "71", "10011010000" },
            new string[] { "" + (char)8 , "h"           , "72", "10011000010" },
            new string[] { "" + (char)9 , "i"           , "73", "10000110100" },
            new string[] { "" + (char)10, "j"           , "74", "10000110010" },
            new string[] { "" + (char)11, "k"           , "75", "11000010010" },
            new string[] { "" + (char)12, "l"           , "76", "11001010000" },
            new string[] { "" + (char)13, "m"           , "77", "11110111010" },
            new string[] { "" + (char)14, "n"           , "78", "11000010100" },
            new string[] { "" + (char)15, "o"           , "79", "10001111010" },
            new string[] { "" + (char)16, "p"           , "80", "10100111100" },
            new string[] { "" + (char)17, "q"           , "81", "10010111100" },
            new string[] { "" + (char)18, "r"           , "82", "10010011110" },
            new string[] { "" + (char)19, "s"           , "83", "10111100100" },
            new string[] { "" + (char)20, "t"           , "84", "10011110100" },
            new string[] { "" + (char)21, "u"           , "85", "10011110010" },
            new string[] { "" + (char)22, "v"           , "86", "11110100100" },
            new string[] { "" + (char)23, "w"           , "87", "11110010100" },
            new string[] { "" + (char)24, "x"           , "88", "11110010010" },
            new string[] { "" + (char)25, "y"           , "89", "11011011110" },
            new string[] { "" + (char)26, "z"           , "90", "11011110110" },
            new string[] { "" + (char)27, "{"           , "91", "11110110110" },
            new string[] { "" + (char)28, "|"           , "92", "10101111000" },
            new string[] { "" + (char)29, "}"           , "93", "10100011110" },
            new string[] { "" + (char)30, "~"           , "94", "10001011110" },
            new string[] { "" + (char)31, "" + (char)127, "95", "10111101000" },
            new string[] { ""           , ""            , "96", "10111100010" },
            new string[] { ""           , ""            , "97", "11110101000" },
            new string[] { ""           , ""            , "98", "11110100010" },
            new string[] { ""           , ""            , "99", "10111011110" }, // Switch C
            new string[] { ""           , ""            , ""  , "10111101110" }, // Switch B
            new string[] { ""           , ""            , ""  , "11101011110" }, // Switch A
            new string[] { ""           , ""            , ""  , "11110101110" },
            new string[] { ""           , ""            , ""  , "11010000100" }, // Start A
            new string[] { ""           , ""            , ""  , "11010010000" }, // Start B
            new string[] { ""           , ""            , ""  , "11010011100" }, // Start C
            new string[] { ""           , ""            , ""  , "1100011101011" }, // Stop
        };
    }
}
