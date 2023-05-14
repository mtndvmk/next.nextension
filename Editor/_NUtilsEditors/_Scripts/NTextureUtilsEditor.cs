using System.Collections.Generic;
using UnityEditor;

public static class NTextureUtilsEditor
{
    public static IEnumerable<TextureImporter> findTextureInAsset()
    {
        var totalTextures = AssetDatabase.FindAssets("t:texture", new string[] { "Assets" });
        foreach (string guid in totalTextures)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("/Editor/"))
            {
                continue;
            }
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter)
            {
                yield return textureImporter;
            }
        }
    }
}