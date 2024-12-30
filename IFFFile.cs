using System;
using System.Text;

namespace IFFicient
{
    public class IFFFile
    {
        /// <summary>
        /// List of chunks in the IFF file
        /// </summary>
        public List<IFFChunk> Chunks { get; } = new List<IFFChunk>();

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
        /// The name of the program that created the IFF file
        /// Can be used to check if the file is intended for the specific program or to identify the program that created the file.
        /// </summary>
        public string ProgramName = "IFFicient by Seger";

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
            // Check if the data from chunk is correct format
            if (chunk.ChunkId.Length != 4) throw new FormatException("Chunk ID must be 4 characters long");
            if (chunk.Size != chunk.Data.Length) throw new FormatException("Chunk size does not match the actual size of the data");

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

            // Read the Program Name
            iff.ProgramName = new string(br.ReadChars(24)).Trim();

            iff.FormID = new string(br.ReadChars(4));

            if (!FormIDs.Contains(iff.FormID)) throw new FormatException("Not a correct file format");

            ulong fileSize = br.ReadUInt64();

            long expectedEndPosition = br.BaseStream.Position + (long)fileSize;
            while (br.BaseStream.Position < expectedEndPosition)
            {
                iff.AddChunk(IFFChunk.ReadChunk(br));
            }

            //Check if we've read exactly the amount of data specified by the size field
            if (br.BaseStream.Position != expectedEndPosition)
            {
                throw new FormatException("File size does not match the actual size of the file");
            }

            return iff;
        }

        /// <summary>
        /// Writes the IFF header to the file
        /// Includes the FORM ID and the total size of the file
        /// </summary>
        /// <param name="bw"></param>
        private void WriteIFFHeader(BinaryWriter bw, string FormID)
        {
            byte[] programName = Encoding.ASCII.GetBytes(ProgramName.PadRight(24, ' ')[0..24]);
            bw.Write(programName);
            byte[] FORM = Encoding.ASCII.GetBytes(FormID.PadRight(4, ' '));
            bw.Write(FORM);
            ulong totalSize = (ulong)Chunks.Sum(c => c.Size + 8 + (c.Size % 2 == 0 ? 0 : 1)); // Total size calculation
            bw.Write(totalSize);
        }

        /// <summary>
        /// Returns a string with the information of the IFF file, with name, total size, amount of chunks and size per chunk
        /// </summary>
        public override string ToString()
        {
            // Get the chunkid, size and data of each chunk
            IEnumerable<string> chunkInfo = Chunks.Select(c => $"{c.ChunkId} ({c.Size} bytes): {Encoding.ASCII.GetString(c.Data)}");

            return $"IFF File: {ProgramName}\nTotal size: {Chunks.Sum(c => c.Size)} bytes\nChunks: {Chunks.Count}\n{string.Join("\n", chunkInfo)}";
        }
    }
}
