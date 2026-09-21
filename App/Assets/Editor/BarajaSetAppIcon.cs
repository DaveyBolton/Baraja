using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

/// <summary>
/// Sets the app/launcher icon to the Ruby Heart card art, run via:
/// Unity -batchmode -executeMethod BarajaSetAppIcon.Run -quit
/// One-shot, not part of either scene builder - the icon is a project
/// setting (ProjectSettings/ProjectSettings.asset), not scene content.
/// </summary>
public static class BarajaSetAppIcon
{
    public static void Run()
    {
        string path = "Assets/Icons/AppIcon_RubyHeart.png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer != null && importer.textureType != TextureImporterType.Default)
        {
            importer.textureType = TextureImporterType.Default;
            importer.SaveAndReimport();
        }

        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (icon == null)
        {
            Debug.LogError("BarajaSetAppIcon: could not load " + path);
            return;
        }

        var icons = new Texture2D[] { icon };
        PlayerSettings.SetIcons(NamedBuildTarget.Unknown, icons, IconKind.Any);
        PlayerSettings.SetIcons(NamedBuildTarget.Standalone, icons, IconKind.Any);
        PlayerSettings.SetIcons(NamedBuildTarget.Android, icons, IconKind.Any);
        PlayerSettings.SetIcons(NamedBuildTarget.iOS, icons, IconKind.Any);

        AssetDatabase.SaveAssets();
        Debug.Log("BarajaSetAppIcon: app icon set to Ruby Heart.");
    }
}
