using System.Collections.Concurrent;
using System.Threading;

namespace LinkUs.Core.Connection
{
    public class SemaphoredQueue<T>
    {
        private readonly Semaphore _lock;
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        public SemaphoredQueue(int maxCount)
        {
            _lock = new Semaphore(0, maxCount);
        }

        public void Enqueue(T element)
        {
            _queue.Enqueue(element);
            _lock.Release();
        }

        public T Dequeue()
        {
            _lock.WaitOne();
            T element;
            while (_queue.TryDequeue(out element) == false) {
                Thread.Sleep(10);
            }
            return element;
        }
    }
}