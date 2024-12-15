using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFFicient
{
    public class FARFile : IFFContainer
    {
        protected override void WriteHeader(BinaryWriter bw)
        {
            bw.Write("FAR ".ToCharArray());
            bw.Write(Chunks.Count); // Number of chunks/files
        }

        protected override void WriteChunk(BinaryWriter bw, IFFChunk chunk)
        {
            // For FAR, we write the filename then the content of the IFF file
            bw.Write(chunk.Type.ToCharArray());
            bw.Write(chunk.Content.Length);
            bw.Write(chunk.Content);
            if (chunk.Type.Length % 2 != 0) bw.Write((byte)0); // Padding for filename if odd
        }
    }
}
