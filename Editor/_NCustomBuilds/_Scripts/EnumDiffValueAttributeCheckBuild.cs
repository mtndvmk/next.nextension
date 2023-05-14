using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nextension.NEditor
{
    public class EnumDiffValueAttributeCheckBuild : INPreprocessBuild
    {
        private EnumDiffValueAttributeCheckBuild() { }
        private static bool checkHasErrorOnBuild(out Exception e)
        {
            var types = NUtils.getCustomTypes();
            foreach (var type in types)
            {
                if (!type.IsEnum) continue;
                var attr = type.GetCustomAttribute(typeof(EnumDiffAttribute));
                if (attr!= null)
                {
                    var enumArr = Enum.GetValues(type);
                    var list = new List<int>();
                    for (int i = 0; i < enumArr.Length; i++)
                    {
                        var enumInt = (int)Convert.ChangeType(enumArr.GetValue(i), TypeCode.Int32);
                        if (list.Contains(enumInt))
                        {
                            e = new Exception($"Same value of enum: [{type}][{enumArr.GetValue(i)}:{enumInt}]");
                            return true;
                        }
                        list.Add(enumInt);
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
        public static void onReloadScript()
        {
            if (checkHasErrorOnBuild(out var e))
            {
                throw e;
            }
        }
    }
}
