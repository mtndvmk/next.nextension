namespace Nextension
{
    public enum DownloadOption
    {
        None = 0,
        ForceNewDownload = 1,
        NotStoreOnDisk = 1 << 1,
    }
}