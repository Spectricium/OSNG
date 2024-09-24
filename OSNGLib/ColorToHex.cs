using System;
using System.Text;

namespace OSNGLib
{
    internal class ColorToHex
    {
        static StringBuilder sb = new StringBuilder();
        static string hexAlphabet = "0123456789ABCDEF";
        public static string ToHexCompressed(byte alpha, byte red, byte green, byte blue, bool includeAlpha)
        {
            sb.Clear();
            if (includeAlpha)
            {
                sb.Append(hexAlphabet[alpha / 17]);
            }
            sb.Append(hexAlphabet[red / 17]);
            sb.Append(hexAlphabet[green / 17]);
            sb.Append(hexAlphabet[blue / 17]);
            return sb.ToString();
        }


        private static byte HexToByte(char hex)
        {
            return (byte)hexAlphabet.IndexOf(hex);
            //return (byte)( hex <= '9' ? hex - '0' : hex - 'A' + 10 );
        }

        public static (byte, byte, byte, byte) FromHexCompressedAsColorValues(string hex)
        {
            //recompiler 2
            int length = hex.Length;
            byte a = 255, r, g, b;
            if (length == 4)
            {
                a = (byte)( HexToByte(hex[0]) * 17 );  // Multiplica por 17 para ajustar de 4 bits a 8 bits
                r = (byte)( HexToByte(hex[1]) * 17 );
                g = (byte)( HexToByte(hex[2]) * 17 );
                b = (byte)( HexToByte(hex[3]) * 17 );
                return (a, r, g, b);
            }
            else if (length == 3)
            {
                r = (byte)( HexToByte(hex[0]) * 17 );
                g = (byte)( HexToByte(hex[1]) * 17 );
                b = (byte)( HexToByte(hex[2]) * 17 );
                return (a, r, g, b);
            }
            else
            {
                throw new ArgumentException("Invalid hex format.");
            }
        }
    }
}
