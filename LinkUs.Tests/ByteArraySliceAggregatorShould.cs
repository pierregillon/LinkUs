using System.Linq;
using LinkUs.Core.Connection;
using NFluent;
using Xunit;

namespace LinkUs.Tests
{
    public class ByteArraySliceAggregatorShould
    {
        private readonly byte[] A_MESSAGE = { 1, 2, 9 };
        private readonly byte[] SOME_ADDITIONAL_DATA = { 5, 4 };

        private readonly ByteArraySliceAggregator _byteArraySliceAggregator;

        public ByteArraySliceAggregatorShould()
        {
            _byteArraySliceAggregator = new ByteArraySliceAggregator();
        }

        [Fact]
        public void build_message_when_aggregating_full_message()
        {
            // Actors
            var fullMessageSlice = GetFullMessageSlice(A_MESSAGE);

            // Actions
            _byteArraySliceAggregator.Aggregate(fullMessageSlice);

            // Asserts
            Check.That(_byteArraySliceAggregator.GetBuiltMessage()).ContainsExactly(A_MESSAGE);
            Check.That(_byteArraySliceAggregator.GetAdditionalData()).IsNull();
        }

        [Fact]
        public void build_message_and_additional_data_when_aggregating_a_bigger_message_that_expected()
        {
            // Actors
            var fullSliceAndAdditionalData = new ByteArraySlice(GetFullMessageSlice(A_MESSAGE)
                                                                    .ToBytes()
                                                                    .Concat(SOME_ADDITIONAL_DATA)
                                                                    .ToArray());

            // Actions
            _byteArraySliceAggregator.Aggregate(fullSliceAndAdditionalData);

            // Asserts
            Check.That(_byteArraySliceAggregator.GetBuiltMessage()).ContainsExactly(A_MESSAGE);
            Check.That(_byteArraySliceAggregator.GetAdditionalData().ToBytes()).ContainsExactly(SOME_ADDITIONAL_DATA);
        }

        [Fact]
        public void do_not_build_final_message_if_slice_is_not_enough()
        {
            // Actors
            var smallSlice = GetFullMessageSlice(A_MESSAGE).ReduceLength(3);

            // Actions
            _byteArraySliceAggregator.Aggregate(smallSlice);

            // Asserts
            Check.That(_byteArraySliceAggregator.GetBuiltMessage()).IsNull();
            Check.That(_byteArraySliceAggregator.GetAdditionalData()).IsNull();
        }

        // ----- Utils
        private static ByteArraySlice GetFullMessageSlice(byte[] message)
        {
            ByteArraySlice byteArraySlice;
            var slicer = new ByteArraySlicer();
            slicer.DefineMessageToSlice(message);
            slicer.TryGetNextSlice(message.Length + 4, out byteArraySlice);
            return byteArraySlice;
        }
    }
}