using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Nextension.TextureLoader
{
    public class TextureSetting
    {
        public enum CompressType
        {
            LowQuality = 0,
            HighQuality = 1,
            NoCompress = 2,
        }
        public bool isLinear;
        public Vector2Int maxDimension;
        public int inSampleSize = 1; // using this if maxDimension not set 
        public bool isReadable;
        public bool isMipChain;
        public int anisoLevel;
        public string name;
        public bool forceNoAlpha;
        public CompressType compressType;

        public int getIntDimension(int originWidth, int originHeight)
        {
            if (maxDimension.x > 0 && maxDimension.y > 0)
            {
                return Mathf.Min(maxDimension.x, maxDimension.y);
            }
            else
            {
                return (int)((float)Mathf.Min(originWidth, originHeight) / inSampleSize);
            }
        }

        internal void apply(Texture2D tex)
        {
            if (isReadable || compressType != CompressType.NoCompress)
            {
                tex.Apply(false, false);
                NTextureUtils.compressWithLog(tex, compressType);
            }
            else
            {
                tex.Apply(false, true);
            }
        }

        internal Texture2D createTexture(int w, int h)
        {
            Texture2D tex;
            if (forceNoAlpha)
            {
                tex = new Texture2D(w, h, TextureFormat.RGB24, isMipChain, isLinear);
            }
            else
            {
                tex = new Texture2D(w, h, TextureFormat.RGBA32, isMipChain, isLinear);
            }
            tex.anisoLevel = anisoLevel;
            tex.name = name;
            return tex;
        }
        internal Texture2D createTexture(int w, int h, TextureFormat textureFormat)
        {
            Texture2D tex = new Texture2D(w, h, textureFormat, isMipChain, isLinear);
            tex.anisoLevel = anisoLevel;
            tex.name = name;
            return tex;
        }
        internal Texture2D createTexture(int w, int h, GraphicsFormat graphicsFormat, TextureCreationFlags flags = TextureCreationFlags.None)
        {
            Texture2D tex = new Texture2D(w, h, graphicsFormat, flags);
            tex.anisoLevel = anisoLevel;
            tex.name = name;
            return tex;
        }
    }
}
