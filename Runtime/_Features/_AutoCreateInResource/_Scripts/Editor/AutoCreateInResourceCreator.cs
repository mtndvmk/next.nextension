#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    internal class AutoCreateInResourceCreator : IOnCompiled, IOnAssetImported
    {
        static int Priority => 1;
        static async void onLoadOrRecompiled()
        {
            await TaskEditor.waitFrame();
            var types = NUtils.getCustomTypes();
            foreach (var type in types)
            {
                if (!AutoCreateInResourceUtils.checkValid(type)) continue;
                var attr = type.GetCustomAttribute<AutoCreateInResourceAttribute>();
                if (attr == null) continue;
                var fileName = attr.getFileName(type);
                if (NAssetUtils.hasObjectInMainResources(fileName)) continue;
                if (attr.useTypeName)
                {
                    var fullPath = NEditorAssetUtils.generateMainResourcesPath(fileName);
                    var dir = Path.GetDirectoryName(fullPath);
                    var so = NEditorAssetUtils.findAssetAt(dir, type, out var foundPath);
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
                    NEditorAssetUtils.createInMainResources(type, fileName);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("exists but can't be loaded"))
                    {
                        Debug.LogWarning($"Warning: Cannot create `{type.FullName}`, reason: {ex}");
                    }
                    else
                    {
                        Debug.LogError($"Failed to create `{type.FullName}`, reason: {ex}");
                    }
                    continue;
                }
            }
            AssetDatabase.Refresh();
        }
        static void onAssetDeleted(string path)
        {
            if (!path.EndsWith(NEditorAssetUtils.SCRIPTABLE_OBJECT_EXTENSION))
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