using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    /// <summary>
    /// Get a number that doesn't use or a number that is greater than the highest used number
    /// </summary>
    public class NNextNumber
    {
        public event Action onCapacityIsFullEvent;
        /// <summary>
        /// ReadyNumber Pool
        /// </summary>
        private Stack<int> m_ReadyNumber;
        /// <summary>
        /// UsedNumber Pool
        /// </summary>
        private HashSet<int> m_UsedNumber;
        public int HighestUsedNumber { get; private set; }
        public int MaxNumber { get; private set; }
        public NNextNumber(int max)
        {
            MaxNumber = max;
            m_ReadyNumber = new Stack<int>();
            m_UsedNumber = new HashSet<int>();
        }
        /// <summary>
        /// Set MaxNumber to 'newMax' if 'newMax' is greater than old MaxNumber
        /// </summary>
        public void setMaxNumber(int newMax)
        {
            if (newMax < MaxNumber)
            {
                Debug.LogWarning("New MaxNumber must larger old MaxNumber");
            }
            else
            {
                MaxNumber = newMax;
            }
        }
        /// <summary>
        /// Get Ready Number
        /// </summary>
        public int getReadyNumber()
        {
            int num = -1;
            if (m_ReadyNumber.Count > 0)
            {
                num = m_ReadyNumber.Pop();
                m_UsedNumber.Add(num);
                return num;
            }
            num = HighestUsedNumber++;
            if (HighestUsedNumber == MaxNumber)
            {
                onCapacityIsFullEvent?.Invoke();
            }
            m_UsedNumber.Add(num);
            return num;
        }
        /// <summary>
        /// Push a number to ReadyPool
        /// </summary>
        public void pushToReady(int num)
        {
            if (m_UsedNumber.Contains(num))
            {
                m_ReadyNumber.Push(num);
                m_UsedNumber.Remove(num);
            }
            else
            {
                Debug.LogError("UseNumber doesn't contain " + num);
            }
        }

        public void copyFrom(NNextNumber src)
        {
            m_ReadyNumber = new Stack<int>(src.m_ReadyNumber);
            m_UsedNumber = new HashSet<int>(src.m_UsedNumber);
            setMaxNumber(src.MaxNumber);
            HighestUsedNumber = src.HighestUsedNumber;
        }
    }
}
