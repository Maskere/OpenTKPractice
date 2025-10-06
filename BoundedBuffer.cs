namespace Practice{
    public class BoundedBuffer<T>{
        private readonly Queue<T> queue;
        private readonly Semaphore empty;
        private readonly Semaphore full;
        private readonly object lockObj;

        private int inQueue;

        public BoundedBuffer(int queueSize){
            queue = new(queueSize);
            empty = new(0,queueSize);
            full = new(queueSize,queueSize);
            lockObj = new();
        }

        public T Insert(T item){
            full.WaitOne();
            lock(lockObj){
                inQueue++;
                queue.Enqueue(item);
            }
            empty.Release();
            return item;
        }

        public T Take(){
            empty.WaitOne();
            T result;
            lock(lockObj){
                inQueue--;
                result = queue.Dequeue();
            }
            full.Release();
            return result;
        }

        public int GetInQueue(){
            return inQueue;
        }
    }
}
