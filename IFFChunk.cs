using System;

namespace IFFicient
{
    public class IFFChunk
    {
        public string Type { get; }
        public byte[] Content { get; }
        public List<IFFChunk> SubChunks { get; } = new List<IFFChunk>();

        public IFFChunk(string type, byte[] content)
        {
            Type = type;
            Content = content;
        }

        public int Size
        {
            get
            {
                int size = Content.Length + 8; // Type + Size + Content
                if (Content.Length % 2 != 0) size++; // Padding
                size += SubChunks.Sum(sub => sub.Size); // Size of subchunks
                return size;
            }
        }

        public void AddSubChunk(IFFChunk chunk)
        {
            SubChunks.Add(chunk);
        }

        // Write this chunk and all its subchunks recursively
        public void Write(BinaryWriter bw)
        {
            bw.Write(Type.ToCharArray());
            bw.Write(Size - 8); // Size of content + subchunks, excluding type and size fields
            bw.Write(Content);
            if (Content.Length % 2 != 0) bw.Write((byte)0); // Padding

            foreach (var subChunk in SubChunks)
            {
                subChunk.Write(bw);
            }
        }

        // Static method to read a chunk and its subchunks recursively
        public static IFFChunk Read(BinaryReader br)
        {
            var type = IFFContainer.ReadString(br, 4);
            var size = br.ReadInt32();
            var content = br.ReadBytes(size);
            var chunk = new IFFChunk(type, content);

            // Handle padding
            if (size % 2 != 0)
            {
                br.ReadByte(); // Skip padding byte
            }

            // Read subchunks if any remain in the current chunk's size
            var remainingSize = size - content.Length;
            while (remainingSize > 0)
            {
                var subChunk = IFFChunk.Read(br);
                chunk.AddSubChunk(subChunk);
                remainingSize -= subChunk.Size;
            }

            return chunk;
        }
    }
}
