using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OSNGLib
{
    public class Decode
    {
        static int allpixels = 0;
        public static Bitmap OSNGToPNG(string path, Action<double, double> ReportProgress = null)
        {
            string[] ou = Helper.Decompress(File.ReadAllBytes(path)).Split(Split.Separator, StringSplitOptions.RemoveEmptyEntries);
            int pixelSize = Convert.ToInt32(ou[0]);
            int width = Convert.ToInt32(ou[1]);
            int height = Convert.ToInt32(ou[2]);
            bool includeAlpha = pixelSize == 4;
            int Pixels = width * height;
            allpixels = 0;
            Bitmap bmp = new Bitmap(width, height, includeAlpha ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);

            // Bloquea los bits completos del bitmap
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);

            // Obtiene la dirección de la primera línea
            IntPtr ptr = bmpData.Scan0;

            // Declara un array para contener los bytes del bitmap
            int stride = Math.Abs(bmpData.Stride);
            byte[] rgbValues = new byte[stride * bmp.Height];

            StringBuilder temp = new StringBuilder();
            for (int i = 3; i < ou.Length; i++)
            {
                string str = ou[i];
                int idx = 0;
                int rowIndex = ( i - 3 ) * width * pixelSize;

                // Debugging output for rowIndex

                while (idx + pixelSize <= str.Length)
                {
                    try
                    {
                        int index = rowIndex + ( idx / pixelSize ) * pixelSize;  // Cálculo de índice ajustado

                        // Debugging output for index

                        if (index + pixelSize <= rgbValues.Length)
                        {
                            temp.Append(str, idx, pixelSize);  // Para obtener una subcadena
                            (byte a, byte r, byte g, byte b) = ColorToHex.FromHexCompressedAsColorValues(temp.ToString());
                            temp.Clear();
                            rgbValues[index] = b;        // Azul
                            rgbValues[index + 1] = g;    // Verde
                            rgbValues[index + 2] = r;    // Rojo
                            if (includeAlpha)
                                rgbValues[index + 3] = a;  // Alpha (si está presente)
                        }
                        else
                        {
                            // Debugging output for index out of range
                            //Console.WriteLine($"Index out of range: {index}");
                        }

                        idx += pixelSize;
                        ReportProgress?.Invoke(Pixels, allpixels++);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        break;
                    }
                }
            }

            // Copia los valores RGB de vuelta al bitmap
            Marshal.Copy(rgbValues, 0, ptr, rgbValues.Length);

            // Desbloquea los bits
            bmp.UnlockBits(bmpData);

            ReportProgress?.Invoke(allpixels, allpixels);
            return bmp;
        }
    }
}
