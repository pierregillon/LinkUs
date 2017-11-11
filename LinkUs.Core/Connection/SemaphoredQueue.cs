using System.Collections.Concurrent;
using System.Threading;

namespace LinkUs.Core.Connection
{
    public class SemaphoredQueue<T>
    {
        private readonly Semaphore _lock = new Semaphore(0, 10);
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

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