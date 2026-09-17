using System.Threading;

namespace Nextension
{
    public class InterlockedId
    {
        private int _counter;
        private readonly uint _minValue;

        public InterlockedId(uint initValue = 0)
        {
            _counter = unchecked((int)initValue);
            _minValue = initValue == uint.MaxValue ? 1 : initValue + 1;
        }

        public uint nextId()
        {
            int current, next;
            do
            {
                current = _counter;
                uint uCurrent = unchecked((uint)current);

                uint uNext = uCurrent == uint.MaxValue ? _minValue : uCurrent + 1;
                next = unchecked((int)uNext);
            }
            while (Interlocked.CompareExchange(ref _counter, next, current) != current);

            return unchecked((uint)next);
        }
    }
}
