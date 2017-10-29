using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using LinkUs.Core;

namespace LinkUs
{
    public class Metadata
    {
        public ClientId ClientId;
        public List<byte[]> Buffers;
        public byte[] PackageLengthBytes = new byte[4];
        public int PackageLength = 0;

        public void Reset()
        {
            ClientId = null;
            Buffers.Clear();
            PackageLength = 0;
        }
    }
}