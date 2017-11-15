using System.Linq;
using LinkUs.Core.Connection;
using NFluent;
using Xunit;

namespace LinkUs.Tests
{
    public class BytesTransfertProtocolShould
    {
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
            var preparedData = _protocol.PrepareMessageToSend(A_MESSAGE);

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
            var dataToSend = _protocol
                .PrepareMessageToSend(A_MESSAGE)
                .Concat(SOME_ADDITIONAL_DATA)
                .ToArray();

            // Actions
            ParsedData parsedData;
            var isParsingSuccessful = _protocol.TryParse(dataToSend, out parsedData);

            // Asserts
            Check.That(isParsingSuccessful).IsTrue();
            Check.That(parsedData.Message).ContainsExactly(A_MESSAGE);
            Check.That(parsedData.ContainsAdditionalData()).IsTrue();
            Check.That(parsedData.AdditionalData).ContainsExactly(SOME_ADDITIONAL_DATA);
        }

        [Fact]
        public void do_not_parse_when_data_not_completed()
        {
            // Actors
            var dataToSend = _protocol
                .PrepareMessageToSend(A_MESSAGE)
                .Take(3)
                .ToArray();

            // Actions
            ParsedData parsedData;
            var isParsingSuccessful = _protocol.TryParse(dataToSend, out parsedData);

            // Asserts
            Check.That(isParsingSuccessful).IsFalse();
            Check.That(parsedData.IsEmpty()).IsTrue();
            Check.That(parsedData.Message).IsNull();
            Check.That(parsedData.AdditionalData).IsNull();
        }
    }
}