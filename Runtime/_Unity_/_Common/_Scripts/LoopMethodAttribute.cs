using System;

namespace Nextension
{
    public class LoopMethodAttribute : Attribute
    {
        internal NLoopType loopType = NLoopType.Update;
        public LoopMethodAttribute() { }
        public LoopMethodAttribute(NLoopType loopType) { this.loopType = loopType; }
    }
}
