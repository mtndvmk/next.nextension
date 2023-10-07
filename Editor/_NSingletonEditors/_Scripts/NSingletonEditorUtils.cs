using Nextension;
using System;
using System.Reflection;

public class NSingletonEditorUtils : IErrorCheckable
{
    static void onPreprocessBuild()
    {
        if (checkHasErrorOnBuild(out var e))
        {
            throw e;
        }
    }
    static void onEditorLoop()
    {
        if (checkHasErrorOnBuild(out var e))
        {
            throw e;
        }
    }
    private static bool checkHasErrorOnBuild(out Exception exception)
    {
        try
        {
            var nSingletonType = typeof(NSingleton<>);
            var types = NUtils.getCustomTypes();

            foreach (var type in types)
            {
                if (type.IsGenericTypeDefinition) continue;
                var baseType = type.BaseType;
                if (baseType == null) continue;
                if (baseType.FullName.StartsWith(nSingletonType.FullName))
                {
                    var awakeType = type.GetTypeInfo().GetDeclaredMethod("Awake");
                    if (awakeType != null)
                    {
                        throw new Exception($"Please override \"onAwake()\" instead using \"Awake()\" at [{type.FullName}]");
                    }

                    var onDestroyType = type.GetMethod("OnDestroy");
                    if (onDestroyType != null)
                    {
                        throw new Exception("Please override onDestroy() instead using OnDestroy()");
                    }
                }
            }

            exception = null;
            return false;
        }
        catch (Exception e)
        {
            exception = e;
            return true;
        }
        finally
        {

        }
    }

}