using System;

namespace Nextension.TextureLoader
{
    internal interface ITexture2DLoader
    {
        Texture2DLoaderOperation startLoad(byte[] inData, TextureSetting setting);
        Texture2DLoaderOperation startLoad(Uri uri, TextureSetting setting);
    }
}
