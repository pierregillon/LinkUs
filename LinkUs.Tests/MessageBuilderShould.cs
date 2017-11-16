using System.Linq;
using LinkUs.Core.Connection;
using NFluent;
using Xunit;

namespace LinkUs.Tests
{
    public class MessageBuilderShould
    {
        private readonly byte[] A_MESSAGE = { 1, 2, 9 };
        private readonly byte[] SOME_ADDITIONAL_DATA = { 5, 4 };

        private readonly MessageBuilder _messageBuilder;

        public MessageBuilderShould()
        {
            _messageBuilder = new MessageBuilder();
        }

        [Fact]
        public void parse_simple_message()
        {
            // Actors
            var preparedData = BuildDataSliceFromMessage(A_MESSAGE);

            // Actions
            _messageBuilder.AddData(preparedData);

            // Asserts
            Check.That(_messageBuilder.GetBuiltMessage()).ContainsExactly(A_MESSAGE);
            Check.That(_messageBuilder.GetAdditionalData()).IsNull();
        }

        [Fact]
        public void parse_message_with_additional_data()
        {
            // Actors
            var preparedData = new ByteArraySlice(BuildDataSliceFromMessage(A_MESSAGE)
                                                      .ToBytes()
                                                      .Concat(SOME_ADDITIONAL_DATA)
                                                      .ToArray());

            // Actions
            _messageBuilder.AddData(preparedData);

            // Asserts
            Check.That(_messageBuilder.GetBuiltMessage()).ContainsExactly(A_MESSAGE);
            Check.That(_messageBuilder.GetAdditionalData().ToBytes()).ContainsExactly(SOME_ADDITIONAL_DATA);
        }

        [Fact]
        public void do_not_parse_when_data_not_completed()
        {
            // Actors
            var preparedData = BuildDataSliceFromMessage(A_MESSAGE).ReduceLength(3);

            // Actions
            _messageBuilder.AddData(preparedData);

            // Asserts
            Check.That(_messageBuilder.GetBuiltMessage()).IsNull();
            Check.That(_messageBuilder.GetAdditionalData()).IsNull();
        }

        // ----- Utils
        private ByteArraySlice BuildDataSliceFromMessage(byte[] message)
        {
            ByteArraySlice byteArraySlice;
            var sendProtocol = new SendBytesTransfertProtocol();
            sendProtocol.PrepareMessageToSend(message);
            sendProtocol.TryGetNextDataToSend(message.Length + 4, out byteArraySlice);
            return byteArraySlice;
        }
    }
}