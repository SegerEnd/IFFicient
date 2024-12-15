using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFFicient
{
    public class IFFFile : IFFContainer
    {
        protected override void WriteHeader(BinaryWriter bw)
        {
            bw.Write("FORM".ToCharArray());
            int totalSize = Chunks.Sum(c => c.Size);
            bw.Write(totalSize); // Total size of all chunks
            bw.Write("SIM ".ToCharArray()); // Format ID for The Sims 1
        }
    }
}
