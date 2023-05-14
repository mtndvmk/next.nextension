using System;

namespace Nextension.NByteBase.Editor
{
    public class NByteBaseEditorUtils
    {
        public static bool checkHasErrorOnBuild(out Exception error)
        {
            try
            {
                NByteBaseDatabase.init();
                error = null;
                return false;
            }
            catch (Exception e)
            {
                error = e;
                return true;
            }
        }

        public static void clearCache()
        {
            NByteBaseUtils.clearCache();
        }
    }
}