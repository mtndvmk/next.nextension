namespace Nextension
{
    public enum DownloadOption : byte
    {
        None = 0,
        NotLoadOnDisk = 1,
        NotStoreOnDisk = 1 << 1,
    }
}