namespace Practice{
    public class BoundedBuffer<T>{
        private readonly Queue<T> queue;
        private readonly Semaphore empty;
        private readonly Semaphore full;
        private readonly object lockObj;

        public BoundedBuffer(int queueSize){
            queue = new(queueSize);
            empty = new(0,queueSize);
            full = new(queueSize,queueSize);
            lockObj = new();
        }

        public T Insert(T item){
            full.WaitOne();
            lock(lockObj){
                queue.Enqueue(item);
            }
            empty.Release();
            return item;
        }

        public T Take(){
            empty.WaitOne();
            T result;
            lock(lockObj){
                result = queue.Dequeue();
            }
            full.Release();
            return result;
        }

        public int GetInQueue(){
            lock(lockObj){
                return queue.Count;
            }
        }
    }
}
