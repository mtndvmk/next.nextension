using UnityEngine;

namespace Nextension.TextureProcess
{
    public partial class TextureProcessOperation : NOperation
    {
        public int OriginWidth { get; protected set; }
        public int OriginHeight { get; protected set; }
        public Texture Texture { get; protected set; }


        internal abstract class AbsProcessOperation
        {
            internal AbsProcessOperation(TextureSetting setting)
            {
                _setting = setting;
                if (_setting == null)
                {
                    _setting = new TextureSetting();
                }
            }
            public TextureProcessOperation Operation { get; internal set; } = new TextureProcessOperation();
            protected readonly TextureSetting _setting;
        }
    }
}
