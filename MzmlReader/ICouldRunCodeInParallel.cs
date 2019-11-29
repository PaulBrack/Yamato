using System.Threading;

namespace MzmlParser
{
    /// <summary>
    /// A convenient interface to implement work that might be run in serial or parallel depending on the implementation.
    /// To use: instantiate an appropriate class, keep submitting work via Submit (which might run immediately or be scheduled), then call WaitForAll which will return only when all work is completed.
    /// </summary>
    public interface ICouldRunCodeInParallel
    {
        void Submit(WaitCallback callback);
        void WaitForAll();
    }
}
