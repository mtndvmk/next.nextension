#if UNITY_EDITOR
using System;
using System.Threading.Tasks;

namespace Nextension
{
    public static class TaskEditor
    {
        public static async Task waitFrame(int count = 1)
        {
            while (count-- > 0)
            {
                await Task.Yield();
            }
        }
        public static async Task waitForSeconds(float seconds)
        {
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var targetTime = currentTime + (long)(seconds * 1000);
            while (currentTime < targetTime)
            {
                await Task.Yield();
                currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }
        public static async Task waitUntil(Func<bool> predicate)
        {
            while (!predicate())
            {
                await Task.Yield();
            }
        }
    }
}
#endif