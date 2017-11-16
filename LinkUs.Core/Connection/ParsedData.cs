namespace LinkUs.Core.Connection
{
    public class ParsedData
    {
        public byte[] Message { get; private set; }
        public ByteArraySlice AdditionalData { get; private set; }

        private ParsedData() { }

        public static ParsedData None()
        {
            return new ParsedData();
        }
        public static ParsedData OnlyMessage(byte[] message)
        {
            return new ParsedData {
                Message = message,
                AdditionalData = null
            };
        }
        public static ParsedData MessageAndAdditionalData(byte[] message, ByteArraySlice additionalData)
        {
            return new ParsedData {
                Message = message,
                AdditionalData = additionalData
            };
        }

        public bool ContainsAdditionalData()
        {
            return AdditionalData != null;
        }
        public bool IsEmpty()
        {
            return Message == null && AdditionalData == null;
        }
    }
}