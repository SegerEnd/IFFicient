using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFFicient
{
    public class IFFFile
    {
        private List<IFFChunk> Chunks { get; } = new List<IFFChunk>();

        public void AddChunk(IFFChunk chunk)
        {
            Chunks.Add(chunk);
        }

        public void WriteToFile(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Create);
            using var bw = new BinaryWriter(fs);

            WriteIFFHeader(bw);
            foreach (var chunk in Chunks)
            {
                chunk.Write(bw);
            }
        }

        public static IFFFile ReadFromFile(string filePath)
        {
            var iff = new IFFFile();
            using var fs = new FileStream(filePath, FileMode.Open);
            using var br = new BinaryReader(fs);

            var formId = new string(br.ReadChars(4));
            if (formId != "DOLL") throw new FormatException("Not a correct DOLL / IFF file");

            uint fileSize = br.ReadUInt32(); // Read and use the size 

            // Log or use fileSize and typeId for validation or processing
            //Console.WriteLine($"Reading DOLL file with size: {fileSize}");

            long expectedEndPosition = br.BaseStream.Position + fileSize;
            while (br.BaseStream.Position < expectedEndPosition)
            {
                iff.AddChunk(IFFChunk.ReadChunk(br));
            }

            // Check if we've read exactly the amount of data specified by the size field
            //if (br.BaseStream.Position != expectedEndPosition)
            //{
            //    Console.WriteLine("Warning: File size does not match the expected size.");
            //}

            return iff;
        }

        private void WriteIFFHeader(BinaryWriter bw)
        {
            byte[] FORM = Encoding.ASCII.GetBytes("DOLL".PadRight(4, ' '));
            bw.Write(FORM);
            uint totalSize = (uint)Chunks.Sum(c => c.Size + 8 + (c.Size % 2 == 0 ? 0 : 1)); // Chunk size + header size + possible padding
            bw.Write(totalSize);
        }
    }
}
