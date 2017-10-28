using System;
using System.Text;

namespace LinkUs.Core
{
    public class ClientId
    {
        private static readonly UTF8Encoding Encoding = new UTF8Encoding();
        private static string _internalId;

        private ClientId(string internalId)
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

        public static ClientId New()
        {
            return new ClientId(Guid.NewGuid().ToString().Substring(12 + 4));
        }
        public static ClientId FromBytes(byte[] bytes)
        {
            return new ClientId(Encoding.GetString(bytes));
        }
    }
}