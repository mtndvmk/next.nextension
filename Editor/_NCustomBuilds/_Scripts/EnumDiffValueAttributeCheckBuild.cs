using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nextension.NEditor
{
    public class EnumDiffValueAttributeCheckBuild : IErrorCheckable
    {
        private EnumDiffValueAttributeCheckBuild() { }
        private static bool checkHasErrorOnBuild(out Exception e)
        {
            var types = NUtils.getCustomTypes();
            foreach (var type in types)
            {
                if (!type.IsEnum) continue;
                var attr = type.GetCustomAttribute(typeof(EnumDiffAttribute));
                if (attr != null)
                {
                    var enumArr = Enum.GetValues(type) as int[];

                    HashSet<int> hashset = new HashSet<int>();

                    for (int i = 0; i < enumArr.Length; ++i)
                    {
                        if (hashset.Contains(enumArr[i]))
                        {
                            e = new Exception($"Same value of enum: [{type}][{enumArr.GetValue(i)}:{enumArr[i]}]");
                            return true;
                        }
                        hashset.Add(enumArr[i]);
                    }
                }
            }
            e = null;
            return false;
        }
        public static void onPreprocessBuild()
        {
            if (checkHasErrorOnBuild(out var e))
            {
                throw e;
            }
        }
        public static void onEditorLoop()
        {
            if (checkHasErrorOnBuild(out var e))
            {
                throw e;
            }
        }
    }
}
