using System.Collections.Generic;

namespace LinkUs.Core
{
    public class Metadata
    {
        public ClientId ClientId;
        public byte[] PackageLengthBytes = new byte[4];
        public int PackageLength = 0;
        public readonly List<byte[]> Buffers = new List<byte[]>();

        public void Reset()
        {
            ClientId = null;
            PackageLength = 0;
            Buffers.Clear();
        }
    }
}