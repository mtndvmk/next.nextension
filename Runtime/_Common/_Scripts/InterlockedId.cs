using System.Threading;

namespace Nextension
{
    public class InterlockedId
    {
        private int _counter = 0;
        private readonly uint _minValue = 1;

        // the first id is initValue + 1
        public InterlockedId(uint initValue = 0) 
        {
            _counter = NConverter.bitConvert<uint, int>(initValue);
            _minValue = initValue + 1;
        }

        public uint nextId()
        {
            return __nextId(ref _counter, _minValue);
        }

        private static uint __nextId(ref int counter, uint umin)
        {
            int current, newValue;
            do
            {
                current = counter;
                bool isOverflow = NConverter.bitConvert<int, uint>(current) < umin;
                if (isOverflow)
                {
                    newValue = NConverter.bitConvert<uint, int>(umin);
                }
                else
                {
                    newValue = current + 1;
                }
            }
            while (Interlocked.CompareExchange(ref counter, newValue, current) != current);
            return NConverter.bitConvert<int, uint>(newValue);
        }
    }
}
