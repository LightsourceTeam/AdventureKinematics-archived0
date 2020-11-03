using System;
using System.Threading;

namespace SourceExtensions
{
    public class UnlockObject : IDisposable
    {
        object target;

        public static UnlockObject unlock(object obj) => new UnlockObject(obj);

        public UnlockObject(object obj)
        {
            target = obj;
            Monitor.Exit(obj);
        }

        public void Dispose()
        {
            Monitor.Enter(target);
        }
    }
}
