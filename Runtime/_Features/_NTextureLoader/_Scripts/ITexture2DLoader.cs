using System;

namespace Nextension.TextureLoader
{
    internal interface ITexture2DLoader
    {
        AbsProcessTask createProcessTask(byte[] inData, TextureSetting setting);
        AbsProcessTask createProcessTask(Uri uri, TextureSetting setting);
        AbsProcessTask createProcessTask(string url, TextureSetting setting);
    }
}
