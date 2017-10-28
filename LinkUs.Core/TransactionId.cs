using System;
using System.Text;

namespace LinkUs.Core {
    public class TransactionId
    {
        private static readonly UTF8Encoding Encoding = new UTF8Encoding();
        private static string _internalId;

        private TransactionId(string internalId)
        {
            _internalId = internalId;
        }

        public byte[] ToByteArray()
        {
            return Encoding.GetBytes(_internalId);
        }
        public override string ToString()
        {
            return _internalId;
        }

        public static TransactionId New()
        {
            return new TransactionId(Guid.NewGuid().ToString().Substring(12 + 4));
        }
        public static TransactionId FromBytes(byte[] bytes)
        {
            return new TransactionId(Encoding.GetString(bytes));
        }
    }
}