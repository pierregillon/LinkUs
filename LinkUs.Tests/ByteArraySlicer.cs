using System;
using LinkUs.Core.Connection;
using NFluent;
using Xunit;

namespace LinkUs.Tests
{
    public class ByteArraySlicerShould
    {
        private const int DEFAULT_DATA_SIZE = 1024;
        private readonly byte[] A_MESSAGE = { 1, 2, 9 };

        private readonly ByteArraySlicer _byteArraySlicer;

        public ByteArraySlicerShould()
        {
            _byteArraySlicer = new ByteArraySlicer();
        }

        [Fact]
        public void throw_error_when_trying_to_ACK_more_bytes_than_sliced()
        {
            // Actors
            var bytes = GetFullDataSlice(A_MESSAGE);

            // Actions
            Action action = () => _byteArraySlicer.AcquitBytes(bytes.Length + 1);

            // Asserts
            Check.ThatCode(action)
                 .Throws<Exception>()
                 .WithMessage("Cannot ACK more bytes than the message contains.");
        }

        [Fact]
        public void throw_error_when_trying_to_get_next_slice_but_message_not_prepared()
        {
            // Acts
            ByteArraySlice byteArraySlice;
            Action action = () => _byteArraySlicer.TryGetNextSlice(DEFAULT_DATA_SIZE, out byteArraySlice);

            // Asserts
            Check.ThatCode(action)
                 .Throws<Exception>()
                 .WithMessage("Unable to get next data slice, no message defined.");
        }

        [Fact]
        public void build_first_slice_that_contains_length_of_the_message()
        {
            // Data
            ByteArraySlice slice;
            byte[] message = { 1, 2, 3 };

            // Acts
            _byteArraySlicer.DefineMessageToSlice(message);
            _byteArraySlicer.TryGetNextSlice(DEFAULT_DATA_SIZE, out slice);

            // Asserts
            Check.That(slice.ToBytes()).ContainsExactly(new byte[] { 3, 0, 0, 0, 1, 2, 3 });
        }

        [Fact]
        public void limit_data_to_send_to_the_given_data_size()
        {
            // Data
            ByteArraySlice slice;
            byte[] message = { 1, 2, 3 };
            const int TWO_BYTES_PER_SLICE = 2;

            // Acts
            _byteArraySlicer.DefineMessageToSlice(message);
            _byteArraySlicer.TryGetNextSlice(TWO_BYTES_PER_SLICE, out slice);

            // Asserts
            Check.That(slice.ToBytes()).ContainsExactly(new byte[] { 3, 0 });
        }

        [Fact]
        public void return_all_slices_util_message_is_built()
        {
            // Data
            ByteArraySlice byteArraySlice;
            const int defaultBufferSize = 5;
            byte[] message = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            // Acts
            _byteArraySlicer.DefineMessageToSlice(message);
            _byteArraySlicer.TryGetNextSlice(defaultBufferSize, out byteArraySlice);
            Check.That(byteArraySlice.ToBytes()).ContainsExactly(new byte[] { 9, 0, 0, 0, 1 });

            // Acts
            _byteArraySlicer.AcquitBytes(defaultBufferSize);
            _byteArraySlicer.TryGetNextSlice(defaultBufferSize, out byteArraySlice);
            Check.That(byteArraySlice.ToBytes()).ContainsExactly(new byte[] { 2, 3, 4, 5, 6 });

            // Acts 2
            _byteArraySlicer.AcquitBytes(defaultBufferSize);
            _byteArraySlicer.TryGetNextSlice(defaultBufferSize, out byteArraySlice);
            Check.That(byteArraySlice.ToBytes()).ContainsExactly(new byte[] { 7, 8, 9 });

            // Acts 3
            _byteArraySlicer.AcquitBytes(3);
            var isSucceded = _byteArraySlicer.TryGetNextSlice(defaultBufferSize, out byteArraySlice);
            Check.That(isSucceded).IsFalse();
        }

        // ----- Utils
        private ByteArraySlice GetFullDataSlice(byte[] message)
        {
            ByteArraySlice byteArraySlice;
            _byteArraySlicer.DefineMessageToSlice(message);
            _byteArraySlicer.TryGetNextSlice(message.Length + 4, out byteArraySlice);
            return byteArraySlice;
        }
    }
}