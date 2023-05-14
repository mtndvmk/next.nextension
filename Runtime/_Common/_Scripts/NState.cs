using System;
using UnityEngine;


namespace Nextension
{
    public class NState
    {
        private bool m_IsDown;
        private bool m_IsUp;
        private bool m_IsPressed;
        private bool m_IsDownable = true;
        private bool m_IsUpable = true;
        private float m_UpdatedTime;
        private Func<bool> m_CompareStateFunc;

        private void updateNState()
        {
            var current = Time.time;
            if (current > m_UpdatedTime)
            {
                m_UpdatedTime = current;
                m_IsPressed = m_CompareStateFunc();
                if (m_IsPressed)
                {
                    m_IsUpable = true;
                    if (m_IsDownable)
                    {
                        m_IsDownable = false;
                        m_IsDown = true;
                    }
                    else
                    {
                        m_IsDown = false;
                    }
                    m_IsUp = false;
                }
                else
                {
                    m_IsDownable = true;
                    if (m_IsUpable)
                    {
                        m_IsUpable = false;
                        m_IsUp = true;
                    }
                    else
                    {
                        m_IsUp = false;
                    }
                    m_IsDown = false;
                }
            }
        }

        public NState(Func<bool> pressedFunc)
        {
            m_CompareStateFunc = pressedFunc;
        }
        public bool getDown()
        {
            updateNState();
            return m_IsDown;
        }
        public bool getUp()
        {
            updateNState();
            return m_IsUp;
        }
        public bool getPress()
        {
            updateNState();
            return m_IsPressed;
        }
    }
}