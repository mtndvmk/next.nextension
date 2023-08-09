namespace Nextension
{
    public interface INPreprocessBuild
    {
        /// <summary>
        /// Callbacks with higher values are called before ones with lower values.
        /// </summary>
        static int Priority => 0;
        static void onPreprocessBuild()
        {
            // Put method in implement class (optional)
            // throw Exception to cancel build
        }
        static void onReloadScript()
        {
            // Put method in implement class (optional)
        }
    }
}
