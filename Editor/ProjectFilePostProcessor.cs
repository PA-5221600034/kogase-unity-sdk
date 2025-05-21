using System;
using Kogase;
using UnityEditor;

public class ProjectFilePostProcessor  : AssetPostprocessor
{
    public static string OnGeneratedSlnSolution(string path, string content)
    {
        return content;
    }

    public static string OnGeneratedCSProject(string path, string content)
    {
        var x = UnityEditor.AssetDatabase.FindAssets($"t:Script {nameof(KogaseSDKRuntimeFolderPathNavigator)}");
        var unityAssetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(x[0]);
        var relativePath = unityAssetPath.StartsWith("Assets/")
            ? unityAssetPath.Substring("Assets/".Length)
            : unityAssetPath.StartsWith("Packages/")
                ? unityAssetPath.Substring("Packages/".Length)
                : unityAssetPath;
        

        string editorProjectConfigFindStr = $"<None Include=\"{relativePath}\\Editor\\Kogase.UnitySDK.Editor.asmdef\" />";
        string editorProjectConfigReplaceStr = string.Format("<None Include=\"{0}\\.editorconfig\" />\r\n    <None Include=\"{0}\\Editor\\Kogase.UnitySDK.Editor.asmdef\" />", relativePath);
        string runtimeProjectConfigFindStr = $"<None Include=\"{relativePath}\\Runtime\\Kogase.UnitySDK.asmdef\" />";
        string runtimeProjectConfigReplaceStr = string.Format("<None Include=\"{0}\\.editorconfig\" />\r\n    <None Include=\"{0}\\Runtime\\Kogase.UnitySDK.asmdef\" />", relativePath);

        if (path.IndexOf("Kogase.UnitySDK.Editor", StringComparison.Ordinal) > 0)
        {
           content = content.Replace(editorProjectConfigFindStr, editorProjectConfigReplaceStr);
        }
        else
        {
           content = content.Replace(runtimeProjectConfigFindStr, runtimeProjectConfigReplaceStr);
        }
        return content;
    }
}
