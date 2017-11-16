using System;
using LinkUs.Core.Connection;
using NFluent;
using Xunit;

namespace LinkUs.Tests
{
    public class SendBytesTransfertProtocolShould
    {
        private const int DEFAULT_DATA_SIZE = 1024;
        private readonly byte[] A_MESSAGE = { 1, 2, 9 };

        private readonly SendBytesTransfertProtocol _protocol;

        public SendBytesTransfertProtocolShould()
        {
            _protocol = new SendBytesTransfertProtocol();
        }

        [Fact]
        public void throw_error_when_trying_to_ACK_more_bytes_than_sent()
        {
            // Actors
            var bytes = GetDataToSendFromMessage(A_MESSAGE);

            // Actions
            Action action = () => _protocol.AcquitSentBytes(bytes.Length + 1);

            // Asserts
            Check.ThatCode(action)
                 .Throws<Exception>()
                 .WithMessage("Cannot ACK more bytes than the message contains.");
        }

        [Fact]
        public void throw_error_when_trying_to_get_next_data_to_send_but_message_not_prepared()
        {
            // Acts
            ByteArraySlice byteArraySlice;
            Action action = () => _protocol.TryGetNextDataToSend(DEFAULT_DATA_SIZE, out byteArraySlice);

            // Asserts
            Check.ThatCode(action)
                 .Throws<Exception>()
                 .WithMessage("Unable to get next data to send, no message prepared.");
        }

        [Fact]
        public void concatenate_data_length_with_data()
        {
            // Data
            ByteArraySlice byteArraySlice;
            byte[] message = { 1, 2, 3 };

            // Acts
            _protocol.PrepareMessageToSend(message);
            _protocol.TryGetNextDataToSend(DEFAULT_DATA_SIZE, out byteArraySlice);

            // Asserts
            Check.That(byteArraySlice.ToBytes()).ContainsExactly(new byte[] { 3, 0, 0, 0, 1, 2, 3 });
        }

        [Fact]
        public void limit_data_to_send_to_the_given_data_size()
        {
            // Data
            ByteArraySlice byteArraySlice;
            byte[] message = { 1, 2, 3 };
            const int dataSize = 2;

            // Acts
            _protocol.PrepareMessageToSend(message);
            _protocol.TryGetNextDataToSend(dataSize, out byteArraySlice);

            // Asserts
            Check.That(byteArraySlice.ToBytes()).ContainsExactly(new byte[] { 3, 0 });
        }

        [Fact]
        public void get_next_data_to_send_multiple_time()
        {
            // Data
            ByteArraySlice byteArraySlice;
            const int defaultBufferSize = 5;
            byte[] message = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            // Acts
            _protocol.PrepareMessageToSend(message);
            _protocol.TryGetNextDataToSend(defaultBufferSize, out byteArraySlice);
            Check.That(byteArraySlice.ToBytes()).ContainsExactly(new byte[] { 9, 0, 0, 0, 1 });

            // Acts
            _protocol.AcquitSentBytes(defaultBufferSize);
            _protocol.TryGetNextDataToSend(defaultBufferSize, out byteArraySlice);
            Check.That(byteArraySlice.ToBytes()).ContainsExactly(new byte[] { 2, 3, 4, 5, 6 });

            // Acts 2
            _protocol.AcquitSentBytes(defaultBufferSize);
            _protocol.TryGetNextDataToSend(defaultBufferSize, out byteArraySlice);
            Check.That(byteArraySlice.ToBytes()).ContainsExactly(new byte[] { 7, 8, 9 });

            // Acts 3
            _protocol.AcquitSentBytes(3);
            var isSucceded = _protocol.TryGetNextDataToSend(defaultBufferSize, out byteArraySlice);
            Check.That(isSucceded).IsFalse();
        }

        // ----- Utils
        private ByteArraySlice GetDataToSendFromMessage(byte[] message)
        {
            ByteArraySlice byteArraySlice;
            _protocol.PrepareMessageToSend(message);
            _protocol.TryGetNextDataToSend(message.Length + 4, out byteArraySlice);
            return byteArraySlice;
        }
    }
}