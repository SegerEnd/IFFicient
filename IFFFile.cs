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
        public string FormID = "FORM";

        /// <summary>
        /// The name of the program that created the IFF file.
        /// (max 24 characters)
        /// Can be used to check if the file is intended for the specific program or to identify the program that created the file.
        /// </summary>
        public string ProgramName;


        /// <summary>
        /// The name of the program for creation of the IFF file.
        /// (max 24 characters)
        /// Used to set the ProgramName if it is not manually set.
        /// Can be used to set the ProgramName to a default value accross the whole application. Instead of setting it each time manually.
        /// </summary>
        public static string ApplicationName = "IFFicient by Seger";

        /// <summary>
        /// List of valid FORM IDs
        /// </summary>
        public static string[] FormIDs = { "FORM", "LIST", "CAT ", "PROP" };

        public IFFFile()
        {
        }

        public IFFFile(string formId)
        {
            // add padding and truncate if necessary
            FormID = formId.PadRight(4, ' ')[0..4];
            // when the formId is not in the FormIDs list add it to the list
            if (!IFFFile.FormIDs.Contains(FormID))
            {
                IFFFile.FormIDs = IFFFile.FormIDs.Append(FormID).ToArray();
            }
        }

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

        /// <summary>
        /// Gets a chunk by its index in the chunks list of the IFF file
        /// </summary>
        public IFFChunk GetChunk(int index)
        {
            return Chunks[index];
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
                using FileStream fs = new FileStream(filePath, FileMode.Create);
                using BinaryWriter bw = new BinaryWriter(fs);

                WriteHeader(bw, FormID);
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
        /// Reads an IFF file from the specified stream
        /// </summary>
        /// <param name="stream">The stream containing the IFF file data, e.g. a FileStream, MemoryStream, etc.</param>
        /// <returns>The complete IFF file object</returns>
        private static IFFFile ReadFromStream(Stream stream)
        {
            var iff = new IFFFile();
            using BinaryReader br = new BinaryReader(stream);

            // Read the Program Name
            iff.ProgramName = new string(br.ReadChars(24)).Trim();

            iff.FormID = new string(br.ReadChars(4));

            if (!FormIDs.Contains(iff.FormID))
                throw new FormatException("Not a correct file format");

            ulong fileSize = br.ReadUInt64();

            long expectedEndPosition = br.BaseStream.Position + (long)fileSize;
            while (br.BaseStream.Position < expectedEndPosition)
            {
                iff.AddChunk(IFFChunk.ReadChunk(br));
            }

            // Check if we've read exactly the amount of data specified by the size field
            if (br.BaseStream.Position != expectedEndPosition)
            {
                throw new FormatException("File size does not match the actual size of the file");
            }

            br.Close();
            stream.Close();

            return iff;
        }

        /// <summary>
        /// Reads an IFF file from the specified path
        /// </summary>
        /// <param name="filePath">The path to the IFF file</param>
        /// <returns>The complete IFF file object</returns>
        public static IFFFile ReadFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty");

            using var fs = new FileStream(filePath, FileMode.Open);
            return ReadFromStream(fs);
        }

        public static IFFFile ReadFromBytes(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Data cannot be null or empty");

            using var ms = new MemoryStream(data);
            return ReadFromStream(ms);
        }

        /// <summary>
        /// Writes the IFF header to the file
        /// Includes the Program name, Form identifier and the total size of the file
        /// </summary>
        /// <param name="bw"></param>
        private void WriteHeader(BinaryWriter bw, string FormID)
        {
            if (string.IsNullOrEmpty(ProgramName))
            {
                ProgramName = ApplicationName;
            }
            byte[] programName = Encoding.ASCII.GetBytes(ProgramName.PadRight(24, ' ')[0..24]); // Encode to ASCII with a max length of 24 characters and added padding
            bw.Write(programName);
            byte[] FORM = Encoding.ASCII.GetBytes(FormID.PadRight(4, ' '));
            bw.Write(FORM);
            ulong totalSize = (ulong)Chunks.Sum(c => c.Size + 8 + (c.Size % 2 == 0 ? 0 : 1)); // Total size calculation 1 byte padding if size is odd
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
