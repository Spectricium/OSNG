using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace OSNGLib
{
    public class Encode
    {
        static string ConvertBytesToReadableSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
        static double CalculateCompressionPercentage(long originalSize, long compressedSize)
        {
            double compressionRatio = 1 - ( (double)compressedSize / originalSize );
            double compressionPercentage = compressionRatio * 100;
            return compressionPercentage;
        }
        public static void AnyToOSNG(string path, string saveto, Action<double, double> ReportProgress = null)
        {
            // No es necesario llamar a GC.Collect() explícitamente aquí
            GC.Collect();
            string pixels = ProcessImage(path, ReportProgress);
            byte[] compressedPixels = Helper.Compress(pixels);
            File.WriteAllBytes(saveto, compressedPixels);
        }

        private static string ProcessImage(string path, Action<double, double> ReportProgress = null)
        {
            Bitmap bm = (Bitmap)Bitmap.FromFile(path);
            int pixelSize = Image.GetPixelFormatSize(bm.PixelFormat) / 8;
            int estimatedSize = bm.Width * bm.Height * ( pixelSize * 2 ) + ( bm.Height * 1 ) + 10;
            StringBuilder build = new StringBuilder(estimatedSize);
            build.Append($"{pixelSize}{Split.Separator[0]}{bm.Width}{Split.Separator[0]}{bm.Height}{Split.Separator[0]}");

            // Bloquea los bits del bitmap
            Rectangle rect = new Rectangle(0, 0, bm.Width, bm.Height);
            BitmapData bmpData = bm.LockBits(rect, ImageLockMode.ReadOnly, bm.PixelFormat);

            // Obtiene la dirección de la primera línea
            IntPtr ptr = bmpData.Scan0;

            // Declara un array para contener los bytes del bitmap
            int bytes = Math.Abs(bmpData.Stride) * bm.Height;
            byte[] rgbValues = new byte[bytes];
            // Copia los valores RGB en el array
            Marshal.Copy(ptr, rgbValues, 0, bytes);
            // Procesa los datos de píxeles
            double pix = 1;
            bool hasAlpha = pixelSize == 4;
            double totalPixels = bm.Width * bm.Height;
            int index = 0;
            int Height = bm.Height;
            int Width = bm.Width;
            for (int y = 0; y < Height; y++)
            {
                int rowStart = y * bmpData.Stride;
                for (int x = 0; x < Width; x++)
                {
                    index = rowStart + ( x * pixelSize );
                    build.Append(ColorToHex.ToHexCompressed(
                        ElAtDe(rgbValues, index + 3),                       // Alpha (si está presente) o predeterminado opaco
                        rgbValues[index + 2],                                  // Rojo
                        rgbValues[index + 1],                                  // Verde
                        rgbValues[index],                                        // Azul
                        hasAlpha                                                    // Si se incluye el canal alfa
                    ));
                    ReportProgress?.Invoke(totalPixels, pix++);
                }
                build.Append(Split.Separator[0]); // Añade separador después de cada fila
            }

            // Desbloquea los bits
            bm.UnlockBits(bmpData);
            bm.Dispose();
            return build.ToString();
        }
        static byte ElAtDe(byte[] arr, int idx)
        {
            try
            {
                return arr[idx];
            } catch
            {
                return 0;
            }
        }
    }
}
