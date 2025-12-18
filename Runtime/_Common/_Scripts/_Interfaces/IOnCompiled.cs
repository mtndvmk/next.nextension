namespace Nextension
{
    public interface IOnCompiled
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
        static void onLoadOrRecompiled()
        {
            // Put method in implement class (optional)
        }
    }
}
