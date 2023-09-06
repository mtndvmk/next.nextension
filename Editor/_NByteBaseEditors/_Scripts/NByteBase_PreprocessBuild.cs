using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

namespace Nextension.NByteBase.Editor
{
    public class NByteBase_PreprocessBuild : IErrorCheckable
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
        static void onEditorLoop()
        {
            Exception exception = null;
            NByteBaseEditorUtils.clearCache();
            if (NByteBaseEditorUtils.checkHasErrorOnBuild(out exception))
            {
                throw exception;
            }
        }
    }
}