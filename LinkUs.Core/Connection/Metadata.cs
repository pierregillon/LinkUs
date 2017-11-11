﻿using System.Collections.Generic;

namespace LinkUs.Core.Connection
{
    public class Metadata
    {
        public ClientId ClientId;
        public byte[] PackageLengthBytes = new byte[4];
        public int PackageLength = 0;
        public readonly List<byte[]> Buffers = new List<byte[]>();
        public int PackageLengthReceivedBytesCount;

        public void Reset()
        {
            ClientId = null;
            PackageLength = 0;
            PackageLengthReceivedBytesCount = 0;
            Buffers.Clear();
        }
    }
}