using System;

namespace IFFicient
{
    public class IFFChunk
    {
        public string ChunkId { get; }
        public uint Size { get; }
        public byte[] Data { get; }

        public IFFChunk(string chunkId, uint size, byte[] data)
        {
            ChunkId = chunkId.PadRight(4, ' '); // Right-pad with spaces if less than 4 chars
            Size = size;
            Size = size;
            Data = data;
        }

        public static IFFChunk ReadChunk(BinaryReader br)
        {
            var chunkId = new string(br.ReadChars(4));
            uint size = br.ReadUInt32();
            byte[] data = br.ReadBytes((int)size);
            // Skip padding byte if size is odd
            if (size % 2 != 0) br.ReadByte();

            return new IFFChunk(chunkId, size, data);
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(ChunkId.ToCharArray());
            bw.Write(Size);
            bw.Write(Data);
            if (Data.Length % 2 != 0) bw.Write((byte)0); // Padding
        }
    }
}
