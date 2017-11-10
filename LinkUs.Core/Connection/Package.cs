using System;
using System.Text;

namespace LinkUs.Core.Connection
{
    public class Package
    {
        public TransactionId TransactionId { get; set; } = TransactionId.New();
        public ClientId Source { get; private set; }
        public ClientId Destination { get; }
        public byte[] Content { get; }

        // ----- Constructor
        public Package(ClientId source, ClientId destination, byte[] content)
        {
            Source = source;
            Destination = destination;
            Content = content;
        }

        // ----- Public methods
        public Package CreateResponsePackage(byte[] response)
        {
            return new Package(Destination, Source, response) {TransactionId = TransactionId};
        }
        public byte[] ToByteArray()
        {
            var transactionIdBytes = TransactionId.ToByteArray();
            var sourceBytes = Source.ToByteArray();
            var destinationBytes = Destination.ToByteArray();

            var fullBytes = new byte[transactionIdBytes.Length + sourceBytes.Length + destinationBytes.Length + Content.Length];

            Buffer.BlockCopy(transactionIdBytes, 0, fullBytes, 0, transactionIdBytes.Length);
            Buffer.BlockCopy(sourceBytes, 0, fullBytes, transactionIdBytes.Length, sourceBytes.Length);
            Buffer.BlockCopy(destinationBytes, 0, fullBytes, transactionIdBytes.Length + sourceBytes.Length, destinationBytes.Length);
            Buffer.BlockCopy(Content, 0, fullBytes, transactionIdBytes.Length + sourceBytes.Length + destinationBytes.Length, Content.Length);

            return fullBytes;
        }
        public override string ToString()
        {
            return
                TransactionId + "|" +
                Source.ToShortString() + "|" +
                Destination.ToShortString() + "|" +
                Content.Length + " bytes";
        }

        // ----- Utils

        public static Package Parse(byte[] buffer)
        {
            var transactionIdBytes = new byte[16];
            var sourceBytes = new byte[16];
            var destinationBytes = new byte[16];
            var content = new byte[buffer.Length - transactionIdBytes.Length - sourceBytes.Length - destinationBytes.Length];

            Buffer.BlockCopy(buffer, 0, transactionIdBytes, 0, transactionIdBytes.Length);
            Buffer.BlockCopy(buffer, transactionIdBytes.Length, sourceBytes, 0, sourceBytes.Length);
            Buffer.BlockCopy(buffer, transactionIdBytes.Length + sourceBytes.Length, destinationBytes, 0, destinationBytes.Length);
            Buffer.BlockCopy(buffer, transactionIdBytes.Length + sourceBytes.Length + destinationBytes.Length, content, 0, content.Length);

            return new Package(ClientId.FromBytes(sourceBytes), ClientId.FromBytes(destinationBytes), content) {
                TransactionId = TransactionId.FromBytes(transactionIdBytes)
            };
        }
        public void ChangeSource(ClientId source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            Source = source;
        }
    }
}