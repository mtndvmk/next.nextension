using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Nextension.TextureLoader
{
    public class Texture2DLoaderOperation : NOperation
    {
        public int OriginWidth { get; private set; }
        public int OriginHeight { get; private set; }
        public Texture2D Texture { get; private set; }

        public void setResult(Texture2D texture, int originWidth, int originHeight)
        {
            Texture = texture;
            OriginWidth = originWidth;
            OriginHeight = originHeight;
            innerFinalize();
        }
        public void setError(Exception e)
        {
            innerFinalize(e);
        }
    }

    internal abstract class AbsProcessor
    {
        public void initialize(TextureSetting setting)
        {
            _setting = setting;
            if (_setting == null)
            {
                _setting = new TextureSetting();
            }
            Operation = new Texture2DLoaderOperation();
        }
        public Texture2DLoaderOperation Operation { get; private set; }
        protected TextureSetting _setting;

        public abstract Task process(byte[] inData);
        public abstract Task process(Uri uri);
    }
}
