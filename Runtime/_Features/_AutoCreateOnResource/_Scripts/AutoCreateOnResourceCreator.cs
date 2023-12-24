#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Nextension
{
    internal class AutoCreateOnResourceCreator : IErrorCheckable, IAssetImportedCallback
    {
        static int Priority => 1;
        static void onLoadOrRecompiled()
        {
            var types = NUtils.getCustomTypes();
            foreach (var type in types)
            {
                if (!AutoCreateOnResourceUtils.checkValid(type)) continue;
                var attr = type.GetCustomAttribute<AutoCreateOnResourceAttribute>();
                if (attr == null) continue;
                var fileName = attr.getFileName(type);
                if (NAssetUtils.hasObjectOnResources(fileName)) continue;
                if (attr.useTypeName)
                {
                    var fullPath = NAssetUtils.createMainResourcesPath(fileName);
                    var dir = Path.GetDirectoryName(fullPath);
                    var so = NAssetUtils.findAssetAt(dir, type, out var foundPath);
                    if (so != null)
                    {
                        try
                        {
                            var error = AssetDatabase.ValidateMoveAsset(foundPath, fullPath);
                            if (string.IsNullOrEmpty(error))
                            {
                                AssetDatabase.MoveAsset(foundPath, fullPath);
                            }
                            else
                            {
                                Debug.LogError(error);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                        continue;
                    }
                }
                try
                {
                    NAssetUtils.createOnResource(type, fileName);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to create `{type.FullName}`, reason: {ex}");
                    continue;
                }
            }
            AssetDatabase.Refresh();
        }
        static void onAssetDeleted(string path)
        {
            if (!path.EndsWith(NAssetUtils.SCRIPTABLE_OBJECT_EXTENSION))
            {
                return;
            }
            if (path.Contains("/AutoCreated/[AutoCreated] "))
            {
                Debug.LogWarning($"Did you delete AutoCreateOnResource file?, please don't do that!\nPath: {path}");
                onLoadOrRecompiled();
            }
        }
    }
}
#endif