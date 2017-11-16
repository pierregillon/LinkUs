namespace LinkUs.Core.Connection
{
    public class SocketAsyncOperationPool : SemaphoredQueue<SocketAsyncOperation>
    {
        public SocketAsyncOperationPool(int maxCount) : base(maxCount)
        {
            for (int i = 0; i < maxCount; i++) {
                Enqueue(new SocketAsyncOperation());
            }
        }
    }
}