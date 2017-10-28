using System;

namespace LinkUs.Core
{
    public class TransactionId
    {
        private readonly Guid _internalId;

        private TransactionId(Guid internalId)
        {
            _internalId = internalId;
        }

        public byte[] ToByteArray()
        {
            return _internalId.ToByteArray();
        }
        public override string ToString()
        {
            return _internalId.ToString().Substring(30);
        }
        public override bool Equals(object obj)
        {
            if (obj is TransactionId) {
                return ((TransactionId) obj)._internalId == _internalId;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return _internalId.GetHashCode();
        }

        public static TransactionId New()
        {
            return new TransactionId(Guid.NewGuid());
        }
        public static TransactionId FromBytes(byte[] bytes)
        {
            return new TransactionId(new Guid(bytes));
        }
    }
}