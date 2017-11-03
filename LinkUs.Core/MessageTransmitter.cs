using System;
using System.Collections.Generic;
using System.Linq;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;

namespace LinkUs.Core
{
    public class MessageTransmitter
    {
        private readonly PackageTransmitter _packageTransmitter;
        private readonly ISerializer _serializer;
        private readonly List<Type> _managedTypes = new List<Type>();

        public event Action<Envelop> MessageReceived;
        public event Action Closed;

        // ----- Constructor
        public MessageTransmitter(PackageTransmitter packageTransmitter, ISerializer serializer)
        {
            _packageTransmitter = packageTransmitter;
            _serializer = serializer;
            _packageTransmitter.PackageReceived += PackageTransmitterOnPackageReceived;
            _packageTransmitter.Closed += PackageTransmitterOnClosed;

            var types = typeof(Message).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(Message)));
            _managedTypes.AddRange(types);
        }

        // ----- Public methods
        public void Send(Envelop envelop)
        {
            var content = _serializer.Serialize(envelop.Message);
            var package = new Package(ClientId.Unknown, envelop.Sender, content) {
                TransactionId = envelop.TransactionId
            };
            _packageTransmitter.Send(package);
        }
        public void Close()
        {
            _packageTransmitter.Close();
            _packageTransmitter.PackageReceived -= PackageTransmitterOnPackageReceived;
            _packageTransmitter.Closed -= PackageTransmitterOnClosed;
        }

        // ----- Callbacks
        private void PackageTransmitterOnPackageReceived(object sender, Package package)
        {
            var message = _serializer.Deserialize<MessageDescriptor>(package.Content);
            var messageType = _managedTypes.Single(x => x.Name == message.Name);
            var messageInstance = (Message) _serializer.Deserialize(package.Content, messageType);
            var envelop = new Envelop(messageInstance, package);
            MessageReceived?.Invoke(envelop);
        }
        private void PackageTransmitterOnClosed(object sender, EventArgs eventArgs)
        {
            Closed?.Invoke();
        }
    }

    public class Envelop
    {
        public Message Message { get; }
        public ClientId Sender { get; }
        public TransactionId TransactionId { get; }

        public Envelop(Message message, ClientId sender) : this(message, sender, TransactionId.New()) { }
        public Envelop(Message message, Package package) : this(message, package.Source, package.TransactionId) { }
        private Envelop(Message message, ClientId sender, TransactionId transactionId)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (sender == null) throw new ArgumentNullException(nameof(sender));

            Message = message;
            Sender = sender;
            TransactionId = transactionId;
        }
        public Envelop CreateReturn(Message message)
        {
            return new Envelop(message, Sender, TransactionId);
        }
    }
}