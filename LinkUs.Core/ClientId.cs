using System;

namespace LinkUs.Core
{
    public class ClientId
    {
        public static ClientId Server = new ClientId(new Guid());
        private readonly Guid _internalId;

        private ClientId(Guid internalId)
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
            if (obj is ClientId) {
                return ((ClientId) obj)._internalId == _internalId;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return _internalId.GetHashCode();
        }

        public static ClientId New()
        {
            return new ClientId(Guid.NewGuid());
        }
        public static ClientId FromBytes(byte[] bytes)
        {
            return new ClientId(new Guid(bytes));
        }
    }
}