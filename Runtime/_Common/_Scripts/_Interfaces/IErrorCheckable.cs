using System;

namespace Nextension
{
    public interface IErrorCheckable
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
        static void onEditorLoop()
        {
            // Put method in implement class (optional)
            // throw Exception to exit playmode and prevent enter playmode
        }
    }
}
