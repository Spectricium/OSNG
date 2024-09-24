using OSNGLib;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace OSNGCli
{
    internal class Program
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
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args.Contains("-e"))
                {
                    for (int i = 1; i < args.Length; i++)
                    {
                        string file = args[i];
                        if (File.Exists(file))
                        {
                            Console.Write("[ " + new FileInfo(file).Name + " ] ");
                            Bitmap bmp = new Bitmap(file);
                            DateTime date;
                            using (var progress = new ProgressBar())
                            {
                                double prog = 0;
                                date = DateTime.UtcNow;
                                Encode.AnyToOSNG(file, Path.ChangeExtension(file, "osng"), (double pixels, double processed) =>
                                {
                                    if (processed == pixels)
                                    {
                                        progress.UpdateText("Compressing...");
                                        return;
                                    }
                                    prog = processed / pixels;
                                    progress.Report(prog);
                                });
                            }
                            DateTime dd = DateTime.UtcNow;
                            bmp.Dispose();
                            long un = new FileInfo(file).Length;
                            long ou = new FileInfo(Path.ChangeExtension(file, "osng")).Length;
                            Console.Write("Done.\n");
                            Console.WriteLine("Original File Size: " + ConvertBytesToReadableSize(un) + ", Output File Size: "+ ConvertBytesToReadableSize(ou) +"\r\n"+"Compression Rate: "+CalculateCompressionPercentage(un,ou)+"%");
                        }
                    }
                }
                else if (args.Contains("-d"))
                {
                    Console.Write("What Format The Exported Images Will Be (jpg default)? ");
                    string format = Console.ReadLine();
                    if (string.IsNullOrEmpty(format) || format == "" || format == " ")
                    {
                        format = "jpg";
                    }
                    format = format.ToLower();
                    for (int i = 1; i < args.Length; i++)
                    {
                        string file = args[i];
                        if (File.Exists(file))
                        {
                            Console.Write("[ " + new FileInfo(file).Name + " ] ");
                            using (var progress = new ProgressBar())
                            {
                                double prog = 0;

                                Bitmap bmp = Decode.OSNGToPNG(file, (double pixels, double processed) =>
                                {
                                    prog = processed / pixels;
                                    progress.Report(prog);
                                    if (prog == 1)
                                    {
                                        progress.Dispose();
                                        progress.UpdateText("Exporting...");
                                    }
                                });
                                string pa = Path.ChangeExtension(file, format);
                                ImageFormat image = ImageFormat.Png;
                                if (format.Contains("jp") && format.Contains("g"))
                                {
                                    image = ImageFormat.Jpeg;
                                }
                                else if (format.Contains("tiff"))
                                {
                                    image = ImageFormat.Tiff;
                                } else if (format.Contains("exif"))
                                {
                                    image = ImageFormat.Exif;
                                } else if (format.Contains("ico"))
                                {
                                    image = ImageFormat.Icon;
                                }
                                bmp.Save(pa, image);
                                bmp.Dispose();
                                progress.UpdateText("");
                                Console.Write("Done.\n");
                            }
                        }
                    }
                }
                else if (args.Contains("-about"))
                {
                    Console.WriteLine(@"Creator: ShowcaserPacman
Developed On A Span Of 2 Month's With Some Help Of ChatGPT For Speeding Up The Decoding & Encoding
Inspired By The .bruh Format From FaceDev
Made Using C# and using ZSTD (ZStandard) Compression Algorithm
Little Thing I Wasted 1 Month Trying To Optimize The Progress Bar, After I Made It Only Show The Percentage Only.");
                    return;
                }
                else
                {
                    Console.WriteLine("OSNGCli [options] [files]");
                    Console.WriteLine(@"-e Encodes Images To Osng
-d Decodes Osng's To Images
-about Show's Info And Credits
-a Create's A File Containing Some Extra Info");
                    return;
                }
            }
        }
    }
}
