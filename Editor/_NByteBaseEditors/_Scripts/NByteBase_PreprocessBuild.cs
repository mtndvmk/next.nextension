using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

namespace Nextension.NByteBase.Editor
{
    public class NByteBase_PreprocessBuild : INPreprocessBuild
    {
        private NByteBase_PreprocessBuild() { }
        static void onPreprocessBuild()
        {
            Exception exception = null;
            NByteBaseEditorUtils.clearCache();
            if (NByteBaseEditorUtils.checkHasErrorOnBuild(out exception))
            {
                throw new BuildFailedException(exception);
            }
        }
        static void onReloadScript()
        {
            Exception exception = null;
            NByteBaseEditorUtils.clearCache();
            if (NByteBaseEditorUtils.checkHasErrorOnBuild(out exception))
            {
                Debug.LogError(exception);
            }
        }
    }
}