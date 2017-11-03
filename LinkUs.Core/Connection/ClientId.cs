using System;

namespace LinkUs.Core.Connection
{
    public class ClientId
    {
        public static ClientId Server = new ClientId(new Guid());
        public static ClientId Unknown = new ClientId(new Guid("11111111-1111-1111-1111-111111111111"));
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
            return _internalId.ToString();
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
        public static ClientId Parse(string value)
        {
            return new ClientId(Guid.Parse(value));
        }
    }
}