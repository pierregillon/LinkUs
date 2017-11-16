using System;
using System.Linq;
using LinkUs.Core.Connection;
using NFluent;
using Xunit;

namespace LinkUs.Tests
{
    public class BytesTransfertProtocolShould
    {
        private const int DEFAULT_DATA_SIZE = 1024;
        private readonly byte[] A_MESSAGE = { 1, 2, 9 };
        private readonly byte[] SOME_ADDITIONAL_DATA = { 5, 4 };

        private readonly BytesTransfertProtocol _protocol;

        public BytesTransfertProtocolShould()
        {
            _protocol = new BytesTransfertProtocol();
        }

        [Fact]
        public void parse_simple_message()
        {
            // Actors
            var preparedData = GetDataToSendFromMessage(A_MESSAGE);

            // Actions
            ParsedData parsedData;
            var result = _protocol.TryParse(preparedData, out parsedData);

            // Asserts
            Check.That(result).IsTrue();
            Check.That(parsedData.Message).ContainsExactly(A_MESSAGE);
            Check.That(parsedData.ContainsAdditionalData()).IsFalse();
        }

        [Fact]
        public void parse_message_with_additional_data()
        {
            // Actors
            var dataToSend = GetDataToSendFromMessage(A_MESSAGE).ToBytes().Concat(SOME_ADDITIONAL_DATA).ToArray();
            var bufferInfo = new BufferInfo(dataToSend);

            // Actions
            ParsedData parsedData;
            var isParsingSuccessful = _protocol.TryParse(bufferInfo, out parsedData);

            // Asserts
            Check.That(isParsingSuccessful).IsTrue();
            Check.That(parsedData.Message).ContainsExactly(A_MESSAGE);
            Check.That(parsedData.ContainsAdditionalData()).IsTrue();
            Check.That(parsedData.AdditionalData.ToBytes()).ContainsExactly(SOME_ADDITIONAL_DATA);
        }

        [Fact]
        public void do_not_parse_when_data_not_completed()
        {
            // Actors
            var dataToSend = GetDataToSendFromMessage(A_MESSAGE);
            dataToSend.Length = 3;

            // Actions
            ParsedData parsedData;
            var isParsingSuccessful = _protocol.TryParse(dataToSend, out parsedData);

            // Asserts
            Check.That(isParsingSuccessful).IsFalse();
            Check.That(parsedData.IsEmpty()).IsTrue();
            Check.That(parsedData.Message).IsNull();
            Check.That(parsedData.AdditionalData).IsNull();
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
            BufferInfo bufferInfo;
            Action action = () => _protocol.TryGetNextDataToSend(DEFAULT_DATA_SIZE, out bufferInfo);

            // Asserts
            Check.ThatCode(action)
                 .Throws<Exception>()
                 .WithMessage("Unable to get next data to send, no message prepared.");
        }

        [Fact]
        public void concatenate_data_length_with_data()
        {
            // Data
            BufferInfo bufferInfo;
            byte[] message = { 1, 2, 3 };

            // Acts
            _protocol.PrepareMessageToSend(message);
            _protocol.TryGetNextDataToSend(DEFAULT_DATA_SIZE, out bufferInfo);

            // Asserts
            Check.That(bufferInfo.ToBytes()).ContainsExactly(new byte[] { 3, 0, 0, 0, 1, 2, 3 });
        }

        [Fact]
        public void limit_data_to_send_to_the_given_data_size()
        {
            // Data
            BufferInfo bufferInfo;
            byte[] message = { 1, 2, 3 };
            const int dataSize = 2;

            // Acts
            _protocol.PrepareMessageToSend(message);
            _protocol.TryGetNextDataToSend(dataSize, out bufferInfo);

            // Asserts
            Check.That(bufferInfo.ToBytes()).ContainsExactly(new byte[] { 3, 0 });
        }

        [Fact]
        public void get_next_data_to_send_multiple_time()
        {
            // Data
            BufferInfo bufferInfo;
            const int defaultBufferSize = 5;
            byte[] message = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            // Acts
            _protocol.PrepareMessageToSend(message);
            _protocol.TryGetNextDataToSend(defaultBufferSize, out bufferInfo);
            Check.That(bufferInfo.ToBytes()).ContainsExactly(new byte[] { 9, 0, 0, 0, 1 });

            // Acts
            _protocol.AcquitSentBytes(defaultBufferSize);
            _protocol.TryGetNextDataToSend(defaultBufferSize, out bufferInfo);
            Check.That(bufferInfo.ToBytes()).ContainsExactly(new byte[] { 2, 3, 4, 5, 6 });

            // Acts 2
            _protocol.AcquitSentBytes(defaultBufferSize);
            _protocol.TryGetNextDataToSend(defaultBufferSize, out bufferInfo);
            Check.That(bufferInfo.ToBytes()).ContainsExactly(new byte[] { 7, 8, 9 });

            // Acts 3
            _protocol.AcquitSentBytes(3);
            var isSucceded = _protocol.TryGetNextDataToSend(defaultBufferSize, out bufferInfo);
            Check.That(isSucceded).IsFalse();
        }

        // ----- Utils
        private BufferInfo GetDataToSendFromMessage(byte[] message)
        {
            BufferInfo bufferInfo;
            _protocol.PrepareMessageToSend(message);
            _protocol.TryGetNextDataToSend(message.Length + 4, out bufferInfo);
            return bufferInfo;
        }
    }
}