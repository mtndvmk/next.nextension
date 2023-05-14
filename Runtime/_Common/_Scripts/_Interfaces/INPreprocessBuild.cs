namespace Nextension
{
    public interface INPreprocessBuild
    {
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
