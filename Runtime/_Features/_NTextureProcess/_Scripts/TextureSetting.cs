using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Nextension.TextureProcess
{
    public class TextureSetting
    {
        public bool isLinear;
        public Vector2Int maxDimension;
        public int inSampleSize = 1; // using this if maxDimension not set 
        public bool isReadable;
        public bool isMipChain;
        public int anisoLevel;
        public string name;
        public bool forceNotAlpha;

        public Texture2D createTexture(int w, int h)
        {
            Texture2D tex;
            if (forceNotAlpha)
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
        public Texture2D createTexture(int w, int h, TextureFormat textureFormat)
        {
            Texture2D tex = new Texture2D(w, h, textureFormat, isMipChain, isLinear);
            tex.anisoLevel = anisoLevel;
            tex.name = name;
            return tex;
        }
        public Texture2D createTexture(int w, int h, GraphicsFormat graphicsFormat, TextureCreationFlags flags = TextureCreationFlags.None)
        {
            Texture2D tex = new Texture2D(w, h, graphicsFormat, flags);
            tex.anisoLevel = anisoLevel;
            tex.name = name;
            return tex;
        }
    }
}
