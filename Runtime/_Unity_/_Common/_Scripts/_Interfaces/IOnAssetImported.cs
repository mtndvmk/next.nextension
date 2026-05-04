namespace Nextension
{
    public interface IOnAssetImported
    {
        /// <summary>
        /// Callbacks with higher values are called before ones with lower values.
        /// </summary>
        static int Priority => 0;
        static void onAssetImported(string path)
        {
            // Put method in implement class (optional)
        }
        static void onAssetDeleted(string path)
        {
            // Put method in implement class (optional)
        }
        static void onAssetMoved(string path)
        {
            // Put method in implement class (optional)
        }
        static void onPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // Put method in implement class (optional)
        }
    }
}
