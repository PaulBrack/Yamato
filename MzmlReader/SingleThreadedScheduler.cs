using System.Threading;

namespace MzmlParser
{
    public class SingleThreadedScheduler : ICouldRunCodeInParallel
    {
        void ICouldRunCodeInParallel.Submit(WaitCallback callback)
        {
            // Run immediately
            callback(null);
        }

        void ICouldRunCodeInParallel.WaitForAll()
        {
            // Everything's already been run in series, so nothing more to do.
            return;
        }
    }
}
