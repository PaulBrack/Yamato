using System.Threading;

namespace MzmlParser
{
    public class ThreadPoolScheduler : ICouldRunCodeInParallel
    {
        private readonly CountdownEvent cde = new CountdownEvent(1);

        void ICouldRunCodeInParallel.Submit(WaitCallback callback)
        {
            cde.AddCount(1);
            ThreadPool.QueueUserWorkItem(state => { callback(state); cde.Signal(); });
        }

        void ICouldRunCodeInParallel.WaitForAll()
        {
            cde.Wait();
        }
    }
}
