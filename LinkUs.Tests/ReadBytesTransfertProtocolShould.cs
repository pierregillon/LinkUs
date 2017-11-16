using System.Linq;
using LinkUs.Core.Connection;
using NFluent;
using Xunit;

namespace LinkUs.Tests
{
    public class ReadBytesTransfertProtocolShould
    {
        private readonly byte[] A_MESSAGE = { 1, 2, 9 };
        private readonly byte[] SOME_ADDITIONAL_DATA = { 5, 4 };

        private readonly ReadBytesTransfertProtocol _protocol;

        public ReadBytesTransfertProtocolShould()
        {
            _protocol = new ReadBytesTransfertProtocol();
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
            var bufferInfo = new ByteArraySlice(dataToSend);

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
            var dataToSend = GetDataToSendFromMessage(A_MESSAGE).ReduceLength(3);

            // Actions
            ParsedData parsedData;
            var isParsingSuccessful = _protocol.TryParse(dataToSend, out parsedData);

            // Asserts
            Check.That(isParsingSuccessful).IsFalse();
            Check.That(parsedData.IsEmpty()).IsTrue();
            Check.That(parsedData.Message).IsNull();
            Check.That(parsedData.AdditionalData).IsNull();
        }

        // ----- Utils
        private ByteArraySlice GetDataToSendFromMessage(byte[] message)
        {
            ByteArraySlice byteArraySlice;
            var sendProtocol = new SendBytesTransfertProtocol();
            sendProtocol.PrepareMessageToSend(message);
            sendProtocol.TryGetNextDataToSend(message.Length + 4, out byteArraySlice);
            return byteArraySlice;
        }
    }
}