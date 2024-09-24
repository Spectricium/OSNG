using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ZstdNet;

namespace OSNGLib
{
    internal class Helper
    {
        private const int BlockSize = 1024 * 1024 * 2; // 2 MB bloques

        public static byte[] Compress(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException(nameof(text), "The text to compress cannot be null or empty.");
            }
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            
            using (MemoryStream compressedStream = new MemoryStream())
            {
                using (Compressor compressor = new Compressor(new CompressionOptions(17)))
                {
                    int offset = 0;
                    while (offset < buffer.Length)
                    {
                        int size = Math.Min(BlockSize, buffer.Length - offset);
                        byte[] block = new byte[size];
                        Array.Copy(buffer, offset, block, 0, size);

                        byte[] compressedBlock;
                        try
                        {
                            compressedBlock = compressor.Wrap(block);
                            compressedStream.Write(BitConverter.GetBytes(compressedBlock.Length), 0, sizeof(int));
                            compressedStream.Write(compressedBlock, 0, compressedBlock.Length);
                        }
                        catch (ZstdException ex)
                        {
                            Console.WriteLine($"Error compressing block at offset {offset}: {ex.Message}");
                            throw;
                        }
                        offset += BlockSize;
                    }
                }
                return compressedStream.ToArray();
            }
        }

        public static string Decompress(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                throw new ArgumentNullException(nameof(buffer), "The buffer to decompress cannot be null or empty.");
            }

            using (MemoryStream decompressedStream = new MemoryStream())
            {
                using (MemoryStream bufferStream = new MemoryStream(buffer))
                {
                    using (Decompressor decompressor = new Decompressor())
                    {
                        byte[] lengthBuffer = new byte[sizeof(int)];
                        int bytesRead;
                        while (( bytesRead = bufferStream.Read(lengthBuffer, 0, lengthBuffer.Length) ) > 0)
                        {
                            if (bytesRead < lengthBuffer.Length)
                            {
                                throw new InvalidDataException("Unexpected end of stream while reading block size.");
                            }

                            int blockSize = BitConverter.ToInt32(lengthBuffer, 0);
                            byte[] compressedBlock = new byte[blockSize];
                            bytesRead = bufferStream.Read(compressedBlock, 0, blockSize);
                            if (bytesRead < blockSize)
                            {
                                throw new InvalidDataException("Unexpected end of stream while reading compressed block.");
                            }

                            try
                            {
                                byte[] decompressedBlock = decompressor.Unwrap(compressedBlock);
                                decompressedStream.Write(decompressedBlock, 0, decompressedBlock.Length);
                            }
                            catch (ZstdException ex)
                            {
                                Console.WriteLine($"Error decompressing block at offset {bufferStream.Position}: {ex.Message}");
                                throw;
                            }
                        }
                    }
                }
                return Encoding.UTF8.GetString(decompressedStream.ToArray());
            }
        }

    }
}