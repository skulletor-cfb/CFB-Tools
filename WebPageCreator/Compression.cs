using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace EA_DB_Editor
{
    public interface IFileIO
    {
        string ReadFromFile(string file);
        void WriteToFile(string file, string data);
        string[] ReadAllLines(string file);
    }

    public interface ICompressor<T> : IFileIO
    {
        T Compress(string data);
        string Decompress(T data);
    }

    public class GZipCompressor : ICompressor<byte[]>
    {
        public static GZipCompressor Instance = new GZipCompressor();

        public byte[] Compress(string data)
        {
            return data.ZipItGood();
        }

        public string Decompress(byte[] data)
        {
            return data.UnzipIt();
        }

        public string ReadFromFile(string file)
        {
            return File.ReadAllBytes(file).UnzipIt();
        }

        public string[] ReadAllLines(string file)
        {
            var s = this.ReadFromFile(file);
            File.WriteAllText("temp", s);
            return File.ReadAllLines("temp");
        }

        public void WriteToFile(string file, string data)
        {
            File.WriteAllBytes(file, data.ZipItGood());
        }
    }

    public class B64GZip : ICompressor<string>
    {
        public string Compress(string data)
        {
            return Convert.ToBase64String(data.ZipItGood());
        }


        public string Decompress(string data)
        {
            return Convert.FromBase64String(data).UnzipIt();
        }

        public string ReadFromFile(string file)
        {
            return this.Decompress(File.ReadAllText(file));
        }

        public void WriteToFile(string file, string data)
        {
            File.WriteAllText(file, this.Compress(data));
        }

        public string[] ReadAllLines(string file)
        {
            var s = this.ReadFromFile(file);
            File.WriteAllText("temp", s);
            return File.ReadAllLines("temp");
        }
    }

    public static class Compression
    {
        public static IFileIO Instance = new B64GZip();
        public static string UnzipIt(this byte[] data)
        {
            List<byte> result = new List<byte>();
            using (var ms = new MemoryStream(data))
            {
                using (var gz = new GZipStream(ms, CompressionMode.Decompress))
                {
                    byte[] bytes = new byte[1024];
                    int read = 0;

                    do
                    {
                        read = gz.Read(bytes, 0, bytes.Length);
                        result.AddRange(bytes.Take(read));
                    } while (read > 0);
                }
            }

            return Encoding.UTF8.GetString(result.ToArray());
        }

        public static byte[] ZipItGood(this string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);

            using (var ms = new MemoryStream())
            {
                using (var gz = new GZipStream(ms, CompressionMode.Compress))
                {
                    gz.Write(bytes, 0, bytes.Length);
                    gz.Close();
                    return ms.ToArray();
                }
            }

        }
    }

}