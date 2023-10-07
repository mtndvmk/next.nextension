using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    /// <summary>
    /// Get a number that doesn't use or a number that is greater than the highest used number
    /// </summary>
    public class NNextUInt32
    {
        /// <summary>
        /// ReadyNumber Pool
        /// </summary>
        private Stack<int> _readyNumbers;
        /// <summary>
        /// UsedNumber Pool
        /// </summary>
        private HashSet<int> _usedNumbers;

        public readonly uint start;
        public readonly uint max;

        public int HighestUsedNumber { get; private set; }
        public NNextUInt32(uint start = 0, uint max = uint.MaxValue)
        {
            if (max < start) throw new Exception("max must greater or equal start");
            this.start = start;
            this.max = max;
            _readyNumbers = new Stack<int>();
            _usedNumbers = new HashSet<int>();
        }

        public int getNext()
        {
            int num;
            if (_readyNumbers.Count > 0)
            {
                num = _readyNumbers.Pop();
                _usedNumbers.Add(num);
                return num;
            }

            if (HighestUsedNumber == max)
            {
                throw new Exception("Reached maximum value");
            }

            num = HighestUsedNumber++;
            _usedNumbers.Add(num);
            return num;
        }
        public void release(int num)
        {
            if (_usedNumbers.Contains(num))
            {
                _readyNumbers.Push(num);
                _usedNumbers.Remove(num);
            }
            else
            {
                Debug.LogError("UseNumber doesn't contain " + num);
            }
        }

        public void reset()
        {
            HighestUsedNumber = 0;
            _usedNumbers.Clear();
            _readyNumbers.Clear();
        }
    }
}
