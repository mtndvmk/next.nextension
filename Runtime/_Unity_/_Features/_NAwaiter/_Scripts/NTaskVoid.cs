using System.Runtime.CompilerServices;

namespace Nextension
{
    [AsyncMethodBuilder(typeof(AsyncNTaskVoidBuilder))]
    public struct NTaskVoid
    {
        public void forget()
        {

        }

    }
}
