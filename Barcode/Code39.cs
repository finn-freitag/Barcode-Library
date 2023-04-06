using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Barcode
{
    public static class Code39
    {
        /*
        1 -> black/narrow
        2 -> white/narrow
        3 -> black/wide
        4 -> white/wide
         */

        public static Bitmap Create(string str, int borderwidth = 25, int height = 75, int multiplier = 1, bool addAsterisks = true)
        {
            // Build code

            str = str.ToUpper();
            if (addAsterisks) str = '*' + str + '*';
            string code = "";
            for(int i = 0; i < str.Length; i++)
            {
                if (!Letters.ContainsKey(str[i])) throw new InvalidCastException("Unknown character!");
                code += Letters[str[i]];
                code += '2';
            }
            if (code.Length > 0) code = code.Substring(0, code.Length - 1);

            // Build black/white pixels

            List<bool> pixels = new List<bool>();

            for(int i = 0; i < code.Length; i++)
            {
                switch(code[i])
                {
                    case '1':
                        pixels.Add(true);
                        break;
                    case '2':
                        pixels.Add(false);
                        break;
                    case '3':
                        pixels.Add(true);
                        pixels.Add(true);
                        pixels.Add(true);
                        break;
                    case '4':
                        pixels.Add(false);
                        pixels.Add(false);
                        pixels.Add(false);
                        break;
                }
            }

            Bitmap bmp = new Bitmap(borderwidth * 2 + pixels.Count * multiplier, borderwidth * 2 + height);

            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);

            for(int i = 0; i < pixels.Count; i++)
            {
                if (pixels[i]) g.FillRectangle(Brushes.Black, borderwidth + i * multiplier, borderwidth, multiplier, height);
            }

            g.Flush();
            g.Dispose();

            return bmp;
        }

        public static readonly Dictionary<char, string> Letters = new Dictionary<char, string>()
        {
            { 'A', "321214123" },
            { 'B', "123214123" },
            { 'C', "323214121" },
            { 'D', "121234123" },
            { 'E', "321234121" },
            { 'F', "123234121" },
            { 'G', "121214323" },
            { 'H', "321214321" },
            { 'I', "123214321" },
            { 'J', "121234321" },
            { 'K', "321212143" },
            { 'L', "123212143" },
            { 'M', "323212141" },
            { 'N', "121232143" },
            { 'O', "321232141" },
            { 'P', "123232141" },
            { 'Q', "121212343" },
            { 'R', "321212341" },
            { 'S', "123212341" },
            { 'T', "121232341" },
            { 'U', "341212123" },
            { 'V', "143212123" },
            { 'W', "343212121" },
            { 'X', "141232123" },
            { 'Y', "341232121" },
            { 'Z', "143232121" },

            { '0', "121432321" },
            { '1', "321412123" },
            { '2', "123412123" },
            { '3', "323412121" },
            { '4', "121432123" },
            { '5', "321432121" },
            { '6', "123432121" },
            { '7', "121412323" },
            { '8', "321412321" },
            { '9', "123412321" },

            { '*', "141232321" },
            { ' ', "143212321" },
            { '-', "141212323" },
            { '$', "141414121" },
            { '%', "121414141" },
            { '.', "341212321" },
            { '/', "141412141" },
            { '+', "141214141" }
        };
    }
}
