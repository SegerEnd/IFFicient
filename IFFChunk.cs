
namespace IFFicient
{
    public class IFFChunk
    {
        private string _chunkID = "ERR ";

        /// <summary>
        /// The ID of the chunk 4-character string (e.g. "NAME", "(c) ", "HOME", etc.)
        /// The setter will right-pad the string with spaces if less than 4 characters & truncate if more than 4 characters
        /// </summary>
        public string ChunkId
        {
            get { return _chunkID; }
            set { _chunkID = value.PadRight(4, ' ')[0..4]; }
        }

        public uint Size;

        /// <summary>
        /// The byte data of the chunk (e.g. the actual data of the chunk)
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Creates a new IFFChunk object with the given chunk ID, size, and data
        /// This is for full control over the chunk data, but it's my recommendation to use the other constructor without the size parameter
        /// </summary>
        /// <param name="chunkId">The 4-character string ID of the chunk (e.g. "NAME", "(c) ", "HOME", etc.)</param>
        /// <param name="size">The size of only the data, not the size of the data + other bytes (e.g. chunk ID, size)</param>
        /// <param name="data">Byte array of the data</param>
        public IFFChunk(string chunkId, uint size, byte[] data)
        {
            ChunkId = chunkId;
            Size = size;
            Size = size;
            Data = data;
        }

        /// <summary>
        /// Creates a new IFFChunk object with the given chunk ID and data
        /// Automatically sets the size of the chunk based on the data length
        /// </summary>
        /// <param name="chunkId">The 4-character string ID of the chunk (e.g. "NAME", "(c) ", "HOME", etc.)</param>
        /// <param name="data"></param>
        public IFFChunk(string chunkId, byte[] data)
        {
            ChunkId = chunkId;
            Size = (uint)data.Length;
            Data = data;
        }

        /// <summary>
        /// Creates a new empty IFFChunk object with the given chunk ID
        /// </summary>
        /// <param name="chunkId">The 4-character string ID of the chunk (e.g. "NAME", "(c) ", "HOME", etc.)</param>
        public IFFChunk(string chunkId)
        {
            ChunkId = chunkId;
            Data = new byte[0];
            Size = (uint)Data.Length;
        }

        /// <summary>
        /// Creates a new IFFChunk object with the given chunk ID and data as a string.
        /// The data will be converted to a byte array byte[] using ASCII encoding.
        /// </summary>
        /// <param name="chunkId">The 4-character string ID of the chunk (e.g. "NAME", "(c) ", "HOME", etc.)</param>
        /// <param name="data">The string of data to set to the chunk</param>
        public IFFChunk(string chunkId, string data)
        {
            ChunkId = chunkId;
            AddData(data);
        }

        /// <summary>
        /// Sets the given data to the chunk including its size.
        /// </summary>
        /// <param name="data">The string of data to set to the chunk</param>
        public void AddData(string data)
        {
            Data = System.Text.Encoding.ASCII.GetBytes(data);
            Size = (uint)Data.Length;
        }

        // Saves JSON to the data including size, where the data is not currently in byte[] format, Use a binary writer to write the data
        //public void AddData(System.Text.Json.JsonDocument data)
        //{
        //    Data = System.Text.Encoding.ASCII.GetBytes(data.RootElement.GetRawText());
        //    Size = (uint)Data.Length;
        //}


        /// <summary>
        /// Creates and reads a chunk on the current position of the BinaryReader
        /// </summary>
        /// <param name="br">The BinaryReader to read the chunk from</param>
        /// <returns>A new IFFChunk object containing the chunk data, ID, and size</returns>
        public static IFFChunk ReadChunk(BinaryReader br)
        {
            var chunkId = new string(br.ReadChars(4));
            uint size = br.ReadUInt32();
            byte[] data = br.ReadBytes((int)size);
            // Skip padding byte if size is odd
            if (size % 2 != 0) br.ReadByte();

            return new IFFChunk(chunkId, size, data);
        }

        public static IFFFile IsIFFFile(IFFChunk chunk)
        {
            // Check if the data of the chunk is containing another IFF file inside
            byte[] data = chunk.Data;
            try
            {
                IFFFile iff = IFFFile.ReadFromBytes(data);
                
                if (!string.IsNullOrEmpty(iff.ProgramName) && iff.Chunks.Count > 0)
                {
                    return iff;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Writes the chunk to the BinaryWriter at its current position
        /// </summary>
        /// <param name="bw">The BinaryWriter to write the chunk to</param>
        public void Write(BinaryWriter bw)
        {
            bw.Write(ChunkId.ToCharArray());
            bw.Write(Size);
            bw.Write(Data);
            if (Data.Length % 2 != 0) bw.Write((byte)0); // Padding
        }

    }
}
