using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nextension
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class AutoCreateOnResourceAttribute : Attribute
    {
        private readonly string postName;
        public AutoCreateOnResourceAttribute()
        {
            postName = string.Empty;
        }
        public AutoCreateOnResourceAttribute(string postName)
        {
            this.postName = postName.Trim();
        }

        public string getFileName(Type type)
        {
            var fileName = "AutoCreated_" + type.Name;
            if (!string.IsNullOrEmpty(postName))
            {
                fileName += "_" + postName;
            }
            return fileName;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class PreloadResourceScriptableAttribute : Attribute
    {

    }
}
