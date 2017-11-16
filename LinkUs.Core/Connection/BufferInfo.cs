using System.Linq;

namespace LinkUs.Core.Connection
{
    public class BufferInfo
    {
        public BufferInfo() { }
        public BufferInfo(byte[] data)
        {
            Buffer = data;
            Length = data.Length;
            Offset = 0;
        }

        public byte[] Buffer { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }

        public byte[] ToBytes()
        {
            return Buffer
                .Skip(Offset)
                .Take(Length)
                .ToArray();
        }
    }
}