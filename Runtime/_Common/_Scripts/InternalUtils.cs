using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    internal static class InternalUtils
    {
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        internal static void checkEditorMode(string note = null)
        {
            if (!Application.isPlaying)
            {
                var err = "Not support editormode";
                if (!string.IsNullOrEmpty(note))
                {
                    err += ": " + note;
                }
                throw new Exception(err);
            }
        }
    }
}
