using System;

namespace Nextension
{
    [Serializable]
        public struct RectValue<T>
        {
            public T left;
            public T right;
            public T top;
            public T bottom;
        }
}
