using System;
using UnityEngine;

namespace Nextension
{
    public static class NWaitableExtensions
    {
        public static async NTask wait(this AsyncOperation waitable)
        {
            await new NWaitAsyncOperation(waitable);
        }
        public static void waitAndRun<TWaitable>(this TWaitable waitable, Action next) where TWaitable : IWaitable
        {
            NTask.waitAndRun(waitable, next);
        }
    }

    public static class NCustomWaitableExtensions
    {
        public static NCustomWaitable wait(this ICustomWaitable waitable)
        {
            return new NCustomWaitable(waitable);
        }
        public static async NTask waitAsTask(this ICustomWaitable waitable)
        {
            await new NCustomWaitable(waitable);
        }
    }
}
