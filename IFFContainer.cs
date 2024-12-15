using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFFicient
{
    public abstract class IFFContainer
    {
        protected List<IFFChunk> Chunks { get; } = new List<IFFChunk>();

        public void AddChunk(IFFChunk chunk)
        {
            Chunks.Add(chunk);
        }

        public virtual void WriteToFile(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Create);
            using var bw = new BinaryWriter(fs);
            WriteHeader(bw);
            WriteChunks(bw);
        }

        public static IFFContainer ReadFromFile(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Open);
            using var br = new BinaryReader(fs);

            if (IsIFFFile(br))
                return ReadIFFFile(br);
            else
                throw new FormatException("Unknown file format"); // Simplified for brevity, assuming no FAR file in this example
        }

        protected abstract void WriteHeader(BinaryWriter bw);

        protected void WriteChunks(BinaryWriter bw)
        {
            foreach (var chunk in Chunks)
            {
                WriteChunk(bw, chunk);
            }
        }

        protected virtual void WriteChunk(BinaryWriter bw, IFFChunk chunk)
        {
            chunk.Write(bw); // Delegate writing to the chunk itself
        }

        protected static bool IsIFFFile(BinaryReader br)
        {
            var signature = ReadString(br, 4);
            br.BaseStream.Seek(-4, SeekOrigin.Current); // Reset the stream position
            return signature == "FORM";
        }

        private static IFFFile ReadIFFFile(BinaryReader br)
        {
            var iff = new IFFFile();
            ReadIFFHeader(br);
            ReadIFFChunks(br, iff);
            return iff;
        }

        private static void ReadIFFHeader(BinaryReader br)
        {
            ReadString(br, 4); // "FORM"
            br.ReadInt32(); // Size - not used in this implementation
            ReadString(br, 4); // "SIM "
        }

        private static void ReadIFFChunks(BinaryReader br, IFFFile iff)
        {
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                var chunk = IFFChunk.Read(br);
                iff.AddChunk(chunk);
            }
        }

        public static string ReadString(BinaryReader br, int length) => new string(br.ReadChars(length));
    }
}
