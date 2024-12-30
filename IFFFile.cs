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
        /// <summary>
        /// List of chunks in the IFF file
        /// </summary>
        private List<IFFChunk> Chunks { get; } = new List<IFFChunk>();

        /// <summary>
        /// The FORM ID of the IFF file
        /// </summary>
        /// <remarks>
        /// FORM is a form of chunks,
        /// LIST is a list of chunks,
        /// CAT is a catalog of chunks,
        /// PROP is a property list
        /// </remarks>
        private string FormID = "FORM";

        /// <summary>
        /// List of valid FORM IDs
        /// </summary>
        private static readonly string[] FormIDs = { "FORM", "LIST", "CAT ", "PROP" };

        /// <summary>
        /// Adds a chunk to the IFF file chunks list
        /// </summary>
        /// <remarks>
        /// All the chunks will be written to the file when calling WriteToFile
        /// </remarks>
        /// <param name="chunk"></param>
        public void AddChunk(IFFChunk chunk)
        {
            Chunks.Add(chunk);
        }

        /// <summary>
        /// Gets a chunk by its chunk ID
        /// </summary>
        /// <remarks>
        /// A chunk ID is a 4-character string, e.g. "NAME", "(c) ", "HOME", etc.
        /// </remarks>
        public IFFChunk GetChunk(string chunkId)
        {
            return Chunks.FirstOrDefault(c => c.ChunkId == chunkId);
        }

        //public IFFChunk GetChunk(int index)
        //{
        //    return Chunks[index];
        //}

        /// <summary>
        /// Writes the IFF file to a file at the specified path
        /// </summary>
        /// <returns>True if the file was written successfully, false otherwise </returns>
        public bool WriteToFile(string filePath)
        {
            try
            {
                using var fs = new FileStream(filePath, FileMode.Create);
                using var bw = new BinaryWriter(fs);

                WriteIFFHeader(bw, FormID);
                foreach (var chunk in Chunks)
                {
                    chunk.Write(bw);
                }

                // free resources
                bw.Close();
                fs.Close();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Reads an IFF file from the specified path
        /// </summary>
        /// <param name="filePath">The path to the IFF file</param>
        /// <returns>The complete IFF file object</returns>
        public static IFFFile ReadFromFile(string filePath)
        {
            var iff = new IFFFile();
            using var fs = new FileStream(filePath, FileMode.Open);
            using var br = new BinaryReader(fs);

            var formId = new string(br.ReadChars(4));
            //if (formId != FormID) throw new FormatException("Not a correct DOLL / IFF file");
            if (!FormIDs.Contains(formId)) throw new FormatException("Not a correct DOLL / IFF file");

            ulong fileSize = br.ReadUInt64(); // Read and use the size 

            // Log or use fileSize and typeId for validation or processing
            //Console.WriteLine($"Reading DOLL file with size: {fileSize}");

            long expectedEndPosition = br.BaseStream.Position + (long)fileSize;
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

        /// <summary>
        /// Writes the IFF header to the file
        /// Includes the FORM ID and the total size of the file
        /// </summary>
        /// <param name="bw"></param>
        private void WriteIFFHeader(BinaryWriter bw, string FormID)
        {
            byte[] FORM = Encoding.ASCII.GetBytes(FormID.PadRight(4, ' '));
            bw.Write(FORM);
            ulong totalSize = (ulong)Chunks.Sum(c => c.Size + 8 + (c.Size % 2 == 0 ? 0 : 1)); // Total size calculation
            bw.Write(totalSize);
        }

        /// <summary>
        /// Returns a string with the information of the IFF file
        /// </summary>
        public override string ToString()
        {
            // give a string representation of the IFF file with name, total size, amount of chunks and size per chunk
            //string result = $"File: {FormID}\n";
            //result += $"Total size: {Chunks.Sum(c => c.Size + 8 + (c.Size % 2 == 0 ? 0 : 1))}\n";
            //result += $"Amount of chunks: {Chunks.Count}\n";
            //result += $"Size per chunk: {Chunks.Average(c => c.Size)}\n";
            return $"File: {FormID}, Total size: {Chunks.Sum(c => c.Size + 8 + (c.Size % 2 == 0 ? 0 : 1))}, Amount of chunks: {Chunks.Count}, Size per chunk: {Chunks.Average(c => c.Size)}";
        }
    }
}
