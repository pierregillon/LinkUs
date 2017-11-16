using System.Linq;

namespace LinkUs.Core.Connection
{
    public class ByteArraySlice
    {
        public byte[] Buffer { get; }
        public int Offset { get; }
        public int Length { get; }

        public ByteArraySlice(byte[] data)
        {
            Buffer = data;
            Length = data.Length;
            Offset = 0;
        }
        public ByteArraySlice(byte[] data, int length)
        {
            Buffer = data;
            Length = length;
            Offset = 0;
        }
        public ByteArraySlice(byte[] data, int length, int offset)
        {
            Buffer = data;
            Length = length;
            Offset = offset;
        }

        public byte[] ToBytes()
        {
            return Buffer
                .Skip(Offset)
                .Take(Length)
                .ToArray();
        }
        public ByteArraySlice ReduceSizeFromLeft(int offsetCount)
        {
            if (offsetCount == 0) {
                return this;
            }
            return new ByteArraySlice(Buffer, Length - offsetCount, Offset + offsetCount);
        }
        public ByteArraySlice ReduceLength(int newLength)
        {
            if (newLength == Length) {
                return this;
            }
            return new ByteArraySlice(Buffer, newLength, Offset);
        }
    }
}